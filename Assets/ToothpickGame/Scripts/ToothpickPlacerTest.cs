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

    List<GameObject> selection = new List<GameObject>();

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

    private List<ToothpickPlaceable> highlighted = new List<ToothpickPlaceable>();

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

        // raycast uin middle, find target obj
        var ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit rh;
        if (Physics.Raycast(ray, out rh))
        {
            // the obj
            var hitCollider = rh.collider;
            var tp = ToothpickPlaceable.Get(hitCollider);

            // highlight shit
            HandleHighlighting(ray, rh, tp);

            // if input, do stuff
            // touch shit
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    HandleClickDown(ray, rh, tp);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    HandleClickUp(ray, rh, tp);
                }
            }
            else // mouse shit
            {
                if (Input.GetMouseButtonDown(0))
                {
                    HandleClickDown(ray, rh, tp);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    HandleClickUp(ray, rh, tp);
                }
            }
        }
    }

    private void HandleClickUp(Ray ray, RaycastHit rh, ToothpickPlaceable tp)
    {
        Deselect();
    }

    private void HandleClickDown(Ray ray, RaycastHit rh, ToothpickPlaceable tp)
    {
        Select(tp.gameObject);
    }

    private void HandleHighlighting(Ray ray, RaycastHit rh, ToothpickPlaceable tp)
    {
        foreach (var h in highlighted)
        {
            h.Unhighlight();
        }

        if (tp != null)
        {
            tp.Highlight();
        }

    }

    private void HandleSnackbar()
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
        for (int i = 0; i < selection.Count; i++)
        {
            selection[i].transform.SetParent(mainCamera.transform);
        }

    }

    private void DoSpawnStuff(Vector2 touchPos)
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

            }
        }
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
}
