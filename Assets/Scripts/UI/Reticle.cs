using UnityEngine;
using System.Collections;

public class Reticle : MonoBehaviour {
    public GameObject[] states;

    public int stateStart = 0;

    private static Reticle mInstance;
    
    private int mCurState = -1;

    public static Reticle instance { get { return mInstance; } }

    public int state {
        get { return mCurState; }
        set {
            if(mCurState != value) {
                if(mCurState >= 0 && mCurState < states.Length)
                    states[mCurState].SetActive(false);

                mCurState = value;

                if(mCurState >= 0 && mCurState < states.Length)
                    states[mCurState].SetActive(true);
            }
        }
    }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

            foreach(GameObject stateGO in states)
                stateGO.SetActive(false);

            state = stateStart;
        }
    }

    // Use this for initialization
    void Start() {

    }
}
