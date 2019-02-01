using System;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class HarInputManageAR : MonoBehaviour
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

    // cur update frame touch pos.... if used externally might be 1 frame old because of Update() ordering.
    // however EVENTS from this script will have the cur. frame touch pos.
    private Vector3 curTouchPos;
    // prev frame touch pos... might be 2 frames old because of Update() ordering...??
    private Vector3 prevTouchPos;
    private bool curTouching; // is input touching ?

    [Header("Click area. xy big is top right.")]
    public Rect clickAreaInScreens = new Rect(0, 0.1f, 1, 0.8f);
    private Rect clickArea;

    /// <summary>
    /// Called after updating/aggregating the touch data (and before the prev touch, so we get the full delta shit)
    /// </summary>
    public event UpdateTouchDelegate OnUpdateTouch;
    public delegate void UpdateTouchDelegate(TouchData data);

    public struct TouchData
    {
        public bool touchDown;
        public bool touchUp;
        public bool touching;
        public Vector3 touchPosition;
        public Vector3 prevTouchPosition;
    }

    private void OnEnable()
    {
        clickArea = new Rect(clickAreaInScreens.x * Screen.width, clickAreaInScreens.y * Screen.height, clickAreaInScreens.width * Screen.width, clickAreaInScreens.height * Screen.height);

    }

    private void Update()
    {
        // touchesss (merges 1 touch with input.mouseposition)
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
                curTouchPos = touchPosition; // button down
                prevTouchPos = touchPosition; // button down
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
                        curTouchPos = touchPosition; // touch down
                        prevTouchPos = touchPosition; // touch down
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

            if (touching)
            {
                curTouchPos = touchPosition; // per frame.
            }

        }

        // we could filter clicks based on screen area...... a little stupid but does the trick
        if ((touching || touchDown) && !clickArea.Contains(touchPosition))
        {
            return;
        }

        if (OnUpdateTouch != null)
        {
            OnUpdateTouch(new TouchData()
            {
                touchDown = touchDown,
                touchUp = touchUp,
                touching = touching,
                touchPosition = touchPosition,
                prevTouchPosition = prevTouchPos,
            });
        }

        // at end, set prev touch position for next frame
        prevTouchPos = touchPosition;
    }
}