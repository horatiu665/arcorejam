using GoogleARCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class HarRaycaster : MonoBehaviour
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

    [Header("Raycast settings")]
    public float raycastWidth = 0; // turns it into a spherecast at width >0
    //public float coneAngleDeg = 15f; // degrees
    public float maxRaycastDistance = 50f;

    private RaycastHit[] raycastHitCache = new RaycastHit[10];

    [Header("Layers for these raycasts")]
    public LayerMask layers = -1;

    // EVENTS
    public event System.Action<ToothpickPlaceable> OnRaycastOverObject;
    public event System.Action OnRaycastOverNothing;

    public event OnTapOnObjectDelegate OnTapOnObject;
    public delegate void OnTapOnObjectDelegate(HarInputManageAR.TouchData touchData, Ray ray, RaycastHit rh, ToothpickPlaceable tp);

    public event OnTapOverNothingDelegate OnTapOverNothing;
    public delegate void OnTapOverNothingDelegate(HarInputManageAR.TouchData touchData, Ray ray);

    private void OnEnable()
    {
        inputMan.OnUpdateTouch += InputMan_OnUpdateTouch;
    }

    private void OnDisable()
    {
        inputMan.OnUpdateTouch -= InputMan_OnUpdateTouch;
    }

    private void InputMan_OnUpdateTouch(HarInputManageAR.TouchData touchData)
    {
        // raycast in middle. Trigger events based on the found object...
        var ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        Vector2 midScreen = new Vector2(Screen.width / 2, Screen.height / 2);
        int colliderCount = GetRaycast_DidWeHitAnyCollider(ray);
        if (colliderCount > 0)
        {
            // find nearest collider in raycast
            var rh = FindNearestRaycast(ref ray, colliderCount);

            // the obj.
            var hitCollider = rh.collider;
            var tp = ToothpickPlaceable.Get(hitCollider);

            if (tp != null)
            {
                OnRaycastOverObject?.Invoke(tp);

                if (touchData.touchDown)
                {
                    OnTapOnObject?.Invoke(touchData, ray, rh, tp);
                }
            }
            else
            {
                colliderCount = 0; // we can say we did not find anything of interest. trigger tap on AR plane.
            }
        }

        // if didn't touch an existing obj
        if (colliderCount == 0)
        {
            OnRaycastOverNothing?.Invoke();

            if (touchData.touchDown)
            {
                TestRaycastOnARShit(midScreen, touchData, ray);

                // can use to deselect, spawn stuff or wahtever.
                OnTapOverNothing?.Invoke(touchData, ray);

            }
        }

        // deselect n shit?
    }

    private bool TestRaycastOnARShit(Vector2 midScreen, HarInputManageAR.TouchData touchData, Ray ray)
    {
        // Raycast against the location the player touched to search for planes.
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
            TrackableHitFlags.FeaturePointWithSurfaceNormal;

        if (Frame.Raycast(midScreen.x, midScreen.y, raycastFilter, out hit))
        {
            // Use hit pose and camera pose to check if hittest is from the
            // back of the plane, if it is, no need to create the anchor.
            if ((hit.Trackable is DetectedPlane) &&
                Vector3.Dot(mainCamera.transform.position - hit.Pose.position,
                    hit.Pose.rotation * Vector3.up) < 0)
            {
                Debug.Log("Hit at back of the current DetectedPlane");
            }
            else
            {
                OnTapOnArPlanes?.Invoke(touchData, ray, new RaycastARData()
                {
                    trackableHit = hit,
                });

                return true;
            }
        }

        return false;
    }

    public delegate void OnTapOnArPlanesDelegate(HarInputManageAR.TouchData touchData, Ray ray, RaycastARData raycastARData);
    public event OnTapOnArPlanesDelegate OnTapOnArPlanes;

    public struct RaycastARData
    {
        public TrackableHit trackableHit;
    }

    // returns collider count after ray/sphere cast
    private int GetRaycast_DidWeHitAnyCollider(Ray ray)
    {
        int colliderCount = 0;
        if (raycastWidth == 0)
        {
            colliderCount = Physics.RaycastNonAlloc(ray, raycastHitCache, maxRaycastDistance, layers);
            return colliderCount;
        }
        else
        {
            // THIS SHOULD BE CONECAST
            colliderCount = Physics.SphereCastNonAlloc(ray, raycastWidth, raycastHitCache, maxRaycastDistance, layers);
            return colliderCount;
        }
    }

    // parses the cache of raycasts, after a raycast query, and returns the nearest raycasthit to the origin ray...
    private RaycastHit FindNearestRaycast(ref Ray ray, int colliderCount)
    {
        int nearestIndex = 0;
        float minSqrMag = float.MaxValue;
        for (int i = 0; i < colliderCount; i++)
        {
            // if dist between hit point and ray origin is smaller than minimum, save it as new minimum
            var sqrMag = (raycastHitCache[i].point - ray.origin).sqrMagnitude;
            if (sqrMag < minSqrMag)
            {
                minSqrMag = sqrMag;
                nearestIndex = i;
            }
        }
        // nearest raycast saved here.
        var rh = raycastHitCache[nearestIndex];
        return rh;
    }

}