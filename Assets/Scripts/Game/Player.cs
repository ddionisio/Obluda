using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : Character {
    public enum EquipHand {
        Left,
        Right,

        NumHands
    }

    private Inventory mInventory = new Inventory();

    public Transform equipHolder;

    private PlayerController mController;

    private Dictionary<string, EquipBase> mEquips = null;

    private EquipBase[] mEquipHands = new EquipBase[(int)EquipHand.NumHands];

    public override Vector3 position { get { return controller.moveController.transform.position; } }
    public override float radius { get { return controller.moveController.radius; } }

    public PlayerController controller { get { return mController; } }

    public Inventory inventory { get { return mInventory; } }

    public EquipBase EquipHandGet(EquipHand hand) {
        return mEquipHands[(int)hand];
    }

    public void EquipHandSet(EquipHand hand, int itmID) {
        Item itm = ItemManager.instance.GetItem(itmID);
        EquipHandSet(hand, itm);
    }

    public void EquipHandSet(EquipHand hand, Item itm) {
        int ind = (int)hand;

        EquipBase equipDat;
        if(itm != null && mEquips.TryGetValue(itm.equipRef, out equipDat)) {
            if(mEquipHands[ind] != equipDat) {
                if(mEquipHands[ind] != null) {
                    mEquipHands[ind].ActionCancel();
                    mEquipHands[ind].gameObject.SetActive(false); //TODO: wait for action to cancel?
                }

                mEquipHands[ind] = equipDat;
                mEquipHands[ind]._setItemID(itm.id);

                DoEquip(ind);
            }
        }
        else if(mEquipHands[ind] != null) {
            mEquipHands[ind].ActionCancel();
            mEquipHands[ind].gameObject.SetActive(false); //TODO: wait for action to cancel?
            mEquipHands[ind] = null;
        }
    }

    public bool EquipHandInProgress(Player.EquipHand hand) {
        EquipBase equipDat = mEquipHands[(int)hand];
        return equipDat != null ? equipDat.ActionInProgress() : false;
    }

    public bool EquipHandsInProgress() {
        for(int i = 0; i < mEquipHands.Length; i++) {
            if(mEquipHands[i] != null && mEquipHands[i].ActionInProgress())
                return true;
        }

        return false;
    }
    
    /// <summary>
    /// Call this after Awake such that ItemManager has been initialized
    /// </summary>
    public void Load() {
        UserData ud = UserData.instance;

        //load stats, etc.

        //load current equip, note that this will only set
        //the reference, they will be activated during SpawnStart
        for(int i = 0; i < mEquipHands.Length; i++) {
            int itmID = ud.GetInt("eq" + i, ItemManager.InvalidID);
            Item itm = ItemManager.instance.GetItem(itmID);

            EquipBase equipDat;
            if(itm != null && mEquips.TryGetValue(itm.equipRef, out equipDat)) {
                mEquipHands[i] = equipDat;
                mEquipHands[i]._setItemID(itm.id);
            }
            else {
                mEquipHands[i] = null;
            }
        }
        //

        mInventory.Load();
    }

    public void Save() {
        UserData ud = UserData.instance;

        //save stats, etc.

        //save current equip
        for(int i = 0; i < mEquipHands.Length; i++) {
            ud.SetInt("eq" + i, mEquipHands[i] != null ? mEquipHands[i].itemID : ItemManager.InvalidID);
        }

        mInventory.Save();
    }

    public void EquipCancelActions() {
        for(int i = 0, max = mEquipHands.Length; i < max; i++) {
            EquipBase equip = mEquipHands[i];
            if(equip != null)
                equip.ActionCancel();
        }
    }

    protected override void OnHPChanged(Stat s, float delta) {
        base.OnHPChanged(s, delta);

        //HUD update
    }

    protected override void StateChanged() {
        base.StateChanged();

        switch((EntityState)state) {
            case EntityState.Hit:
                EquipCancelActions();
                break;

            case EntityState.Dead:
                EquipCancelActions();
                break;
        }
    }

    void DeInit() {
        state = StateInvalid;

        InputManager input = Main.instance != null ? Main.instance.input : null;

        if(input != null) {
            input.AddButtonCall(0, InputAction.Menu, OnInputMenu);
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        DeInit();

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        //dealloc here
        DeInit();

        base.OnDestroy();
    }

    public override void SpawnFinish() {
        //initialize current equips
        for(int i = 0; i < mEquipHands.Length; i++) {
            DoEquip(i);
        }

        //TEST
        EquipHandSet(EquipHand.Left, 1);

        base.SpawnFinish();
    }

    protected override void SpawnStart() {
        base.SpawnStart();

        //initialize some things
    }

    protected override void Awake() {
        base.Awake();

        //initialize variables
        autoSpawnFinish = true;

        mController = GetComponent<PlayerController>();

        mEquips = new Dictionary<string, EquipBase>(equipHolder.childCount);

        foreach(Transform equip in equipHolder) {
            EquipBase equipDat = equip.GetComponent<EquipBase>();
            if(equipDat != null) {
                equipDat.gameObject.SetActive(false);
                equipDat.source = this;
                mEquips.Add(equip.name, equipDat);
            }
        }
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
        InputManager input = Main.instance.input;

        input.AddButtonCall(0, InputAction.Menu, OnInputMenu);
    }

    void Update() {
        for(int i = 0, max = mEquipHands.Length; i < max; i++) {
            EquipBase equip = mEquipHands[i];
            if(equip != null)
                equip.ActionUpdate();
        }
    }

    void OnInputMenu(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            UIModalManager ui = UIModalManager.instance;

            if(!ui.ModalIsInStack("pause")) {
                ui.ModalOpen("pause");
            }
        }
    }

    void OnApplicationFocus(bool focus) {
        if(!focus) {
#if UNITY_EDITOR
#else
            UIModalManager ui = UIModalManager.instance;

            if(!ui.ModalIsInStack("pause")) {
                ui.ModalOpen("pause");
            }
#endif
        }
    }

    void DoEquip(int ind) {
        bool isLeft = (EquipHand)ind == EquipHand.Left;
        EquipBase equipDat = mEquipHands[ind];

        if(equipDat != null) {
            equipDat.gameObject.SetActive(true);

            Vector3 s = equipDat.transform.localScale;
            s.x = isLeft ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
            equipDat.transform.localScale = s;

            equipDat.Equip(isLeft);
        }
    }
}
