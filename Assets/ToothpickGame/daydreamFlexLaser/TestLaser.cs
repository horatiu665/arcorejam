using DaydreamElements.ObjectManipulation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class TestLaser : GvrBasePointer
{
    /// Distance from the pointer that raycast hits will be detected.
    [Tooltip("Distance from the pointer that raycast hits will be detected.")]
    public float maxPointerDistance = 20.0f;

    /// Distance from the pointer that the reticle will be drawn at when hitting nothing.
    [Tooltip("Distance from the pointer that the reticle will be drawn at when hitting nothing.")]
    public float defaultReticleDistance = 20.0f;

    /// Distance from the pointer that the line will be drawn at when hitting nothing.
    [Tooltip("Distance from the pointer that the line will be drawn at when hitting nothing.")]
    public float defaultLineDistance = 0.3f;

    /// The laser visual used by the pointer.
    [Tooltip("The laser visual used by the pointer.")]
    public FlexLaserVisual flexLaserVisual;

    /// The reticle used by the pointer.
    [Tooltip("The reticle used by the pointer.")]
    public FlexReticle reticle;

    private bool isHittingTarget;

    private static Transform selectedTransform;
    private static Vector3 selectedPoint;

    public static void SetSelected(Transform t, Vector3 localSpaceHitPosition)
    {
        selectedTransform = t;
        selectedPoint = localSpaceHitPosition;
    }

    public static void ReleaseAll()
    {
        selectedTransform = null;
    }

    public static void ReleaseSelected(Transform t)
    {
        if (selectedTransform == t)
        {
            selectedTransform = null;
        }
    }

    public static bool IsObjectSelected()
    {
        return selectedTransform != null;
    }

    public override float MaxPointerDistance
    {
        get
        {
            return maxPointerDistance;
        }
    }

    public override void GetPointerRadius(out float enterRadius, out float exitRadius)
    {
        enterRadius = 0.1f;
        exitRadius = 0.1f;
    }

    public override void OnPointerEnter(RaycastResult raycastResult, bool isInteractive)
    {
        OnPointerHover(raycastResult, isInteractive);
    }

    public override void OnPointerHover(RaycastResult raycastResult, bool isInteractive)
    {
        if (!IsObjectSelected())
        {
            reticle.SetTargetPosition(raycastResult.worldPosition, PointerTransform.position);
            Vector3 ray = raycastResult.worldPosition - PointerTransform.position;
            if (flexLaserVisual != null)
            {
                if (ray.magnitude < defaultLineDistance)
                {
                    flexLaserVisual.SetReticlePoint(raycastResult.worldPosition);
                }
                else
                {
                    SetLaserToDefaultDistance();
                }
            }
            isHittingTarget = true;
        }
    }

    public override void OnPointerExit(GameObject previousObject)
    {
        isHittingTarget = false;
    }

    public override void OnPointerClickDown()
    {
    }

    public override void OnPointerClickUp()
    {
    }

    protected override void Start()
    {
        base.Start();
    }
    
    void Update()
    {
        if (!isHittingTarget)
        {
            reticle.SetTargetPosition(GetPointAlongPointer(defaultReticleDistance), PointerTransform.position);
            SetLaserToDefaultDistance();
        }
        if (IsObjectSelected())
        {
            reticle.Hide();
            float dist = (selectedTransform.position - PointerTransform.position).magnitude;
            flexLaserVisual.SetReticlePoint(GetPointAlongPointer(dist));
        }
        else
        {
            reticle.Show();
        }

        flexLaserVisual.UpdateVisual(selectedTransform, selectedPoint);
    }
    
    private void SetLaserToDefaultDistance()
    {
        Vector3 direction = (reticle.TargetPosition - PointerTransform.position).normalized;
        Vector3 laserPoint = transform.position + (direction * defaultLineDistance);

        flexLaserVisual.SetReticlePoint(laserPoint);
    }
}