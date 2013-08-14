using UnityEngine;
using System.Collections;

using HutongGames.PlayMaker;

/// <summary>
/// Interactive digging for special triggers
/// Make sure this object has an FSM, upon dig, ActionHit state is sent to the FSM
/// event gameobject will be the dighole if it is valid
/// </summary>
public class DigInteract : MonoBehaviour {
    public string digHoleType; //if not null, spawn a dighole
    public bool digHoleTypeUseDefault; //if true, ignore digHoleType and use the default
    public bool digHoleIsChild; //if true, then spawn the dighole as a child to this object

    private PlayMakerFSM mFSM;
    private ItemDrop mItemDrop;

    private Vector3 mLastDigPos;
    private Vector3 mLastDigNormal;

    public Vector3 lastDigPosition { get { return mLastDigPos; } }
    public Vector3 lastDigNormal { get { return mLastDigNormal; } }

    /// <summary>
    /// Called when a dig happens
    /// </summary>
    public void Action(Vector3 digPos, Vector3 digNormal) {
        mLastDigPos = digPos;
        mLastDigNormal = digNormal;

        string holeType = digHoleTypeUseDefault ? DigHoleManager.instance.digHoleDefault : digHoleType;

        if(!string.IsNullOrEmpty(holeType)) {
            DigHole dh = DigHoleManager.instance.SpawnDig(holeType, digHoleIsChild ? transform : null, digPos, digNormal);

            if(dh != null && mFSM != null) {
                Fsm.EventData.GameObjectData = dh.gameObject;
            }
        }

        if(mFSM != null) {
            mFSM.SendEvent(ActionEvent.Hit);
        }

        if(mItemDrop != null) {
            mItemDrop.Drop();
        }
    }

    void Awake() {
        mFSM = GetComponent<PlayMakerFSM>();
        mItemDrop = GetComponent<ItemDrop>();
    }
}
