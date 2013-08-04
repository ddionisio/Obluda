using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory {
    public delegate void OnItemChange(Inventory inv, ItemData itmDat);

    public event OnItemChange itemAddCallback;
    public event OnItemChange itemRemoveCallback;

    private SortedDictionary<int, ItemData> mItems = new SortedDictionary<int, ItemData>();

    public void Add(int id) {
        ItemData itmDat = null;

        if(mItems.TryGetValue(id, out itmDat)) {
            itmDat.quantity = Mathf.Clamp(itmDat.quantity + 1, 0, itmDat.item.maxQuantity);
        }
        else {
            Item itm = ItemManager.instance.GetItem(id);
            if(itm != null) {
                itmDat = itm.CreateData();
                mItems.Add(id, itmDat);
            }
        }

        if(itmDat != null && itemAddCallback != null) {
            itemAddCallback(this, itmDat);
        }
    }

    public void Remove(int id) {
        ItemData itmDat;
        if(mItems.TryGetValue(id, out itmDat)) {
            itmDat.quantity = Mathf.Clamp(itmDat.quantity - 1, 0, itmDat.item.maxQuantity);

            if(itmDat.quantity <= 0)
                mItems.Remove(id);

            if(itemRemoveCallback != null) {
                itemRemoveCallback(this, itmDat);
            }
        }
    }

    public int GetCount(int id) {
        return mItems.ContainsKey(id) ? mItems[id].quantity : 0;
    }

    public ItemData GetItemData(int id) {
        ItemData ret = null;
        mItems.TryGetValue(id, out ret);
        return ret;
    }

    public Item GetItem(int id) {
        ItemData dat = GetItemData(id);
        return dat != null ? dat.item : null;
    }

    public List<ItemData> GetItems(ItemType type) {
        List<ItemData> ret = new List<ItemData>();

        foreach(KeyValuePair<int, ItemData> pair in mItems) {
            if(pair.Value.item.type == type)
                ret.Add(pair.Value);
        }

        return ret;
    }

    public void Load() {
        ItemManager itmMgr = ItemManager.instance;

        itmMgr.LoadItems(mItems, "inv");
    }

    public void Save() {
        ItemManager itmMgr = ItemManager.instance;

        itmMgr.SaveItems(mItems, "inv");
    }
}
