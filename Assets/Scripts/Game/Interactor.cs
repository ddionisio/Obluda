using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

public class Interactor : MonoBehaviour {
    public enum State {
        None,
        Invalid,
        Dialog,
        Act,
    }

    public PlayMakerFSM targetFSM; //if null, use the fsm of this object
        
    public GameObject onPointGO; //set to active if we are pointed at

    [SerializeField]
    bool _defaultLocked = false;

    [SerializeField]
    State _stateOnPoint = State.None; //what state to use if we are being pointed at.

    private bool mIsPointedAt = false; //this is set by player controller to determine if we are pointing at this interactor
    private bool mLocked = false; //if true, can't perform interaction if we are being pointed at
    private State mStateOnPoint;

    public bool lockedDefault { get { return _defaultLocked; } }
    public bool locked { get { return mLocked; } set { mLocked = value; } }

    public State stateOnPointDefault { get { return _stateOnPoint; } }
    public State stateOnPoint {
        get { return mStateOnPoint; }
        set {
            if(mStateOnPoint != value) {
                mStateOnPoint = value;
                                
                if(mIsPointedAt)
                    Reticle.instance.state = (int)mStateOnPoint;
            }
        }
    }

    public bool isPointedAt {
        get { return mIsPointedAt; }
        set {
            if(mIsPointedAt != value) {
                mIsPointedAt = value;

                if(onPointGO != null)
                    onPointGO.SetActive(mIsPointedAt);

                if(mIsPointedAt)
                    Reticle.instance.state = (int)mStateOnPoint;
            }
        }
    }

    public void Act(GameObject source) {
        if(!mLocked) {
            Fsm.EventData.GameObjectData = source;
            targetFSM.SendEvent(EntityEvent.Interact);
        }
    }

    void OnEnable() {
        mStateOnPoint = _stateOnPoint;
        locked = _defaultLocked;
    }

    void Awake() {
        if(targetFSM == null)
            targetFSM = GetComponent<PlayMakerFSM>();

        if(onPointGO != null)
            onPointGO.SetActive(false);
    }
}
