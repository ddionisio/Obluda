using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//TODO: right now it assumes a sphere collider
public class FPMoveController : MonoBehaviour {
    public Transform target;

    public float moveForce = 15.0f;
    public float moveAirForce = 5.0f;
    public float moveMaxSpeed = 5.0f;

    public float jumpForce = 25.0f;
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

    //private HashSet<Collider> mColls = new HashSet<Collider>();
    private Dictionary<Collider, CollideInfo> mColls = new Dictionary<Collider, CollideInfo>(16);

    private bool mInputEnabled = false;

    private CollisionFlags mCollFlags;

    private bool mSlide = false;
    private float mTopBottomColCos;
    private bool mJump = false;
    private float mJumpLastTime = 0.0f;

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

        Vector3 up = target.up;

        foreach(ContactPoint contact in col.contacts) {
            if(!mColls.ContainsKey(contact.otherCollider)) {
                //mColls.Add(contact.otherCollider, contact);

                CollisionFlags colFlag = M8.PhysicsUtil.GetCollisionFlagsSphereCos(up, target.collider.bounds.center, mTopBottomColCos, contact.point);
                mCollFlags |= colFlag;

                if(colFlag == CollisionFlags.Below) {
                    mSlide = Vector3.Angle(up, contact.normal) > slopLimit;
                }

                mColls.Add(contact.otherCollider, new CollideInfo() { flag = colFlag, normal = contact.normal });
            }
        }
    }

    //void OnCollisionStay(Collision col) {
        /*if(mUpdateCollisionFlags) {
            Vector3 up = target.up;

            foreach(ContactPoint contact in col.contacts) {
                mColls.Add(contact.otherCollider);

                //determine flags
                CollisionFlags colFlag = M8.PhysicsUtil.GetCollisionFlagsSphereCos(up, target.collider.bounds.center, mTopBottomColCos, contact.point);
                mCollFlags |= colFlag;

                if(colFlag == CollisionFlags.Below) {
                    mSlide = Vector3.Angle(up, contact.normal) > slopLimit;
                }

                //Debug.Log("contact: " + contact.otherCollider.name);
            }

            mUpdateCollisionFlags = false;
        }*/

        //Debug.Log("flags: " + mCollFlags);
    //}

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

        mCollFlags = CollisionFlags.None;
        mSlide = false;

        Vector3 up = target.up;

        foreach(KeyValuePair<Collider, CollideInfo> pair in mColls) {
            //ContactPoint contact = pair.Value;

            //CollisionFlags colFlag = M8.PhysicsUtil.GetCollisionFlagsSphereCos(up, target.collider.bounds.center, mTopBottomColCos, contact.point);
            mCollFlags |= pair.Value.flag;

            if(pair.Value.flag == CollisionFlags.Below) {
                mSlide = Vector3.Angle(up, pair.Value.normal) > slopLimit;
            }
        }
    }

    void OnDestroy() {
        inputEnabled = false;
    }

    void Awake() {
        if(target == null)
            target = transform;

        mTopBottomColCos = Mathf.Cos(topBottomCollisionAngle);
    }

    // Use this for initialization
    void Start() {
        inputEnabled = startInputEnabled;
    }

    // Update is called once per frame
    void FixedUpdate() {
#if UNITY_EDITOR
        mTopBottomColCos = Mathf.Cos(topBottomCollisionAngle);
#endif

        Rigidbody body = target.rigidbody;
        Quaternion rot = target.rotation;

        if(mInputEnabled) {
            InputManager input = Main.instance.input;

            mCurInputMoveAxis.x = moveInputX != InputManager.ActionInvalid ? input.GetAxis(player, moveInputX) : 0.0f;
            mCurInputMoveAxis.y = moveInputY != InputManager.ActionInvalid ? input.GetAxis(player, moveInputY) : 0.0f;
            mCurInputTurnAxis = turnInput != InputManager.ActionInvalid ? input.GetAxis(player, turnInput) : 0.0f;

            if(mCurInputTurnAxis != 0.0f) {
                rot *= Quaternion.AngleAxis(mCurInputTurnAxis * turnSensitivity, Vector3.up);
                body.MoveRotation(rot);
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

                    if(body.velocity.sqrMagnitude < moveMaxSpeed * moveMaxSpeed) {
                        body.AddForce(rot * Vector3.forward * mCurInputMoveAxis.y * moveForce);
                        body.AddForce(rot * Vector3.right * mCurInputMoveAxis.x * moveForce);
                    }
                }
                else {//we are standing, check if we are only colliding below and not 'sliding'
                    if(mCollFlags == CollisionFlags.Below && !mSlide)
                        body.drag = mJump ? airDrag : standDrag;
                    else
                        body.drag = mJump ? airDrag : groundDrag;
                }
            }
            else {
                body.drag = airDrag;

                if(mCurInputMoveAxis != Vector2.zero) {
                    if(body.velocity.sqrMagnitude < moveMaxSpeed * moveMaxSpeed) {
                        body.AddForce(rot * Vector3.forward * mCurInputMoveAxis.y * moveAirForce);
                        body.AddForce(rot * Vector3.right * mCurInputMoveAxis.x * moveAirForce);
                    }
                }
            }
        }
        else {
            mCurInputMoveAxis = Vector2.zero;
            mCurInputTurnAxis = 0.0f;
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
}
