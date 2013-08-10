using UnityEngine;
using System.Collections;

public class Stat : MonoBehaviour {
    public const int invalidID = 0;

    public delegate void OnChange(Stat stat, float delta);

    [System.Flags]
    public enum Flag {
        None = 0,
        Block = 0x1,
        Invulnerable = 0x2,
        Pullable = 0x4, //for grappling, if this object can be pulled to player, otherwise the player is pulled
        Pickable = 0x8,
    }

    public int id = invalidID; //if valid, get stats from user data

    public float maxHP;

    public event OnChange hpChangeCallback;

    private float mCurHP;
    private Flag mFlags;

    public float curHP {
        get { return mCurHP; }
        set {
            float newVal = Mathf.Clamp(value, 0.0f, maxHP);
            if(mCurHP != newVal) {
                if(newVal < mCurHP && (mFlags & Flag.Invulnerable) != 0)
                    return;

                float prev = mCurHP;
                mCurHP = newVal;

                if(hpChangeCallback != null)
                    hpChangeCallback(this, newVal - prev);
            }
        }
    }

    public Flag flags {
        get { return mFlags; }
        set { mFlags = value; }
    }

    public virtual void Reset() {
    }

    public virtual void Save() {
        //if persistent, save stats based on id
    }

    protected virtual void OnDestroy() {
        hpChangeCallback = null;
    }
}
