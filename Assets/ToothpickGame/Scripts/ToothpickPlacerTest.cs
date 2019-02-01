using System;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

/// <summary>
/// Controls the HelloAR example.
/// </summary>
public class ToothpickPlacerTest : MonoBehaviour
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

    public HashSet<GameObject> selection = new HashSet<GameObject>();
    
    public GameObject toothpickPrefab;

    private Vector3 curTouchPos;
    private Vector3 prevTouchPos;
    
    public HashSet<ToothpickPlaceable> highlighted = new HashSet<ToothpickPlaceable>();

    [Header("Raycast settings")]
    public float raycastWidth = 0; // turns it into a spherecast at width >0
    public float coneAngleDeg = 15f; // degrees
    public float maxRaycastDistance = 50f;

    [Header("Rotation settings")]
    public Vector2 rotationSpeed = new Vector2(1, 1);
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

    [Header("Click area. xy big is top right.")]
    public Rect clickAreaInScreens = new Rect(0, 0.1f, 0, 1f);
    private Rect clickArea;

    public event RaycasterDelegate OnClickPlanesDown;
    public event RaycasterDelegate OnClickObjectDown;
    public event ReleaseDelegate OnClickObjectUp;
    public delegate void RaycasterDelegate(Vector2 touchPos, Ray ray, RaycastHit rh, ToothpickPlaceable toothpick);
    public delegate void ReleaseDelegate(Vector2 touchPos, Ray ray, ToothpickPlaceable toothpick);

    private RaycastHit[] raycastHitCache = new RaycastHit[10];

    [Header("Layers for selecting toothpicks")]
    public LayerMask layers = -1;
    
    private void Update()
    {
        // touchesss
        var touchDown = false;
        var touchUp = false;
        var touching = false;
        var touchPosition = Vector3.zero;
        {
            // default mouse pos
            if (Input.GetMouseButton(0))
            {
                touchPosition = Input.mousePosition;
                touching = true;
            }

            // mouse down n up
            if (Input.GetMouseButtonDown(0))
            {
                touchDown = true;
                touchPosition = Input.mousePosition;
                curTouchPos = touchPosition;
                prevTouchPos = touchPosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                touchUp = true;
                touchPosition = Input.mousePosition;
            }

            // touch overrides this shit
            if (Input.touchCount > 0)
            {
                for (int i = Input.touchCount - 1; i >= 0; i--)
                {
                    var t = Input.GetTouch(i);
                    if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    {
                        touchUp = true;
                        touchPosition = t.position;
                    }
                    else if (t.phase == TouchPhase.Began)
                    {
                        touchDown = true;
                        touchPosition = t.position;
                        curTouchPos = touchPosition;
                        prevTouchPos = touchPosition;
                    }
                    else if (t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Moved)
                    {
                        touching = true;
                        touchPosition = t.position;
                    }
                }
                // prioritize touch up == true.
                if (touchDown && touchUp)
                {
                    touchDown = false;
                }

            }

            curTouching = touching;

            if ((touching || touchDown) && !clickArea.Contains(touchPosition))
            {
                Debug.Log("NOT: " + touchPosition);
                return;
            }

        }

        // raycast uin middle, find target obj
        var ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        Vector2 midScreen = new Vector2(Screen.width / 2, Screen.height / 2);
        int colliderCount = GetRaycast_DidWeHitAnyCollider(ray);
        if (colliderCount > 0)
        {
            // find nearest collider in raycast
            var rh = FindNearestRaycast(ref ray, colliderCount);

            // the obj
            var hitCollider = rh.collider;
            var tp = ToothpickPlaceable.Get(hitCollider);

            if (tp != null)
            {
                // highlight shit
                HandleHighlighting(tp);

                // if input, do stuff
                // touch shit
                if (touchDown)
                {
                    HandleClickDown(touchPosition, ray, rh, tp);
                }
            }
            else
            {
                colliderCount = 0;
            }
        }

        // if didn't touch an existing obj
        if (colliderCount == 0)
        {
            HandleHighlighting(null);

            if (touchDown)
            {
                // ray from mid of screen...??? or mouse input?
                var touchPos = new Vector2(Screen.width / 2, Screen.height / 2);
                var spawnedToothpick = DoSpawnStuff(touchPos);

                if (spawnedToothpick != null)
                {
                    // now we spawned something, so we should also grab it immediately
                    // THE RAYCASTHIT RH IS PROBABLY FUCKED HERE. REMEMBER TO FIX WHEN USING LOL.
                    Select(spawnedToothpick.gameObject);
                    HandleHighlighting(spawnedToothpick);
                }
            }
        }

        if (touchUp)
        {
            HandleClickUp(touchPosition, ray, null);
            HandleHighlighting(null);
        }
        
        // at end, set prev touch position for next frame
        prevTouchPos = curTouchPos;
        curTouchPos = touchPosition;
        debug_lastTouchPos = touchPosition;
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

    // trigger rotation when touching and selected...
    private void FixedUpdate()
    {
        if (curTouching)
        {
            if (selection.Count > 0)
                Rotate1();
        }
    }

    // the rotation shit, that depends on the prev. touch, and swipy input xy
    private void Rotate1()
    {
        // do rotation shit...
        var delta = curTouchPos - prevTouchPos;
        var maxDelta = maxDeltaTouchForRotation * 1080 * Mathf.Min(Screen.width, Screen.height);
        var rotationInput = new Vector2(
            rotationMapping.Evaluate(Mathf.Abs(delta.x) / maxDeltaTouchForRotation) * Mathf.Sign(delta.x) * rotationSpeed.x,
            rotationMapping.Evaluate(Mathf.Abs(delta.y) / maxDeltaTouchForRotation) * Mathf.Sign(delta.y) * rotationSpeed.y);

        foreach (var s in selection)
        {
            var dummy = rotationDummy.transform;
            var dummyChild = rotationDummyChild.transform;
            dummy.position = s.transform.position;
            dummy.rotation = mainCamera.transform.rotation;
            dummyChild.position = s.transform.position;
            dummyChild.rotation = s.transform.rotation;
            dummyChild.SetParent(dummy);
            var initRot = dummy.rotation;
            dummy.Rotate(new Vector3(rotationInput.y, 0, 0), Space.Self);
            //dummy.Rotate(mainCamera.transform.right, rotationInput.y);
            dummy.Rotate(Vector3.up, rotationInput.x);

            dummyChild.SetParent(null);

            RotateRigidbody(s.GetComponent<Rigidbody>(), dummyChild.rotation, s.transform.rotation);

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

    private void OnEnable()
    {
        clickArea = new Rect(clickAreaInScreens.x * Screen.width, clickAreaInScreens.y * Screen.height, clickAreaInScreens.width * Screen.width, clickAreaInScreens.height * Screen.height);

    }

    // returns collider count
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

    private void HandleClickUp(Vector2 touchPos, Ray ray, ToothpickPlaceable tp)
    {
        Deselect();

        if (OnClickObjectUp != null)
        {
            OnClickObjectUp(touchPos, ray, tp);
        }
    }

    private void HandleClickDown(Vector2 touchPos, Ray ray, RaycastHit rh, ToothpickPlaceable tp)
    {
        Select(tp.gameObject);

        if (OnClickObjectDown != null)
        {
            OnClickObjectDown(touchPos, ray, rh, tp);
        }
    }

    private void HandleHighlighting(ToothpickPlaceable tp)
    {
        if (selection.Count > 0)
        {
            foreach (var s in selection)
            {
                var selTp = ToothpickPlaceable.Get(s);
                selTp.Highlight(true);
                highlighted.Add(selTp);
            }
            return;
        }

        // clear formerly highlighted, except the current one.
        foreach (var h in highlighted)
        {
            if (h != tp)
            {
                h.Unhighlight();
            }
        }
        highlighted.Clear();

        if (tp != null)
        {
            tp.Highlight();
            highlighted.Add(tp);
        }

    }
    public void RefreshHighlightingExternal()
    {
        HandleHighlighting(null);

    }
    
    // Deselect = Select(null)
    public void Deselect()
    {
        Select(null);
    }

    public void Select(GameObject toothpick, bool additive = false)
    {
        // null is like deselect.
        if (toothpick == null)
        {
            //// unparent
            //foreach (var s in selection)
            //{
            //    s.transform.SetParent(null);
            //}
            // clear list.
            selection.Clear();
            return;
        }

        // add/remove to selection list
        if (additive)
        {
            selection.Add(toothpick);
        }
        else
        {
            selection.Clear();
            selection.Add(toothpick);
        }

        //// parent selection list - could have been just this. or just the actual object...
        //foreach (var s in selection)
        //{
        //    s.transform.SetParent(mainCamera.transform);

        //}

    }

    private ToothpickPlaceable DoSpawnStuff(Vector2 touchPos)
    {
        // Raycast against the location the player touched to search for planes.
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
            TrackableHitFlags.FeaturePointWithSurfaceNormal;

        if (Frame.Raycast(touchPos.x, touchPos.y, raycastFilter, out hit))
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
                // Choose the Andy model for the Trackable that got hit.
                GameObject prefab;
                prefab = toothpickPrefab;

                // Instantiate Andy model at the hit pose.
                var andyObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

                // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                // world evolves.
                var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                // Make Andy model a child of the anchor.
                andyObject.transform.parent = anchor.transform;

                return andyObject.GetComponent<ToothpickPlaceable>();
            }
        }

        return null;
    }
    
    private Vector2 debug_lastTouchPos;
    private bool curTouching;
    //private void OnGUI()
    //{
    //    //GUI.Label(new Rect(10, 10, 300, 100), debug_lastTouchPos.ToString());
    //}
}
