using UnityEngine;
using System.Collections;

public class Character : EntityBase {
    [System.Flags]
    public enum Flag {
        None = 0,
        Block = 0x1,
        Invulnerable = 0x2,
    }

    private Stat mStats;
    private float mRadius;

    public Stat stats { get { return mStats; } }
    public virtual float radius { get { return mRadius; } }

    protected virtual void OnHPChanged(Stat s, float delta) {
        if(s.curHP <= 0) {
            //dead
            state = (int)EntityState.Dead;
        }
        else {
            if(delta < 0) {
                state = (int)EntityState.Hit;
            }
        }
    }

    protected override void OnDespawned() {
        //reset stuff here

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    public override void SpawnFinish() {
        //start ai, player control, etc

        state = (int)EntityState.Normal;
    }

    protected override void SpawnStart() {
        //initialize some things
    }

    protected override void Awake() {
        base.Awake();
                
        //initialize variables
        mStats = GetComponent<Stat>();
        if(mStats != null) {
            mStats.hpChangeCallback += OnHPChanged;
        }

        mRadius = 0.0f;

        if(collider != null) {
            if(collider is SphereCollider)
                mRadius = ((SphereCollider)collider).radius;
            else if(collider is CapsuleCollider)
                mRadius = ((CapsuleCollider)collider).radius;
        }
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }
}
