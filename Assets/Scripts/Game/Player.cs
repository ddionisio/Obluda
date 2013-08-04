using UnityEngine;
using System.Collections;

public class Player : EntityBase {
    private Inventory mInventory = new Inventory();

    public Transform equipHolder;

    public Inventory inventory { get { return mInventory; } }

    public void Load() {
        //load stats, etc.

        mInventory.Load();
    }

    public void Save() {
        //save stats, etc.

        mInventory.Save();
    }

    protected override void StateChanged() {
        
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
        //start ai, player control, etc
        state = (int)EntityState.Normal;
    }

    protected override void SpawnStart() {
        //initialize some things
    }

    protected override void Awake() {
        base.Awake();

        //initialize variables
        autoSpawnFinish = true;
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
        InputManager input = Main.instance.input;

        input.AddButtonCall(0, InputAction.Menu, OnInputMenu);
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
}
