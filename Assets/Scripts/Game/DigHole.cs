using UnityEngine;
using System.Collections;

public class DigHole : MonoBehaviour {
    public float removeDelay = 10.0f;

    private Interactor mInteract;
    
    private float mCurTime;
        
    protected virtual void OnInteract(Interactor interact, GameObject source) {
    }

    protected virtual void OnSpawned() {
        mCurTime = 0.0f;

        //TODO: dig hole fade in animation
    }

    protected virtual void Awake() {
        mInteract = GetComponent<Interactor>();

        if(mInteract != null)
            mInteract.actionCallback += OnInteract;

        mCurTime = 0.0f;
    }

    // Update is called once per frame
    void Update() {
        //TODO: dig hole fade out animation
        if(removeDelay > 0.0f) {
            mCurTime += Time.deltaTime;
            if(mCurTime >= removeDelay) {
                if(!DigHoleManager.instance.RemoveDig(this)) {
                    PoolController.ReleaseAuto(this.transform);
                }
            }
        }
    }
}
