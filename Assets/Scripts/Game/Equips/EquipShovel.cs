using UnityEngine;
using System.Collections;

public class EquipShovel : EquipBase {
    public enum DigType {
        None,
        Diggable,
        Hole,
    }

    public AnimatorData animator;

    public Transform digHighlight;

    public string takeIdle;
    public string takeSwing;
    public string takeDig;

    public float digOfs;

    public string digTag;
    public LayerMask digLayer;

    public string digHoleTag;
    public LayerMask digHoleLayer;

    private bool mIsHitting;
    private bool mDigging;

    private DigType mDigType = DigType.None;
    private RaycastHit mDigHit;

    public void SetHitting(bool isHitting) {
        mIsHitting = isHitting;
    }

    public void SetDigging(bool isDigging) {
        mDigging = isDigging && mDigType == DigType.Diggable;
    }

    /// <summary>
    /// Set as equip, left=false -> right hand. Note: if left=false, then this gameobject's x-scale is set to -1
    /// </summary>
    public override void Equip(bool left) {
        animator.Play(takeIdle);

        mIsHitting = false;
    }
        
    /// <summary>
    /// Called when we want to perform an action. isPress=false means the button was released
    /// </summary>
    /// <returns>true if action has been performed. This will cancel out any other action and interact.</returns>
    public override bool Action(InputManager.State state) {
        if(state == InputManager.State.Pressed) {
            if(mDigType == DigType.Diggable) {
                animator.Play(takeDig);
            }
            else {
                animator.Play(takeSwing, true);
            }
        }
        else {
            if(animator.isPlaying) {
                if(animator.currentPlayingTake.name == takeSwing)
                    animator.currentPlayingTake.sequence.loops = animator.currentPlayingTake.sequence.completedLoops + 1;
            }
        }

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
    public override void ActionCancel() {
        animator.Play(takeIdle);

        mIsHitting = false;

        DeinitDig();
    }

    /// <summary>
    /// Update visually while active
    /// </summary>
    /// <param name="ctrl"></param>
    public override void ActionUpdate() {
        if(mIsHitting) {
            hit.Perform(source.position, source.controller.moveController.eye.forward, source.radius + hit.radius);
        }
        else if(mDigging) {
            //spawn dig spot
            DigInteract digInteract = mDigHit.collider.GetComponent<DigInteract>();
            if(digInteract != null)
                digInteract.Action(mDigHit.point, mDigHit.normal);
            else {
                //spawn a default dig hole
                DigHoleManager.instance.SpawnDefaultDig(null, mDigHit.point, mDigHit.normal);
            }

            DeinitDig();
        }
        else {
            if(!animator.isPlaying || animator.currentPlayingTake.name == takeDig) {
                //check if pointing to dig spot
                UpdateDigSpot();
            }
        }

        //display dig highlight
        if(mDigType == DigType.Diggable) {
            digHighlight.position = mDigHit.point;
            digHighlight.up = mDigHit.normal;
        }
    }

    protected override void Awake() {
        base.Awake();

        digHighlight.gameObject.SetActive(false);
    }

    void DeinitDig() {
        mDigType = DigType.None;
        mDigging = false;
        digHighlight.gameObject.SetActive(false);
    }

    void UpdateDigSpot() {
        bool isHit = Physics.Raycast(
            source.position,
            source.controller.moveController.eye.forward,
            out mDigHit, source.radius + digOfs, digLayer | digHoleLayer);

        if(isHit) {
            //check if it's a digHole
            GameObject go = mDigHit.collider.gameObject;
            if((digHoleLayer & (1 << go.layer)) != 0 && go.tag == digHoleTag) {
                mDigType = DigType.Hole;
                digHighlight.gameObject.SetActive(false);
            }
            else if((digLayer & (1 << go.layer)) != 0 && go.tag == digTag) {
                mDigType = DigType.Diggable;
                digHighlight.gameObject.SetActive(true);
            }
            else {
                DeinitDig();
            }
        }
        else {
            DeinitDig();
        }
    }
}
