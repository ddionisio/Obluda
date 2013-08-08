using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

//put this under levelCore object
public class ItemManager : MonoBehaviour {
    public const int InvalidID = 0;
    public TextAsset config;

    private static ItemManager mInstance;

    [System.Serializable]
    private struct SpawnSave {
        public string scene;
        public int id;
        public Vector3 pos;
        public Quaternion rot;
    }

    private Dictionary<int, Item> mItems;
    private PoolController mPool;

    public static ItemManager instance { get { return mInstance; } }

    public Item GetItem(int id) {
        Item itm = null;
        if(!mItems.TryGetValue(id, out itm))
            Debug.LogError("Unable to find item with id: " + id);

        return itm;
    }

    ItemEntity _SpawnItem(int id, Vector3 pos, Quaternion rot, bool spawnSave) {
        Item itm = GetItem(id);
        if(itm != null) {
            Transform t = mPool.Spawn(itm.spawnRef, itm.nameKey, null, pos);
            if(t != null) {
                t.rotation = rot;

                ItemEntity itmEnt = t.GetComponent<ItemEntity>();
                itmEnt.itemRef = itm;

                //save spawn, only if maxSpawnSave > 0
                if(spawnSave)
                    SpawnSaveAdd(itmEnt);

                return itmEnt;
            }
        }

        return null;
    }

    public ItemEntity SpawnItem(int id, Vector3 pos, Quaternion rot) {
        return _SpawnItem(id, pos, rot, true);
    }

    public void LoadItems(SortedDictionary<int, ItemData> items, string header) {
        List<ItemData> itmList = new List<ItemData>();
        
        LoadItems(itmList, header);

        items.Clear();

        foreach(ItemData itmDat in itmList) {
            items.Add(itmDat.id, itmDat);
        }
    }

    public void LoadItems(List<ItemData> items, string header) {
        UserData ud = UserData.instance;

        items.Clear();

        int count = ud.GetInt(header, 0);

        for(int i = 0; i < count; i++) {
            string key = header + i;
            items.Add(new ItemData(ud, key));
        }
    }

    public void SaveItems(SortedDictionary<int, ItemData> items, string header) {
        List<ItemData> itmList = new List<ItemData>(items.Values);
        SaveItems(itmList, header);
    }

    public void SaveItems(List<ItemData> items, string header) {
        UserData ud = UserData.instance;

        int count = ud.GetInt(header, 0);

        for(int i = 0; i < items.Count; i++) {
            string key = header + i;
            items[i].Save(ud, key);
        }

        //delete excess items from previous
        for(int i = items.Count; i < count; i++) {
            ItemData.Remove(ud, header + i);
        }

        ud.SetInt(header, items.Count);
    }

    public bool RemoveItemSpawnSave(ItemEntity itmEnt) {
        if(itmEnt._spawnId != 0) {
            SpawnSaveRemove(itmEnt.itemId, itmEnt._spawnId);
            itmEnt._spawnId = 0;
            return true;
        }

        return false;
    }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

            //get item data
            fastJSON.JSON.Instance.Parameters.UseExtensions = true;
            List<Item> items = fastJSON.JSON.Instance.ToObject<List<Item>>(config.text);

            mItems = new Dictionary<int, Item>(items.Count);

            foreach(Item item in items) {
                mItems.Add(item.id, item);
            }

            mPool = GetComponent<PoolController>();
        }
    }

    // Use this for initialization
    void Start() {
        SpawnSavePopulateCurrentScene();
    }


    #region spawn save

    List<SpawnSave> SpawnSaveGet(int itmID) {
        List<SpawnSave> ret = null;

        string dat = UserData.instance.GetString("_itm_" + itmID);

        if(!string.IsNullOrEmpty(dat)) {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(System.Convert.FromBase64String(dat));
            ret = (List<SpawnSave>)bf.Deserialize(ms);
        }

        return ret;
    }

    void SpawnSaveSet(int itmID, List<SpawnSave> dat) {
        if(dat != null && dat.Count > 0) {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, dat);
            UserData.instance.SetString("_itm_" + itmID, System.Convert.ToBase64String(ms.GetBuffer()));
        }
        else {
            UserData.instance.Delete("_itm_" + itmID);
        }
    }

    void SpawnSaveRemove(int itmID, int spawnID) {
        List<SpawnSave> dat = SpawnSaveGet(itmID);
        if(dat != null && dat.Count > 0) {
            for(int i = 0; i < dat.Count; i++) {
                if(dat[i].id == spawnID) {
                    dat.RemoveAt(i);
                    break;
                }
            }

            SpawnSaveSet(itmID, dat);
        }
    }

    void SpawnSaveAdd(ItemEntity itmEnt) {
        Item itm = itmEnt.itemRef;

        if(itm.maxSpawnSave > 0) {
            List<SpawnSave> dat = SpawnSaveGet(itm.id);

            if(dat == null)
                dat = new List<SpawnSave>();
            else if(dat.Count == itm.maxSpawnSave) {
                //remove oldest
                dat.RemoveAt(0);
            }

            itmEnt._spawnId = itmEnt.GetInstanceID();

            dat.Add(new SpawnSave() { scene = Application.loadedLevelName, id = itmEnt._spawnId, pos = itmEnt.transform.position, rot = itmEnt.transform.rotation });

            SpawnSaveSet(itm.id, dat);
        }
    }

    void SpawnSavePopulateCurrentScene() {
        //go through item ids and load items if they match the scene
        foreach(KeyValuePair<int, Item> pair in mItems) {
            List<SpawnSave> dat = SpawnSaveGet(pair.Key);
            if(dat != null) {
                foreach(SpawnSave spawnDat in dat) {
                    if(spawnDat.scene == Application.loadedLevelName) {
                        ItemEntity itmSpawned = _SpawnItem(pair.Key, spawnDat.pos, spawnDat.rot, false);
                        if(itmSpawned != null)
                            itmSpawned._spawnId = spawnDat.id;
                    }
                }
            }
        }
    }

    #endregion
}
