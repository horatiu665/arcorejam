using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ToothpickPhysicsMovement : MonoBehaviour
{
    [SerializeField]
    private ToothpickPlacerTest _placer;
    public ToothpickPlacerTest placer
    {
        get
        {
            if (_placer == null)
            {
                _placer = GetComponent<ToothpickPlacerTest>();
            }
            return _placer;
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
    public float forceMulti = 1f;

    private void OnEnable()
    {
        placer.OnClickObjectDown += Placer_OnClickObjectDown;
        placer.OnClickObjectUp += Placer_OnClickObjectUp;
    }

    private void OnDisable()
    {
        placer.OnClickObjectDown -= Placer_OnClickObjectDown;
        placer.OnClickObjectUp -= Placer_OnClickObjectUp;
    }

    private void Placer_OnClickObjectDown(Vector2 touchPos, Ray ray, RaycastHit rh, ToothpickPlaceable toothpick)
    {
        Select(toothpick);
        TestLaser.SetSelected(toothpick.transform, toothpick.transform.InverseTransformPoint(rh.point));
    }

    private void Placer_OnClickObjectUp(Vector2 touchPos, Ray ray, ToothpickPlaceable toothpick)
    {
        Deselect();
        TestLaser.ReleaseAll();
    }

    private void Select(ToothpickPlaceable toothpick)
    {
        curSelected = toothpick;
        curSelected.rigidbody.useGravity = false;
        curSelected.rigidbody.isKinematic = false;
        leadingDummy.transform.position = curSelected.transform.position;
        leadingDummy.transform.SetParent(placer.mainCamera.transform);
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


}