using UnityEngine;
using System.Collections;

public class ModalPause : UIController {
    public UIEventListener resume;

    protected override void OnActive(bool active) {
        if(active) {
            resume.onClick = OnResumeClick;
        }
        else {
            resume.onClick = null;
        }
    }

    protected override void OnOpen() {
        Main.instance.sceneManager.Pause();
    }

    protected override void OnClose() {
        Main.instance.sceneManager.Resume();
    }

    void OnResumeClick(GameObject go) {
        UIModalManager.instance.ModalCloseTop();
    }
}
