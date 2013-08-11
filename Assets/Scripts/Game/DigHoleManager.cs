using UnityEngine;
using System.Collections;

public class DigHoleManager : MonoBehaviour {
    public int maxHoles = 5;

    private static DigHoleManager mInstance = null;

    private PoolController mPool;

    public static DigHoleManager instance { get { return mInstance; } }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

            mPool = GetComponent<PoolController>();
        }
    }
}
