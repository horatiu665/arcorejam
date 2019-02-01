using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class FollowWithPhysics : MonoBehaviour
{
    [SerializeField]
    private Camera _mainCamera;
    public Camera mainCamera
    {
        get
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
            return _mainCamera;
        }
    }

    [SerializeField]
    private HarInputManageAR _inputMan;
    public HarInputManageAR inputMan
    {
        get
        {
            if (_inputMan == null)
            {
                _inputMan = GetComponent<HarInputManageAR>();
            }
            return _inputMan;
        }
    }


    [SerializeField]
    private Transform _leadingDummy;
    public Transform leadingDummy
    {
        get
        {
            if (_leadingDummy == null)
            {
                _leadingDummy = new GameObject("[dummy lead]").transform;
                _leadingDummy.transform.position = transform.position;
                _leadingDummy.transform.SetParent(mainCamera.transform);
            }
            return _leadingDummy;
        }
    }
    [SerializeField]
    private Rigidbody _r;
    public Rigidbody r
    {
        get
        {
            if (_r == null)
            {
                _r = GetComponent<Rigidbody>();
            }
            return _r;
        }
    }

    public Transform centerOfGravity;
    bool selected = false;

    [Header("RB grab settings")]
    public bool notGrabbed_kinematic = true;
    public float lerpFactor = 1f;

    public AnimationCurve distToForceMapping = new AnimationCurve() { keys = new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 0, 0) } };
    public float maxDistForMapping = 1f;
    public float forceMulti = 1f;

    private void Select()
    {
        selected = true;
        r.useGravity = false;
        r.isKinematic = false;
        leadingDummy.transform.SetParent(mainCamera.transform);
    }

    private void Deselect()
    {
        selected = false;
        r.useGravity = false;

        leadingDummy.SetParent(null);

    }



    private void OnEnable()
    {
        inputMan.OnUpdateTouch += InputMan_OnUpdateTouch;
    }

    private void OnDisable()
    {
        inputMan.OnUpdateTouch -= InputMan_OnUpdateTouch;
    }

    private void InputMan_OnUpdateTouch(HarInputManageAR.TouchData data)
    {
        latestData = data;
    }

    void Start()
    {
        Select();

        //r.centerOfMass = transform.InverseTransformPoint(centerOfGravity.transform.position);
    }


    void FixedUpdate()
    {
        if (selected)
        {
            MoveRb(r);
            Rotate1();
        }
    }


    private void MoveRb(Rigidbody r)
    {
        // accelerate rb towards pointer...?
        var dir = leadingDummy.position - r.position;
        Vector3 normalizedForce = dir.normalized;
        float distanceFromControlTransform = dir.magnitude;

        dir = normalizedForce * distToForceMapping.Evaluate(distanceFromControlTransform / maxDistForMapping);

        // Set the desired max velocity for the rigidbody.
        Vector3 targetVelocity = dir * forceMulti;

        // Have the rigidbody accelerate until it reaches target velocity.
        float timeStep = Mathf.Clamp01(Time.fixedDeltaTime * lerpFactor * 8.0f);
        r.velocity += timeStep * (targetVelocity - r.velocity);
    }









    [Header("Rotation settings")]
    public float rotationForceMulti = 10f;

    [SerializeField]
    private GameObject _rotationDummy;
    public GameObject rotationDummy
    {
        get
        {
            if (_rotationDummy == null)
            {
                _rotationDummy = new GameObject("[RotationDummy]");
            }
            return _rotationDummy;
        }
    }

    [SerializeField]
    private GameObject _rotationDummyChild;
    private HarInputManageAR.TouchData latestData;

    public GameObject rotationDummyChild
    {
        get
        {
            if (_rotationDummyChild == null)
            {
                _rotationDummyChild = new GameObject("[RotationDummyChild]");
            }
            return _rotationDummyChild;
        }
    }

    // the rotation shit, that depends on the prev. touch, and swipy input xy
    private void Rotate1()
    {
        var finalRot = leadingDummy.transform.rotation;
        RotateRigidbody(r, finalRot, this.transform.rotation);

    }

    // Sets the angular velocity of the rigidbody.
    private void RotateRigidbody(Rigidbody r, Quaternion finalRot, Quaternion curRot)
    {
        float angle;
        Vector3 axis;
        // Get the delta between the control transform rotation and the rigidbody.
        Quaternion rigidbodyRotationDelta = finalRot *
                                            Quaternion.Inverse(curRot);
        // Convert this rotation delta to values that can be assigned to rigidbody
        // angular velocity.
        rigidbodyRotationDelta.ToAngleAxis(out angle, out axis);
        // Set the angular velocity of the rigidbody so it rotates towards the
        // control transform.
        float timeStep = Mathf.Clamp01(Time.fixedDeltaTime * 8f * rotationForceMulti);
        r.angularVelocity = timeStep * angle * axis;
    }


}