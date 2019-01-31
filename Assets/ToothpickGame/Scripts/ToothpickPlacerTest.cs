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

    HashSet<GameObject> selection = new HashSet<GameObject>();

    /// <summary>
    /// A game object parenting UI for displaying the "searching for planes" snackbar.
    /// </summary>
    public GameObject SearchingForPlaneUI;

    /// <summary>
    /// The rotation in degrees need to apply to model when the Andy model is placed.
    /// </summary>
    private const float k_ModelRotation = 180.0f;

    /// <summary>
    /// A list to hold all planes ARCore is tracking in the current frame. This object is used across
    /// the application to avoid per-frame allocations.
    /// </summary>
    private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();

    /// <summary>
    /// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
    /// </summary>
    private bool m_IsQuitting = false;

    public GameObject toothpickPrefab;

    public HashSet<ToothpickPlaceable> highlighted = new HashSet<ToothpickPlaceable>();

    [Header("Raycast settings")]
    public float raycastWidth = 0; // turns it into a spherecast at width >0
    public float coneAngleDeg = 15f; // degrees

    [Header("Rotation settings")]
    public Vector2 rotationSpeed = new Vector2(1, 1);
    public float maxDeltaTouchForRotation = 100f;
    private Vector3 prevTouchPos;
    private bool scaleMode = false;
    public AnimationCurve rotationMapping = new AnimationCurve() { keys = new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 0, 0) } };

    [Header("Click area. xy big is top right.")]
    public Rect clickAreaInScreens = new Rect(0, 0.1f, 0, 1f);
    private Rect clickArea;

    public event RaycasterDelegate OnClickObjectDown, OnClickObjectUp;
    public event RaycasterDelegate OnClickPlanesDown;
    public delegate void RaycasterDelegate(Vector2 touchPos, Ray ray, RaycastHit rh, ToothpickPlaceable toothpick);

    /// <summary>
    /// The Unity Update() method.
    /// </summary>
    public void UpdateOld()
    {
        _UpdateApplicationLifecycle();
        HandleSnackbar();

        // check if we are clicking n moving stuff...
        if (Input.GetMouseButtonDown(0) ||
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector2 innn = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

            // first check if we clicked a current spawned stuff
            var ray = this.mainCamera.ScreenPointToRay(innn);
            bool foundSelection = false;
            RaycastHit rh;
            if (Physics.Raycast(ray, out rh))
            {
                var hitCollider = rh.collider;
                var tp = ToothpickPlaceable.Get(hitCollider);
                // if the hit obj is a toothpick...
                if (tp != null)
                {
                    // we hit a toothpick when we clicked. Select it.
                    Select(tp.gameObject);
                    foundSelection = true;
                }
            }

            if (!foundSelection)
            {
                DoSpawnStuff(Input.mousePosition);
            }
        }
        else if (Input.GetMouseButtonUp(0) ||
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
        {
            Deselect();

        }

    }

    private void Update()
    {
        _UpdateApplicationLifecycle();
        HandleSnackbar();

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

            if ((touching || touchDown) && !clickArea.Contains(touchPosition))
            {
                Debug.Log("NOT: " + touchPosition);
                return;
            }

        }

        // raycast uin middle, find target obj
        var ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit rh;
        Vector2 midScreen = new Vector2(Screen.width / 2, Screen.height / 2);
        bool raycastHitSomething = GetRaycast(ray, out rh);
        if (raycastHitSomething)
        {
            // the obj
            var hitCollider = rh.collider;
            var tp = ToothpickPlaceable.Get(hitCollider);

            // highlight shit
            HandleHighlighting(tp);

            // if input, do stuff
            // touch shit
            if (touchDown)
            {
                HandleClickDown(touchPosition, ray, rh, tp);
            }
        }
        // if didn't touch an existing obj
        else
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
            HandleClickUp(touchPosition, ray, rh, null);
            HandleHighlighting(null);
        }


        if (touching)
        {
            if (selection.Count > 0)
            {
                // do rotation shit...
                var delta = touchPosition - prevTouchPos;
                var maxDelta = maxDeltaTouchForRotation * 1080 * Mathf.Min(Screen.width, Screen.height);
                var rotationInput = new Vector2(
                    rotationMapping.Evaluate(Mathf.Abs(delta.x) / maxDeltaTouchForRotation) * Mathf.Sign(delta.x) * rotationSpeed.x,
                    rotationMapping.Evaluate(Mathf.Abs(delta.y) / maxDeltaTouchForRotation) * Mathf.Sign(delta.y) * rotationSpeed.y);

                foreach (var s in selection)
                {
                    var dummy = rotationDummy.transform;
                    dummy.position = s.transform.position;
                    dummy.rotation = mainCamera.transform.rotation;
                    var oldParent = s.transform.parent;
                    s.transform.SetParent(dummy);
                    var initRot = dummy.rotation;
                    dummy.Rotate(new Vector3(rotationInput.y, 0, 0), Space.Self);
                    //dummy.Rotate(mainCamera.transform.right, rotationInput.y);
                    dummy.Rotate(Vector3.up, rotationInput.x);

                    s.transform.SetParent(oldParent);

                }
            }
        }


        // at end, set prev touch position for next frame
        prevTouchPos = touchPosition;
        debug_lastTouchPos = touchPosition;
    }

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

    private void OnEnable()
    {
        clickArea = new Rect(clickAreaInScreens.x * Screen.width, clickAreaInScreens.y * Screen.height, clickAreaInScreens.width * Screen.width, clickAreaInScreens.height * Screen.height);

    }

    private bool GetRaycast(Ray ray, out RaycastHit rh)
    {
        if (raycastWidth == 0)
        {
            if (Physics.Raycast(ray, out rh))
            {
                return true;
            }
            return false;
        }
        else
        {
            // THIS SHOULD BE CONECAST
            if (Physics.SphereCast(ray, raycastWidth, out rh))
            {
                return true;
            }
            return false;
        }
    }

    private void HandleClickUp(Vector2 touchPos, Ray ray, RaycastHit rh, ToothpickPlaceable tp)
    {
        Deselect();

        if (OnClickObjectUp != null)
        {
            OnClickObjectUp(touchPos, ray, rh, tp);
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

    private void HandleSnackbar()
    {
        if (SearchingForPlaneUI != null)
        {
            // Hide snackbar when currently tracking at least one plane.
            Session.GetTrackables<DetectedPlane>(m_AllPlanes);
            bool showSearchingUI = true;
            for (int i = 0; i < m_AllPlanes.Count; i++)
            {
                if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
                {
                    showSearchingUI = false;
                    break;
                }
            }

            SearchingForPlaneUI.SetActive(showSearchingUI);
        }
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
            // unparent
            foreach (var s in selection)
            {
                s.transform.SetParent(null);
            }
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

        // parent selection list - could have been just this. or just the actual object...
        foreach (var s in selection)
        {
            s.transform.SetParent(mainCamera.transform);

        }

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

    /// <summary>
    /// Check and update the application lifecycle.
    /// </summary>
    private void _UpdateApplicationLifecycle()
    {
        // Exit the app when the 'back' button is pressed.
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Only allow the screen to sleep when not tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (m_IsQuitting)
        {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    /// <summary>
    /// Actually quit the application.
    /// </summary>
    private void _DoQuit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, 0);
                toastObject.Call("show");
            }));
        }
    }

    Vector2 debug_lastTouchPos;
    //private void OnGUI()
    //{
    //    //GUI.Label(new Rect(10, 10, 300, 100), debug_lastTouchPos.ToString());
    //}
}
