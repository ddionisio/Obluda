using UnityEngine;
using HutongGames.PlayMaker;
using M8.PlayMaker;

[ActionCategory("Game")]
[Tooltip("Set the point-at interaction icon for the reticle to default.")]
public class InteractorSetPointAtStateDefault : FSMActionComponentBase<Interactor> {

    // Code that runs on entering the state.
    public override void OnEnter() {
        base.OnEnter();

        mComp.stateOnPoint = mComp.stateOnPointDefault;

        Finish();
    }
}
