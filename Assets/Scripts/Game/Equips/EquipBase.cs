﻿using UnityEngine;
using System.Collections;

public abstract class EquipBase : MonoBehaviour {
    private int mItemID = ItemManager.InvalidID;

    private HitInfo mHit;
    private Player mSource;

    public int itemID { get { return mItemID; } }
    public HitInfo hit { get { return mHit; } }

    public Player source { get { return mSource; } set { mSource = value; } }

    /// <summary>
    /// Only used by Player when this equipment becomes active
    /// </summary>
    /// <param name="itmID"></param>
    public void _setItemID(int itmID) {
        mItemID = itmID;
    }

    /// <summary>
    /// Set as equip, left=false -> right hand. Note: if left=false, then this gameobject's x-scale is set to -1
    /// </summary>
    public abstract void Equip(bool left);

    /// <summary>
    /// Called when we want to perform an action. isPress=false means the button was released
    /// </summary>
    /// <returns>true if action has been performed. This will cancel out any other action and interact.</returns>
    public abstract bool Action(InputManager.State state);

    /// <summary>
    /// Return true if we are in a process of performing an action.
    /// </summary>
    /// <returns></returns>
    public abstract bool ActionInProgress();

    /// <summary>
    /// Cancel current action in progress.
    /// </summary>
    public abstract void ActionCancel();
        
    /// <summary>
    /// Update visually while active
    /// </summary>
    /// <param name="ctrl"></param>
    public abstract void ActionUpdate();

    protected virtual void Awake() {
        mHit = GetComponent<HitInfo>();
    }
}
