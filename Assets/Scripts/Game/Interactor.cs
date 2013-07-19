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
        
    public bool defaultLocked = false;

    public GameObject onPointGO; //set to active if we are pointed at

    [SerializeField]
    State _stateOnPoint = State.None; //what state to use if we are being pointed at.

    private bool mIsPointedAt = false; //this is set by player controller to determine if we are pointing at this interactor
    private bool mLocked = false; //if true, can't perform interaction if we are being pointed at

    public bool locked { get { return mLocked; } set { mLocked = value; } }

    public State stateOnPoint {
        get { return _stateOnPoint; }
        set {
            if(_stateOnPoint != value) {
                _stateOnPoint = value;
                                
                if(mIsPointedAt)
                    Reticle.instance.state = (int)_stateOnPoint;
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
                    Reticle.instance.state = (int)_stateOnPoint;
            }
        }
    }

    public void Act() {
        if(!mLocked) {
            targetFSM.SendEvent(EntityEvent.Interact);
        }
    }

    void OnEnable() {
        locked = defaultLocked;
    }

    void Awake() {
        if(targetFSM == null)
            targetFSM = GetComponent<PlayMakerFSM>();

        if(onPointGO != null)
            onPointGO.SetActive(false);
    }
}
