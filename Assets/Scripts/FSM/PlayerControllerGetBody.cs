using UnityEngine;
using HutongGames.PlayMaker;
using M8.PlayMaker;

[ActionCategory("Game")]
[Tooltip("Get the GameObject of Player that is in the world.  This contains its physical data including the FPMoveController (RigidBodyController).")]
public class PlayerControllerGetBody : FSMActionComponentBase<PlayerController> {
    [RequiredField]
    [UIHint(UIHint.Variable)]
    public FsmGameObject output;

    public override void Reset() {
        base.Reset();
    }

    // Code that runs on entering the state.
    public override void OnEnter() {
        base.OnEnter();

        output.Value = mComp.moveController.gameObject;

        Finish();
    }


}
