using UnityEngine;
using HutongGames.PlayMaker;
using M8.PlayMaker;

[ActionCategory("Game")]
[Tooltip("Set the interactor locked/unlocked.")]
public class InteractorSetLocked : FSMActionComponentBase<Interactor> {
    [RequiredField]
    public FsmBool locked;

    public override void Reset() {
        base.Reset();

        locked = null;
    }

    // Code that runs on entering the state.
    public override void OnEnter() {
        base.OnEnter();

        mComp.locked = locked.Value;

        Finish();
    }


}
