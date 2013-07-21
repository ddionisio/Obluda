using UnityEngine;
using HutongGames.PlayMaker;
using M8.PlayMaker;

[ActionCategory("Game")]
[Tooltip("Check to see if interactor is locked")]
public class InteractorIsLocked : FSMActionComponentBase<Interactor> {
    public FsmEvent isTrue;
    public FsmEvent isFalse;

    public override void Reset() {
        base.Reset();

        isTrue = null;
        isFalse = null;
    }

    // Code that runs on entering the state.
    public override void OnEnter() {
        base.OnEnter();

        if(mComp.locked) {
            if(!FsmEvent.IsNullOrEmpty(isTrue))
                Fsm.Event(isTrue);
        }
        else {
            if(!FsmEvent.IsNullOrEmpty(isFalse))
                Fsm.Event(isFalse);
        }

        Finish();
    }

    public override string ErrorCheck() {
        if(FsmEvent.IsNullOrEmpty(isTrue) &&
            FsmEvent.IsNullOrEmpty(isFalse))
            return "Action sends no events!";
        return "";
    }
}
