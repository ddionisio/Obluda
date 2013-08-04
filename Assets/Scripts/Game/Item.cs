//using UnityEngine;
//using System.Collections;
using System.Collections.Generic;

public enum ItemType {
    None,
    Equip,
    Useable,
    Seed,
    Misc //also quest and other junk
}

[System.Serializable]
public class Item {
    public int id;

    public ItemType type;

    public string nameKey; //key to localization
    public string descKey; //key to localization

    public string iconRef; //ngui sprite ref
    public string equipRef; //equipment reference from player
    public string spawnRef; //ref in world spawn for drops

    public int value;
    public int maxQuantity;

    //flags
    public bool important; //for pickups, save it in state, otherwise it is removed when exiting scene

    public static int numTypes { get { return System.Enum.GetValues(typeof(ItemType)).Length; } }

    public ItemData CreateData() {
        return new ItemData(id);
    }
}

public class ItemData {
    public int id = -1;
    public int quantity = 0;

    public Item item {
        get {
            return ItemManager.instance.GetItem(id);
        }
    }

    public static void Remove(UserData ud, string header) {
        ud.Delete(header + "_id");
        ud.Delete(header + "_q");
    }

    public ItemData(int id) {
        this.id = id;
        quantity = 1;
    }

    public ItemData(UserData ud, string header) {
        id = ud.GetInt(header + "_id", -1);
        quantity = ud.GetInt(header + "_q", 0);
    }

    public void Save(UserData ud, string header) {
        ud.SetInt(header + "_id", id);
        ud.SetInt(header + "_q", quantity);
    }
}