using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//TODO: right now it assumes a sphere collider
public class FPMoveController : MonoBehaviour {
    public Transform dirHolder; //the forward vector of this determines our forward movement

    public float moveForce = 25.0f;
    public float moveAirForce = 10.0f;
    public float moveMaxSpeed = 3.5f;

    public float slopSlideForce = 25.0f;

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
        public Vector3 contactPoint;
        public Vector3 normal;

    }

    private Vector2 mCurInputMoveAxis;
    private float mCurInputTurnAxis;
    private Vector3 mCurMoveDir;

    //private HashSet<Collider> mColls = new HashSet<Collider>();
    private Dictionary<Collider, CollideInfo> mColls = new Dictionary<Collider, CollideInfo>(16);

    private bool mInputEnabled = false;

    private CollisionFlags mCollFlags;
    
    private float mTopBottomColCos;
    private bool mJump = false;
    private float mJumpLastTime = 0.0f;

    private bool mIsSlopSlide;
    private Vector3 mSlopNormal;

    private Vector3 mGroundMoveVel;

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
        Vector3 pos = collider.bounds.center;
        
        foreach(ContactPoint contact in col.contacts) {
            if(!mColls.ContainsKey(contact.otherCollider)) {
                //mColls.Add(contact.otherCollider, contact);

                CollisionFlags colFlag = M8.PhysicsUtil.GetCollisionFlagsSphereCos(up, pos, mTopBottomColCos, contact.point);

                mColls.Add(contact.otherCollider, new CollideInfo() { flag = colFlag, normal = contact.normal, contactPoint = contact.point });
            }
        }

        RefreshCollInfo();
    }

    void OnCollisionStay(Collision col) {
        Vector3 up = transform.up;
        Vector3 pos = collider.bounds.center;

        //refresh contact infos
        foreach(ContactPoint contact in col.contacts) {
            if(mColls.ContainsKey(contact.otherCollider)) {
                CollisionFlags colFlag = M8.PhysicsUtil.GetCollisionFlagsSphereCos(up, pos, mTopBottomColCos, contact.point);

                mColls[contact.otherCollider] = new CollideInfo() { flag = colFlag, normal = contact.normal, contactPoint = contact.point };
            }
        }

        //recalculate flags
        RefreshCollInfo();
    }

    void OnCollisionExit(Collision col) {
        foreach(ContactPoint contact in col.contacts) {
            if(mColls.ContainsKey(contact.otherCollider))
                mColls.Remove(contact.otherCollider);
        }

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

            //check for slope slide
            if(mIsSlopSlide) {
                body.drag = groundDrag;

                Vector3 dir = M8.MathUtil.Slide(-transform.up, mSlopNormal);
                dir.Normalize();
                body.AddForce(dir * slopSlideForce);
            }
            else {
                //move
                if(isGrounded) {
                    if(Move(moveForce)) {
                        body.drag = mJump ? airDrag : groundDrag;
                    }
                    else {//we are standing
                        if(mJump) //allow jump to move body
                            body.drag = airDrag;
                        else if(mCollFlags != CollisionFlags.Below)
                            body.drag = groundDrag;
                        else
                            body.drag = standDrag;
                    }
                }
                else {
                    body.drag = airDrag;

                    Move(moveAirForce);
                }
            }
        }
        else {
            mCurInputMoveAxis = Vector2.zero;
            mCurInputTurnAxis = 0.0f;
            mCurMoveDir = Vector3.zero;
        }
    }

    //return true if we moved
    bool Move(float force) {
        if(mCurInputMoveAxis != Vector2.zero) {
            //compute move direction
            Quaternion dirRot = dirHolder.rotation;
            Vector3 moveDelta = dirRot * Vector3.forward * mCurInputMoveAxis.y;
            moveDelta += dirRot * Vector3.right * mCurInputMoveAxis.x;

            mCurMoveDir = moveDelta.normalized;

            //check if we need to slide off walls
            foreach(KeyValuePair<Collider, CollideInfo> pair in mColls) {
                if(pair.Value.flag == CollisionFlags.Sides || pair.Value.flag == CollisionFlags.Below) {
                    if(Vector3.Dot(mCurMoveDir, pair.Value.normal) < 0.0f) {
                        moveDelta = M8.MathUtil.Slide(mCurMoveDir, pair.Value.normal);
                        break;
                    }
                }
            }

            //check if we can move based on speed or if going against new direction
            Vector3 vel = rigidbody.velocity - mGroundMoveVel;
            //if( < 0.0f)
                //Debug.Log("wtf: "+Vector3.Angle(vel, mCurMoveDir));

            float velMagSqr = vel.sqrMagnitude;
            bool canMove = velMagSqr < moveMaxSpeed * moveMaxSpeed;
            if(!canMove) { //see if we are trying to move the opposite dir
                Vector3 velDir = vel / Mathf.Sqrt(velMagSqr);
                canMove = Vector3.Dot(mCurMoveDir, velDir) < 0.0f;
            }

            if(canMove) {
                rigidbody.AddForce(moveDelta * force);
                return true;
            }
        }
        else
            mCurMoveDir = Vector3.zero;

        return false;
    }

    void OnInputJump(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            if(isGrounded && !mJump && !mIsSlopSlide) {
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
        mIsSlopSlide = false;
        mGroundMoveVel = Vector3.zero;

        bool groundNoSlope = false; //prevent slope slide if we are also touching a non-slidable ground (standing on the corner base of slope)

        Vector3 up = transform.up;
        //
        foreach(KeyValuePair<Collider, CollideInfo> pair in mColls) {
            Vector3 n = pair.Value.normal;
            CollisionFlags flag = pair.Value.flag;

            mCollFlags |= pair.Value.flag;

            if(flag == CollisionFlags.Below) {
                if(!groundNoSlope) {
                    mIsSlopSlide = Vector3.Angle(up, n) > slopLimit;
                    if(mIsSlopSlide)
                        mSlopNormal = n;
                    else
                        groundNoSlope = true;
                }

                //for platforms
                Rigidbody body = pair.Key.rigidbody;
                if(body != null && body.velocity != Vector3.zero) {
                    mGroundMoveVel += body.velocity;
                }
            }
        }
    }
}
