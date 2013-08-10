using UnityEngine;
using HutongGames.PlayMaker;
using M8.PlayMaker;

[ActionCategory("Game")]
public class StatSetFlag : FSMActionComponentBase<Stat> {
    public enum Action {
        Set,
        Clear,
        Flip
    }

    public Stat.Flag flag;
    public Action action;

    // Code that runs on entering the state.
    public override void OnEnter() {
        base.OnEnter();

        switch(action) {
            case Action.Set:
                mComp.flags |= flag;
                break;

            case Action.Clear:
                mComp.flags &= ~flag;
                break;

            case Action.Flip:
                mComp.flags ^= flag;
                break;
        }

        Finish();
    }
}
