using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//TODO: right now it assumes a sphere collider
public class FPMoveController : MonoBehaviour {
    public Transform dirHolder; //the forward vector of this determines our forward movement

    public float moveForce = 25.0f;
    public float moveAirForce = 10.0f;
    public float moveMaxSpeed = 3.5f;

    public float jumpForce = 40.0f;
    public float jumpDelay = 0.2f;

    public float turnSensitivity = 2.0f;

    public float airDrag = 0.0f; //if there is no ground collision, this is the drag
    public float groundDrag = 0.01f; //if there is ground and/or side collision and/or we are moving
    public float standDrag = 10.0f; //drag when we are standing still

    public float slopLimit = 45.0f; //if we are standing still and slope is high, just use groundDrag

    public float topBottomCollisionAngle = 30.0f; //criteria to determine collision flag

    public int player = 0;
    public int moveInputX = InputManager.ActionInvalid;
    public int moveInputY = InputManager.ActionInvalid;
    public int turnInput = InputManager.ActionInvalid;
    public int jumpInput = InputManager.ActionInvalid;

    public bool startInputEnabled = false;

    private struct CollideInfo {
        public CollisionFlags flag;
        public Vector3 normal;

    }

    private Vector2 mCurInputMoveAxis;
    private float mCurInputTurnAxis;
    private Vector3 mCurMoveDir;

    //private HashSet<Collider> mColls = new HashSet<Collider>();
    private Dictionary<Collider, CollideInfo> mColls = new Dictionary<Collider, CollideInfo>(16);

    private bool mInputEnabled = false;

    private CollisionFlags mCollFlags;

    private bool mGroundSlide = false;
    private float mTopBottomColCos;
    private bool mJump = false;
    private float mJumpLastTime = 0.0f;
    private Vector3 mCurUp; //this is for checking to see if the up vector suddenly changed
    
    public CollisionFlags collisionFlags { get { return mCollFlags; } }
    public bool isGrounded { get { return (mCollFlags & CollisionFlags.Below) != 0; } }
    public bool isJump { get { return mJump; } }

    public bool inputEnabled {
        get { return mInputEnabled; }
        set {
            if(mInputEnabled != value) {
                mInputEnabled = value;

                InputManager input = Main.instance != null ? Main.instance.input : null;
                if(input != null) {
                    if(mInputEnabled) {
                        input.AddButtonCall(player, jumpInput, OnInputJump);
                    }
                    else {
                        input.RemoveButtonCall(player, jumpInput, OnInputJump);
                    }
                }
            }
        }
    }

    void OnCollisionEnter(Collision col) {
        //refresh during stay
        //mCollFlags = CollisionFlags.None;
        //mSlide = false;

        //mColls.Clear();

        Vector3 up = transform.up;
        
        foreach(ContactPoint contact in col.contacts) {
            if(!mColls.ContainsKey(contact.otherCollider)) {
                //mColls.Add(contact.otherCollider, contact);

                CollisionFlags colFlag = M8.PhysicsUtil.GetCollisionFlagsSphereCos(up, collider.bounds.center, mTopBottomColCos, contact.point);
                mCollFlags |= colFlag;

                if(colFlag == CollisionFlags.Below) {
                    mGroundSlide = Vector3.Angle(up, contact.normal) > slopLimit;
                }

                mColls.Add(contact.otherCollider, new CollideInfo() { flag = colFlag, normal = contact.normal });
            }
        }
    }

    /*void OnCollisionStay(Collision col) {
        Vector3 up = transform.up;

        foreach(ContactPoint contact in col.contacts) {
            CollisionFlags colFlag = M8.PhysicsUtil.GetCollisionFlagsSphereCos(up, collider.bounds.center, mTopBottomColCos, contact.point);
            if(colFlag == CollisionFlags.Sides) {
                Vector3 dir = rigidbody.velocity;
                float velMag = dir.magnitude;
                if(velMag > 0.0f) {
                    dir /= velMag;
                    if(Vector3.Dot(dir, contact.normal) < -0.5f) {
                        rigidbody.velocity = M8.MathUtil.Slide(dir, contact.normal) * velMag;
                    }
                }
            }
        }
    }*/

    void OnCollisionExit(Collision col) {
        /*mUpdateCollisionFlags = true;
        mCollFlags = CollisionFlags.None;
        mSlide = false;

        foreach(ContactPoint contact in col.contacts) {
            mColls.Remove(contact.otherCollider);
        }*/

        foreach(ContactPoint contact in col.contacts) {
            if(mColls.ContainsKey(contact.otherCollider))
                mColls.Remove(contact.otherCollider);
        }

        mCurUp = transform.up;
        RefreshCollInfo();
    }

    void OnDestroy() {
        inputEnabled = false;
    }

    void Awake() {
        mTopBottomColCos = Mathf.Cos(topBottomCollisionAngle);
    }

    // Use this for initialization
    void Start() {
        inputEnabled = startInputEnabled;
        mCurUp = transform.up;
    }

    // Update is called once per frame
    void FixedUpdate() {
#if UNITY_EDITOR
        mTopBottomColCos = Mathf.Cos(topBottomCollisionAngle);
#endif

        Rigidbody body = rigidbody;
        Quaternion rot = transform.rotation;

        Quaternion dirRot = dirHolder.rotation;

        if(mInputEnabled) {
            InputManager input = Main.instance.input;

            bool doMove = false;
            float doMoveForce = 0.0f;

            mCurInputMoveAxis.x = moveInputX != InputManager.ActionInvalid ? input.GetAxis(player, moveInputX) : 0.0f;
            mCurInputMoveAxis.y = moveInputY != InputManager.ActionInvalid ? input.GetAxis(player, moveInputY) : 0.0f;
            mCurInputTurnAxis = turnInput != InputManager.ActionInvalid ? input.GetAxis(player, turnInput) : 0.0f;

            if(mCurInputTurnAxis != 0.0f) {
                dirRot *= Quaternion.AngleAxis(mCurInputTurnAxis * turnSensitivity, Vector3.up);
                dirHolder.rotation = dirRot;
            }

            if(mJump) {
                if(Time.fixedTime - mJumpLastTime >= jumpDelay || (mCollFlags & CollisionFlags.Above) != 0) {
                    mJump = false;
                }
                else {
                    body.AddForce(rot * Vector3.up * jumpForce);
                }
            }

            if(isGrounded) {
                if(mCurInputMoveAxis != Vector2.zero) {
                    body.drag = mJump ? airDrag : groundDrag;

                    doMove = body.velocity.sqrMagnitude < moveMaxSpeed * moveMaxSpeed;
                    doMoveForce = moveForce;
                }
                else {//we are standing, check if we are only colliding below and not 'sliding'
                    if(mCollFlags == CollisionFlags.Below && !mGroundSlide)
                        body.drag = mJump ? airDrag : standDrag;
                    else
                        body.drag = mJump ? airDrag : groundDrag;
                }
            }
            else {
                body.drag = airDrag;

                if(mCurInputMoveAxis != Vector2.zero) {
                    doMove = body.velocity.sqrMagnitude < moveMaxSpeed * moveMaxSpeed;
                    doMoveForce = moveAirForce;
                }
            }

            if(doMove) {
                //bool doSlide = false;
                //Vector3 slide

                Vector3 moveDelta = dirRot * Vector3.forward * mCurInputMoveAxis.y;
                moveDelta += dirRot * Vector3.right * mCurInputMoveAxis.x;

                mCurMoveDir = moveDelta.normalized;

                //check if we need to slide
                foreach(KeyValuePair<Collider, CollideInfo> pair in mColls) {
                    if(pair.Value.flag == CollisionFlags.Sides) {
                        if(Vector3.Dot(mCurMoveDir, pair.Value.normal) < 0.0f) {
                            moveDelta = M8.MathUtil.Slide(mCurMoveDir, pair.Value.normal);
                            break;
                        }
                    }
                }

                body.AddForce(moveDelta * doMoveForce);
            }
            else {
                mCurMoveDir = Vector3.zero;
            }
        }
        else {
            mCurInputMoveAxis = Vector2.zero;
            mCurInputTurnAxis = 0.0f;
            mCurMoveDir = Vector3.zero;
        }

        if(transform.up != mCurUp) {
            mCurUp = transform.up;
            RefreshCollInfo();
        }
    }

    void OnInputJump(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            if(isGrounded && !mJump) {
                mJump = true;
                mJumpLastTime = Time.fixedTime;
            }
        }
        else if(dat.state == InputManager.State.Released) {
            mJump = false;
        }
    }

    void RefreshCollInfo() {
        mCollFlags = CollisionFlags.None;
        mGroundSlide = false;

        Vector3 up = transform.up;

        foreach(KeyValuePair<Collider, CollideInfo> pair in mColls) {
            //ContactPoint contact = pair.Value;

            //CollisionFlags colFlag = M8.PhysicsUtil.GetCollisionFlagsSphereCos(up, target.collider.bounds.center, mTopBottomColCos, contact.point);
            mCollFlags |= pair.Value.flag;

            if(pair.Value.flag == CollisionFlags.Below) {
                mGroundSlide = Vector3.Angle(up, pair.Value.normal) > slopLimit;
            }
        }
    }
}
