using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public float interactDistance;
    public LayerMask interactLayerCast; //for solid checks
    public LayerMask interactLayer; //layers of objects that are interactive (this should be a mask within interactLayerCast)
    public float interactCheckDelay = 0.15f;

    private Player mPlayer;
    private FPController mMoveCtrl;
    private CursorAutoLock mCursorAutoLock;

    private bool mReticleActive = false;
    private bool mReticleIsRunning = false;
    private WaitForSeconds mReticleWaitDelay;
    private Interactor mReticleCurInteract = null;

    private bool mInputEnabled = false;

    public FPController moveController { get { return mMoveCtrl; } }

    public bool reticleActive {
        get { return mReticleActive; }
        set {
            if(mReticleActive != value) {
                mReticleActive = value;

                if(mReticleActive) {
                    if(!mReticleIsRunning)
                        StartCoroutine("DoReticleCheck");
                }
                else {
                    mReticleIsRunning = false;

                    if(mReticleCurInteract != null) {
                        mReticleCurInteract.isPointedAt = false;
                        mReticleCurInteract = null;
                    }

                    if(Reticle.instance != null)
                        Reticle.instance.state = Reticle.instance.stateStart;
                }
            }
        }
    }

    public bool inputEnabled {
        get { return mInputEnabled; }
        set {
            if(mInputEnabled != value) {
                mInputEnabled = value;

                mMoveCtrl.inputEnabled = mInputEnabled;

                //our own inputs
                InputManager input = Main.instance != null ? Main.instance.input : null;
                if(input != null) {
                    if(mInputEnabled) {
                        input.AddButtonCall(0, InputAction.Action, OnInputAction);
                    }
                    else {
                        input.RemoveButtonCall(0, InputAction.Action, OnInputAction);
                    }
                }
            }
        }
    }

    void Awake() {
        mMoveCtrl = GetComponentInChildren<FPController>();
        mCursorAutoLock = GetComponent<CursorAutoLock>();

        //set input keys to these controllers
        
        mMoveCtrl.moveInputX = InputAction.MoveX;
        mMoveCtrl.moveInputY = InputAction.MoveY;
        mMoveCtrl.lookInputX = InputAction.LookX;
        mMoveCtrl.lookInputY = InputAction.LookY;
        mMoveCtrl.jumpInput = InputAction.Jump;

        mCursorAutoLock.cursorLockCallback += OnCursorLock;

        mReticleWaitDelay = new WaitForSeconds(interactCheckDelay);
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    #region input

    void OnInputAction(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            //check if interactive
            if(mReticleCurInteract == null) {
                //determine what sort of equipment and fire that weapon
            }
        }
        else if(dat.state == InputManager.State.Released) {
            if(mReticleCurInteract != null) {
                //
                mReticleCurInteract.Act(gameObject);
            }
            else {
                //release click based action?
            }
        }
    }

    #endregion

    #region player messages

    void EntityStart(EntityBase ent) {
        mPlayer = ent as Player;

        //hook up callbacks
        mPlayer.setStateCallback += OnPlayerSetState;
    }

    void OnPlayerSetState(EntityBase ent, int state) {
        switch((EntityState)state) {
            case EntityState.Normal:
                if(mCursorAutoLock.isLocked)
                    inputEnabled = true;

                reticleActive = true;
                break;

            case EntityState.Dead:
                inputEnabled = false;
                reticleActive = false;
                break;

            case EntityState.Invalid:
                inputEnabled = false;
                reticleActive = false;
                break;
        }
    }

    #endregion

    void OnCursorLock(bool locked) {
        if(locked) {
            switch((EntityState)mPlayer.state) {
                case EntityState.Normal:
                    inputEnabled = true;
                    break;
            }
        }
        else {
            inputEnabled = false;
        }
    }

    IEnumerator DoReticleCheck() {
        mReticleIsRunning = true;

        while(mReticleIsRunning) {
            yield return mReticleWaitDelay;

            //check
            Interactor checkInteract = null;

            RaycastHit hit;
            if(Physics.Raycast(mMoveCtrl.transform.position, mMoveCtrl.eye.forward, out hit, interactDistance, interactLayerCast)) {
                if((interactLayer & (1 << hit.collider.gameObject.layer)) != 0) {
                    Interactor interactor = hit.collider.GetComponent<Interactor>();
                    checkInteract = interactor;
                }
            }

            //set reticle if there is a new one
            if(mReticleCurInteract != checkInteract) {
                if(mReticleCurInteract != null)
                    mReticleCurInteract.isPointedAt = false;

                mReticleCurInteract = checkInteract;
                if(mReticleCurInteract != null) {
                    mReticleCurInteract.isPointedAt = true;
                }
                else {
                    Reticle.instance.state = Reticle.instance.stateStart;
                }
            }
        }
    }
}
