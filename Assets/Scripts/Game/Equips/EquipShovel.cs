using UnityEngine;
using System.Collections;

public class EquipShovel : EquipBase {
    public AnimatorData animator;

    public string takeIdle;
    public string takeSwing;
    public string takeDig;

    /// <summary>
    /// Set as equip, left=false -> right hand. Note: if left=false, then this gameobject's x-scale is set to -1
    /// </summary>
    public override void Equip(PlayerController ctrl, bool left) {
        animator.Play(takeIdle);
    }

    /// <summary>
    /// Called when we want to perform an action. isPress=false means the button was released
    /// </summary>
    /// <returns>true if action has been performed. This will cancel out any other action and interact.</returns>
    public override bool Action(PlayerController ctrl, bool isPress) {
        if(isPress)
            animator.Play(takeSwing);

        return true;
    }

    /// <summary>
    /// Return true if we are in a process of performing an action.
    /// </summary>
    /// <returns></returns>
    public override bool ActionInProgress() {
        return animator.isPlaying && animator.currentPlayingTake.name != takeIdle;
    }

    /// <summary>
    /// Cancel current action in progress.
    /// </summary>
    public override void ActionCancel(PlayerController ctrl) {
        animator.Play(takeIdle);
    }

    /// <summary>
    /// Update visually while active
    /// </summary>
    /// <param name="ctrl"></param>
    public override void ActionUpdate(PlayerController ctrl) {
    }
}
