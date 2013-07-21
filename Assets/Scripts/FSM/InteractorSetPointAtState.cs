using UnityEngine;
using HutongGames.PlayMaker;
using M8.PlayMaker;

[ActionCategory("Game")]
[Tooltip("Set the reticle icon for the interactor when player points at it.")]
public class InteractorSetPointAtState : FSMActionComponentBase<Interactor> {
    public Interactor.State state = Interactor.State.None;

    // Code that runs on entering the state.
    public override void OnEnter() {
        base.OnEnter();

        mComp.stateOnPoint = state;

        Finish();
    }


}
