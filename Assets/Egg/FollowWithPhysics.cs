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
    private Transform _leadingDummy;
    public Transform leadingDummy
    {
        get
        {
            if (_leadingDummy == null)
            {
                _leadingDummy = new GameObject("[dummy lead]").transform;
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
        leadingDummy.transform.position = transform.position;
        leadingDummy.transform.SetParent(mainCamera.transform);
    }

    private void Deselect()
    {
        selected = false;
        r.useGravity = false;

        leadingDummy.SetParent(null);

    }

    void Start()
    {
        Select();
    }

    void FixedUpdate()
    {
        if (selected)
            MoveRb(r);
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

}