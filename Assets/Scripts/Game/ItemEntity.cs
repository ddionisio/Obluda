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
        //first check if this is spawned, then just remove that reference.
        if(!ItemManager.instance.RemoveItemSpawnData(this)) {
            if(removePersist && serializer != null) {
                serializer.MarkRemove();
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
        InitStartItemRef();
    }

    protected override void Awake() {
        base.Awake();

        //initialize variables
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();
                
        //initialize variables from other sources (for communicating with managers, etc.)
        InitStartItemRef();
    }

    void InitStartItemRef() {
        if(startItemId != -1) {
            itemRef = ItemManager.instance.GetItem(startItemId);
            if(mItemRef != itemRef)
                mItemRef = itemRef;
        }
    }
}
