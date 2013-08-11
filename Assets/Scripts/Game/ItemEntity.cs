using UnityEngine;
using System.Collections;

public class ItemEntity : EntityBase {
    const string SerialItemKeyBase = "_sItmR";

    [SerializeField]
    int startItemId = -1;

    [SerializeField]
    bool removePersist = true; //if this is a scene item, persist after pickup?

    private int mItemId = -1;
    private Item mItemRef;

    public int itemId { get { return mItemId; } }

    /// <summary>
    /// NOTE: This is only used by ItemManager
    /// </summary>
    [System.NonSerialized]
    public int _spawnId = 0;

    public Item itemRef {
        get { return mItemRef; }
        set {
            if(mItemRef != value) {
                mItemId = value != null ? value.id : -1;
                mItemRef = value;
            }
        }
    }

    public void Collect(PlayerController pc) {
        if(mItemId == -1) {
            Debug.LogError("Invalid item id, can't collect!");
            return;
        }

        pc.player.inventory.Add(mItemId);
        Release();
    }

    public override void Release() {
        //if persistent, remove save reference and mark as 'removed'
        if(!ItemManager.instance.RemoveItemSpawnData(this)) {
            if(serializer != null) {
                serializer.SetValue("removed", 1, removePersist);
            }
        }

        base.Release();
    }

    protected override void OnDespawned() {
        if(mItemRef != null) {
            //reset stuff here
            mItemRef = null;
        }

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        //dealloc here
        mItemRef = null;

        base.OnDestroy();
    }

    public override void SpawnFinish() {
        //start ai, player control, etc
    }

    protected override void SpawnStart() {
        //initialize some things
        if(startItemId != -1) { //this means it is placed on the scene
            if(mItemRef == null)
                mItemRef = ItemManager.instance.GetItem(startItemId);
        }
    }

    protected override void Awake() {
        base.Awake();

        //check if this is marked as 'removed' from save reference
        //this should only be for items placed in the editor, not from pool!
        if(serializer != null) {
            if(serializer.GetValue("removed", 0) != 0) {
                DestroyImmediate(gameObject);
                return;
            }
        }

        //initialize variables
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();
                
        //initialize variables from other sources (for communicating with managers, etc.)
        if(startItemId != -1)
            itemRef = ItemManager.instance.GetItem(startItemId);
    }
}
