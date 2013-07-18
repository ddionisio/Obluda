using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    private Player mPlayer;
    private FPCameraController mCameraCtrl;
    private FPMoveController mMoveCtrl;
    private CursorAutoLock mCursorAutoLock;

    bool mInputEnabled = false;

    public bool inputEnabled {
        get { return mInputEnabled; }
        set {
            if(mInputEnabled != value) {
                mInputEnabled = value;

                mCameraCtrl.inputEnabled = mInputEnabled;
                mMoveCtrl.inputEnabled = mInputEnabled;

                //our own inputs
                InputManager input = Main.instance != null ? Main.instance.input : null;
                if(input != null) {
                    if(mInputEnabled) {
                    }
                    else {
                    }
                }
            }
        }
    }

    void Awake() {
        mCameraCtrl = GetComponentInChildren<FPCameraController>();
        mMoveCtrl = GetComponentInChildren<FPMoveController>();
        mCursorAutoLock = GetComponent<CursorAutoLock>();

        //set input keys to these controllers
        mCameraCtrl.lookInputY = InputAction.LookY;

        mMoveCtrl.moveInputX = InputAction.MoveX;
        mMoveCtrl.moveInputY = InputAction.MoveY;
        mMoveCtrl.turnInput = InputAction.LookX;
        mMoveCtrl.jumpInput = InputAction.Jump;

        mCursorAutoLock.cursorLockCallback += OnCursorLock;
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    #region player messages

    void EntityStart(EntityBase ent) {
        mPlayer = ent as Player;

        //hook up callbacks
        mPlayer.setStateCallback += OnPlayerSetState;
    }

    void OnPlayerSetState(EntityBase ent, int state) {
        switch(state) {
            case Player.StateNormal:
                if(mCursorAutoLock.isLocked)
                    inputEnabled = true;
                break;

            case Player.StateDead:
                inputEnabled = false;
                break;

            case Player.StateInvalid:
                inputEnabled = false;
                break;
        }
    }

    #endregion

    void OnCursorLock(bool locked) {
        if(locked) {
            switch(mPlayer.state) {
                case Player.StateNormal:
                    inputEnabled = true;
                    break;
            }
        }
        else {
            inputEnabled = false;
        }
    }
}
