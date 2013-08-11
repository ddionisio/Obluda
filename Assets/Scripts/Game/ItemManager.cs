using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

/// <summary>
/// Put this as child in core object
/// </summary>
public class ItemManager : MonoBehaviour {
    public const string spawnHolderTag = "ItemSpawnHolder";
    public const int InvalidID = 0;
    public TextAsset config;

    private static ItemManager mInstance;

    [System.Serializable]
    private struct SpawnSave {
        public int itmID;
        public int spwnID;
        public Vector3 pos;
        public Quaternion rot;
    }

    private bool mStarted = false;

    private Dictionary<int, Item> mItems;
    private PoolController mPool;

    private Transform mSpawnHolder;
    private Dictionary<int, ItemEntity> mSpawnedItemSaves = new Dictionary<int,ItemEntity>(); //[spawn id, item entity]

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
            Transform t = mPool.Spawn(itm.spawnRef, itm.nameKey, mSpawnHolder, pos);
            if(t != null) {
                t.rotation = rot;

                ItemEntity itmEnt = t.GetComponent<ItemEntity>();
                itmEnt.itemRef = itm;

                //save spawn, only if maxSpawnSave > 0
                if(spawnSave && itm.maxSpawnSave > 0) {
                    SceneSerializer ss = itmEnt.GetComponent<SceneSerializer>();
                    if(ss == null)
                        ss = itmEnt.gameObject.AddComponent<SceneSerializer>();

                    ss.__GenNewID();

                    mSpawnedItemSaves.Add(ss.id, itmEnt);
                }

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

    /// <summary>
    /// Call this during ItemEntity Release when we want to explicitly say that item will no longer be spawned when the scene is reloaded.
    /// Returns true if item spawn has been deleted.
    /// This will remove all persistent data for the item.
    /// </summary>
    public bool RemoveItemSpawnData(ItemEntity itmEnt) {
        SceneSerializer ss = itmEnt.GetComponent<SceneSerializer>();
        if(ss != null) {
            if(mSpawnedItemSaves.ContainsKey(ss.id)) {
                mSpawnedItemSaves.Remove(ss.id);
                ss.DeleteAllValues();

                return true;
            }
        }

        return false;
    }

    ////RootBroadcastMessage("SceneChange", mSceneToLoad, SendMessageOptions.DontRequireReceiver);

    void OnDisable() {
        //level unload, save spawns
        if(mStarted) {
            SpawnSaveCurrentScene();

            mSpawnedItemSaves.Clear();
        }
    }

    void OnEnable() {
        //new level loaded
        if(mStarted) {
            SetSpawnHolder();
            SpawnPopulateCurrentScene();
        }
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
        mStarted = true;
        SetSpawnHolder();
        SpawnPopulateCurrentScene();
    }

    void SetSpawnHolder() {
        GameObject spawnHolderGO = GameObject.FindGameObjectWithTag(spawnHolderTag);
        if(spawnHolderGO == null) {
            spawnHolderGO = new GameObject("itemSpawns");
            spawnHolderGO.tag = spawnHolderTag;
        }

        mSpawnHolder = spawnHolderGO.transform;
    }
        
    #region spawn save

    List<SpawnSave> SpawnSaveGet() {
        List<SpawnSave> ret = null;

        string dat = UserData.instance.GetString("il_" + Application.loadedLevelName);

        if(!string.IsNullOrEmpty(dat)) {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(System.Convert.FromBase64String(dat));
            ret = (List<SpawnSave>)bf.Deserialize(ms);
        }

        return ret;
    }

    void SpawnSaveSet(List<SpawnSave> dat) {
        if(dat != null && dat.Count > 0) {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, dat);
            UserData.instance.SetString("il_" + Application.loadedLevelName, System.Convert.ToBase64String(ms.GetBuffer()));
        }
        else {
            UserData.instance.Delete("il_" + Application.loadedLevelName);
        }
    }

    void SpawnPopulateCurrentScene() {
        mSpawnedItemSaves.Clear();

        //go through item ids and load items if they match the scene
        List<SpawnSave> dats = SpawnSaveGet();
        if(dats != null) {
            foreach(SpawnSave dat in dats) {
                ItemEntity itmSpawned = _SpawnItem(dat.itmID, dat.pos, dat.rot, false);
                if(itmSpawned != null) {
                    SceneSerializer ss = itmSpawned.GetComponent<SceneSerializer>();
                    if(ss == null)
                        ss = itmSpawned.gameObject.AddComponent<SceneSerializer>();

                    ss.__EditorSetID(dat.spwnID);

                    mSpawnedItemSaves.Add(ss.id, itmSpawned);
                }
            }
        }
    }

    void SpawnSaveCurrentScene() {
        List<SpawnSave> dats = new List<SpawnSave>();

        foreach(KeyValuePair<int, ItemEntity> pair in mSpawnedItemSaves) {
            dats.Add(new SpawnSave() { spwnID = pair.Key, itmID = pair.Value.itemId, pos = pair.Value.transform.position, rot = pair.Value.transform.rotation });
        }

        SpawnSaveSet(dats);
    }

    #endregion
}
