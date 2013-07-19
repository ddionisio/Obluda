using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {

    private static HUD mInstance;
        
    public static HUD instance { get { return mInstance; } }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

        }
    }

    // Use this for initialization
    void Start() {

    }
}
