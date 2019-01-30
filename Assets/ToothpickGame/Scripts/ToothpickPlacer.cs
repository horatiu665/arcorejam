using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.XR;
using Random = UnityEngine.Random;

public class ToothpickPlacer : MonoBehaviour
{
    public List<ToothpickPlaceable> selection = new List<ToothpickPlaceable>();

    [SerializeField]
    private Camera _mainCamera;
    public Camera mainCamera
    {
        get
        {
            if (_mainCamera == null)
            {
                _mainCamera = GetComponent<Camera>();
            }
            return _mainCamera;
        }
    }
    
    private List<ToothpickPlaceable> spawnedSticks = new List<ToothpickPlaceable>();
    public ToothpickPlaceable toothpickPrefab;

    [SerializeField]
    private bool placingMode = false;

    public void SetPlacingMode(bool active)
    {
        placingMode = active;

        if (!active)
        {
            // finish placing... and change mode to guess mode ;)

        }
    }

    void Update()
    {
        if (placingMode)
        {
            Update_TouchDown();
            Update_TouchUp();

        }
    }

    private void Update_TouchDown()
    {
        // move stuff... select...???
        // one button selects the raycast in middle of screen
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit rh;
            var mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);
            bool foundSelection = false;
            if (Physics.Raycast(mouseRay, out rh))
            {
                var hitObject = rh.collider;
                // if the hit obj is a toothpick...
                var tp = hitObject.GetComponent<ToothpickPlaceable>();
                if (tp != null)
                {
                    // we hit a toothpick when we clicked. Select it.
                    Select(tp);
                    foundSelection = true;
                }
            }
            
        }
    }

    private void Update_TouchUp()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Select(null);

        }
    }

    // Deselect = Select(null)
    public void Deselect()
    {
        Select(null);
    }

    public void Select(ToothpickPlaceable toothpick, bool additive = false)
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

}
