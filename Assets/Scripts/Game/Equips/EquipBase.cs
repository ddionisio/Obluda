using UnityEngine;
using System.Collections;

public abstract class EquipBase : MonoBehaviour {

    /// <summary>
    /// Set as equip, left=false -> right hand. Note: if left=false, then this gameobject's x-scale is set to -1
    /// </summary>
    public abstract void Equip(PlayerController ctrl, bool left);

    /// <summary>
    /// Called when we want to perform an action. isPress=false means the button was released
    /// </summary>
    /// <returns>true if action has been performed. This will cancel out any other action and interact.</returns>
    public abstract bool Action(PlayerController ctrl, bool isPress);

    /// <summary>
    /// Return true if we are in a process of performing an action.
    /// </summary>
    /// <returns></returns>
    public abstract bool ActionInProgress();

    /// <summary>
    /// Cancel current action in progress.
    /// </summary>
    public abstract void ActionCancel(PlayerController ctrl);
        
    /// <summary>
    /// Update visually while active
    /// </summary>
    /// <param name="ctrl"></param>
    public abstract void Update(PlayerController ctrl);
}
