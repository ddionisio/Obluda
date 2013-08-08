using UnityEngine;
using System.Collections;

public abstract class EquipBase : MonoBehaviour {
    private int mItemID = ItemManager.InvalidID;

    public int itemID { get { return mItemID; } }

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
    public abstract void Equip(Player player, bool left);

    /// <summary>
    /// Called when we want to perform an action. isPress=false means the button was released
    /// </summary>
    /// <returns>true if action has been performed. This will cancel out any other action and interact.</returns>
    public abstract bool Action(Player player, InputManager.State state);

    /// <summary>
    /// Return true if we are in a process of performing an action.
    /// </summary>
    /// <returns></returns>
    public abstract bool ActionInProgress();

    /// <summary>
    /// Cancel current action in progress.
    /// </summary>
    public abstract void ActionCancel(Player player);
        
    /// <summary>
    /// Update visually while active
    /// </summary>
    /// <param name="ctrl"></param>
    public abstract void ActionUpdate(Player player);
}
