using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DigHoleManager : MonoBehaviour {
    public const string digSpawnHolderTag = "DigHoleSpawnHolder";

    public string digHoleDefault = "digSeed";

    public int maxHoles = 5;

    private static DigHoleManager mInstance = null;

    private List<DigHole> mDigs;

    private PoolController mPool;

    private Transform mSpawnHolder;

    public static DigHoleManager instance { get { return mInstance; } }

    public DigHole SpawnDefaultDig(Transform parent, Vector3 pos, Vector3 upVector) {
        return SpawnDig(digHoleDefault, parent, pos, upVector);
    }

    /// <summary>
    /// Make sure the type is correct. if parent is null, spawned to holder
    /// </summary>
    public DigHole SpawnDig(string type, Transform parent, Vector3 pos, Vector3 upVector) {
        if(mDigs.Count == maxHoles) {
            //remove oldest
            RemoveDig(mDigs[0]);
        }

        Transform t = mPool.Spawn(type, type, parent == null ? mSpawnHolder : parent, pos, Quaternion.identity);
        if(t != null) {
            t.up = upVector;

            DigHole dh = t.GetComponent<DigHole>();
            if(dh != null) {
                mDigs.Add(dh);
                return dh;
            }
            else {
                Debug.LogError("Invalid dig hole: "+type);
            }
        }

        return null;
    }

    /// <summary>
    /// This is normally called by DigHole to remove itself. Returns true if dig is found, removed, and released.
    /// </summary>
    public bool RemoveDig(DigHole dig) {
        int ind = mDigs.IndexOf(dig);
        if(ind != -1) {
            mDigs.RemoveAt(ind);
            mPool.Release(dig.transform);
            return true;
        }

        return false;
    }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;
    }

    void OnEnable() {
        GameObject go = GameObject.FindGameObjectWithTag(digSpawnHolderTag);
        if(go == null) {
            go = new GameObject("digHoleSpawnHolder");
            go.tag = digSpawnHolderTag;
        }

        mSpawnHolder = go.transform;
    }

    void OnDisable() {
        mSpawnHolder = null;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

            mPool = GetComponent<PoolController>();

            mDigs = new List<DigHole>(maxHoles);
        }
    }
}
