using UnityEngine;
using HutongGames.PlayMaker;
using M8.PlayMaker;

[ActionCategory("Game")]
[Tooltip("Set the interactor lock default.")]
public class InteractorSetLockedDefault : FSMActionComponentBase<Interactor> {

    // Code that runs on entering the state.
    public override void OnEnter() {
        base.OnEnter();

        mComp.locked = mComp.lockedDefault;

        Finish();
    }


}
