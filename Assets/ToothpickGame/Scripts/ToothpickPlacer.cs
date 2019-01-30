using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARExtensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.FaceSubsystem;
using Random = UnityEngine.Random;

public class ToothpickPlacer : MonoBehaviour
{
    public List<ToothpickPlaceable> selection = new List<ToothpickPlaceable>();

    [SerializeField]
    private bool placingMode = false;

    public void PlacingMode(bool active)
    {
        placingMode = active;

        if (!active)
        {
            // finish placing...

        }
    }

    void Update()
    {
        if (placingMode)
        {
            // move stuff... select...???
            // one button selects the raycast in middle of screen
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit rh;
            }
        }
    }

    public void Select(ToothpickPlaceable toothpick, bool additive = false)
    {
        if (additive)
        {
            selection.Add(toothpick);
        }
        else
        {
            selection.Clear();
            selection.Add(toothpick);
        }
    }

}