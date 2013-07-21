using UnityEngine;
using System.Collections;

public class CursorAutoLock : MonoBehaviour {
    public delegate void OnCursorLock(bool isLock);

    public bool checkActiveUIModal = true; //cursor can't lock if there's an active modal

    public event OnCursorLock cursorLockCallback;
        
    private bool mWasLocked = false;

    public bool isLocked { get { return mWasLocked; } }

    public void SetLockCursor(bool locked) {
        Screen.lockCursor = locked && CanLock();
    }

    //void OnMouseDown() {
      //  Screen.lockCursor = CanLock();
    //}

    void OnUIModalActive() {
        if(checkActiveUIModal)
            Screen.lockCursor = false;
    }

    void OnUIModalInactive() {
        if(checkActiveUIModal)
            Screen.lockCursor = true;
    }

    void OnDestroy() {
        cursorLockCallback = null;
    }

    // Update is called once per frame
    void Update() {
        if(!Screen.lockCursor) {
            if(Input.GetMouseButtonDown(0)) {
                Screen.lockCursor = CanLock();
                return;
            }

            if(mWasLocked) {
                mWasLocked = false;
                if(cursorLockCallback != null)
                    cursorLockCallback(false);
            }
        }
        else if(Screen.lockCursor) {
            if(!CanLock()) {
                Screen.lockCursor = false;
                return;
            }

            if(!mWasLocked) {
                mWasLocked = true;
                if(cursorLockCallback != null)
                    cursorLockCallback(true);
            }
        }
    }

    bool CanLock() {
        if(!checkActiveUIModal)
            return true;

        return UIModalManager.instance == null || UIModalManager.instance.activeCount == 0;
    }
}
