using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ToothpickPhysicsMoverHar : MonoBehaviour
{
    [SerializeField]
    private ToothpickPlacer _placer;
    public ToothpickPlacer placer
    {
        get
        {
            if (_placer == null)
            {
                _placer = GetComponent<ToothpickPlacer>();
            }
            return _placer;
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
    private HarRaycaster _raycaster;
    public HarRaycaster raycaster
    {
        get
        {
            if (_raycaster == null)
            {
                _raycaster = GetComponent<HarRaycaster>();
            }
            return _raycaster;
        }
    }

    [SerializeField]
    private TestLaser _laser;
    private ToothpickPlaceable curSelected;

    public TestLaser laser
    {
        get
        {
            if (_laser == null)
            {
                _laser = FindObjectOfType<TestLaser>();
            }
            return _laser;
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

    [Header("RB grab settings")]
    public bool notGrabbed_kinematic = true;
    public float lerpFactor = 1f;

    public AnimationCurve distToForceMapping = new AnimationCurve() { keys = new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 0, 0) } };
    public float maxDistForMapping = 1f;
    public float forceMulti = 20f;


    [Header("Rotation settings")]
    public Vector2 rotationSpeed = new Vector2(-100, 100);
    public float maxDeltaTouchForRotation = 100f;
    private bool scaleMode = false;
    public AnimationCurve rotationMapping = new AnimationCurve() { keys = new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 0, 0) } };
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


    private void OnEnable()
    {
        raycaster.OnTapOnObject += Raycaster_OnTapOnObject;
        inputMan.OnUpdateTouch += InputMan_OnUpdateTouch;
    }

    private void OnDisable()
    {
        raycaster.OnTapOnObject -= Raycaster_OnTapOnObject;
        inputMan.OnUpdateTouch -= InputMan_OnUpdateTouch;
    }

    private void InputMan_OnUpdateTouch(HarInputManageAR.TouchData data)
    {
        latestData = data;

        if (data.touchUp)
        {
            // release and deselect..

            Deselect();
            TestLaser.ReleaseAll();
        }
    }
    
    private void Raycaster_OnTapOnObject(HarInputManageAR.TouchData touchData, Ray ray, RaycastHit rh, ToothpickPlaceable tp)
    {
        Select(tp);
        TestLaser.SetSelected(tp.transform, tp.transform.InverseTransformPoint(rh.point));
    }

    private void Select(ToothpickPlaceable toothpick)
    {
        curSelected = toothpick;
        curSelected.rigidbody.useGravity = false;
        curSelected.rigidbody.isKinematic = false;
        leadingDummy.transform.position = curSelected.transform.position;
        leadingDummy.transform.SetParent(inputMan.mainCamera.transform);
    }

    private void Deselect()
    {
        if (curSelected != null)
        {
            curSelected.rigidbody.useGravity = false;
            curSelected.rigidbody.isKinematic = true;
        }

        curSelected = null;

        leadingDummy.SetParent(null);

    }

    private void Update()
    {
        if (curSelected != null)
        {
            var r = curSelected.rigidbody;
            // rotate RB

            // moverb
            MoveRb(r);

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


    // trigger rotation when touching and selected...
    private void FixedUpdate()
    {
        if (curSelected != null)
        {
            Rotate1();
        }
    }

    // the rotation shit, that depends on the prev. touch, and swipy input xy
    private void Rotate1()
    {
        // do rotation shit...
        var delta = latestData.touchPosition - latestData.prevTouchPosition;
        var maxDelta = maxDeltaTouchForRotation * 1080 * Mathf.Min(Screen.width, Screen.height);
        var rotationInput = new Vector2(
            rotationMapping.Evaluate(Mathf.Abs(delta.x) / maxDeltaTouchForRotation) * Mathf.Sign(delta.x) * rotationSpeed.x,
            rotationMapping.Evaluate(Mathf.Abs(delta.y) / maxDeltaTouchForRotation) * Mathf.Sign(delta.y) * rotationSpeed.y);

        var s = curSelected;
        if (s != null)
        {
            var dummy = rotationDummy.transform;
            var dummyChild = rotationDummyChild.transform;
            dummy.position = s.transform.position;
            dummy.rotation = inputMan.mainCamera.transform.rotation;
            dummyChild.position = s.transform.position;
            dummyChild.rotation = s.transform.rotation;
            dummyChild.SetParent(dummy);
            var initRot = dummy.rotation;
            dummy.Rotate(new Vector3(rotationInput.y, 0, 0), Space.Self);
            //dummy.Rotate(mainCamera.transform.right, rotationInput.y);
            dummy.Rotate(Vector3.up, rotationInput.x);

            dummyChild.SetParent(null);

            RotateRigidbody(s.rigidbody, dummyChild.rotation, s.transform.rotation);

        }
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