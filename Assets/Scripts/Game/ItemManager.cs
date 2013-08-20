using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Put this as child in core object
/// </summary>
public class ItemManager : MonoBehaviour {
    public const string spawnHolderTag = "ItemSpawnHolder";
    public const int InvalidID = 0;
    public TextAsset config;

    private static ItemManager mInstance;

    private Dictionary<int, Item> mItems;
    private PoolController mPool;

    private Transform mSpawnHolder;

    public static ItemManager instance { get { return mInstance; } }

    public Transform spawnHolder { get { if(mSpawnHolder == null) { SetSpawnHolder(); } return mSpawnHolder; } }

    public Item GetItem(int id) {
        Item itm = null;
        if(!mItems.TryGetValue(id, out itm))
            Debug.LogError("Unable to find item with id: " + id);

        return itm;
    }

    public ItemEntity SpawnItem(int id, Vector3 pos, Quaternion rot) {
        Item itm = GetItem(id);
        if(itm != null) {
            Transform t = mPool.Spawn(itm.spawnRef, itm.nameKey, mSpawnHolder, pos, rot);
            if(t != null) {
                ItemEntity itmEnt = t.GetComponent<ItemEntity>();
                itmEnt.itemRef = itm;

                return itmEnt;
            }
        }

        return null;
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

    ////RootBroadcastMessage("SceneChange", mSceneToLoad, SendMessageOptions.DontRequireReceiver);

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;
    }

    void OnDisable() {
        mSpawnHolder = null;
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

    void SetSpawnHolder() {
        GameObject spawnHolderGO = GameObject.FindGameObjectWithTag(spawnHolderTag);
        if(spawnHolderGO == null) {
            spawnHolderGO = new GameObject("itemSpawns");
            spawnHolderGO.tag = spawnHolderTag;
        }

        mSpawnHolder = spawnHolderGO.transform;
    }
}
