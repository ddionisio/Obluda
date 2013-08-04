using UnityEngine;
using System.Collections;

public class ItemEntity : EntityBase {
    [SerializeField]
    int startItemId = -1;

    private int mItemId = -1;
    private Item mItemRef;

    public int itemId { get { return mItemId; } }

    public Item itemRef {
        get { return mItemRef; }
        set {
            if(mItemRef != value) {
                mItemId = value != null ? value.id : -1;
                mItemRef = value;
            }
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        mItemRef = null;

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
        if(mItemId != -1 && mItemRef == null)
            mItemRef = ItemManager.instance.GetItem(mItemId);
    }

    protected override void Awake() {
        base.Awake();

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
