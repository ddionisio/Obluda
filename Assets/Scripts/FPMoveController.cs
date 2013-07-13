using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//TODO: right now it assumes a sphere collider
public class FPMoveController : MonoBehaviour {
    public Transform target;

    public float force = 5.0f;
    public float jumpSpeed = 10.0f;
    public float maxSpeed = 50.0f;
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

    public bool startInputEnabled = false;

    private Vector2 mCurInputMoveAxis;
    private float mCurInputTurnAxis;

    private HashSet<Collider> mColls = new HashSet<Collider>();

    private bool mInputEnabled = false;

    private CollisionFlags mCollFlags;

    private bool mSlide = false;
    private float mTopBottomColCos;

    public CollisionFlags collisionFlags { get { return mCollFlags; } }
    public bool isGrounded { get { return (mCollFlags & CollisionFlags.Below) != 0; } }

    public bool inputEnabled {
        get { return mInputEnabled; }
        set {
            if(mInputEnabled != value) {
                mInputEnabled = value;

                InputManager input = Main.instance != null ? Main.instance.input : null;
                if(input != null) {
                    if(mInputEnabled) {
                    }
                    else {
                    }
                }
            }
        }
    }

    void OnCollisionEnter(Collision col) {
        //refresh during stay
        mColls.Clear();
    }

    void OnCollisionStay(Collision col) {
        mCollFlags = CollisionFlags.None;
        mSlide = false;

        Vector3 up = target.up;

        foreach(ContactPoint contact in col.contacts) {
            mColls.Add(contact.otherCollider);

            //determine flags
            CollisionFlags colFlag = M8.PhysicsUtil.GetCollisionFlagsSphereCos(up, target.collider.bounds.center, mTopBottomColCos, contact.point);
            mCollFlags |= colFlag;

            if(colFlag == CollisionFlags.Below) {
                mSlide = Vector3.Angle(up, contact.normal) > slopLimit;
            }
        }
    }

    void OnCollisionExit(Collision col) {
        foreach(ContactPoint contact in col.contacts) {
            mColls.Remove(contact.otherCollider);
        }

        if(mColls.Count == 0) {
            mCollFlags = CollisionFlags.None;
            mSlide = false;
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

            if(isGrounded) {
                if(mCurInputMoveAxis != Vector2.zero) {
                    body.drag = groundDrag;

                    if(body.rigidbody.velocity.sqrMagnitude < maxSpeed * maxSpeed) {
                        body.AddForce(rot * Vector3.forward * mCurInputMoveAxis.y * force);
                        body.AddForce(rot * Vector3.right * mCurInputMoveAxis.x * force);
                    }
                }
                else {//we are standing, check if we are only colliding below and not 'sliding'
                    if(mCollFlags == CollisionFlags.Below && !mSlide)
                        body.drag = standDrag;
                    else
                        body.drag = groundDrag;
                }
            }
            else {
                body.drag = airDrag;
            }
        }
        else {
            mCurInputMoveAxis = Vector2.zero;
            mCurInputTurnAxis = 0.0f;
        }
    }
}
