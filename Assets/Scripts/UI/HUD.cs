using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {
    public UISprite reticle;

    private static HUD mInstance;

    private NGUIColorPulse mReticleActivePulse;

    public static HUD instance { get { return mInstance; } }

    public bool reticleActive {
        get { return mReticleActivePulse.enabled; }
        set {
            mReticleActivePulse.enabled = value;
        }
    }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

            mReticleActivePulse = reticle.GetComponentInChildren<NGUIColorPulse>();
            mReticleActivePulse.enabled = false;
        }
    }

    // Use this for initialization
    void Start() {

    }
}
