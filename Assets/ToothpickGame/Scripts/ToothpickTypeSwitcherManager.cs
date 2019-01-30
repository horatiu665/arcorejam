using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ToothpickTypeSwitcherManager : MonoBehaviour
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

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) ||
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector2 innn = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

            // first check if we clicked a current spawned stuff
            var ray = this.mainCamera.ScreenPointToRay(innn);
            RaycastHit rh;
            if (Physics.Raycast(ray, out rh))
            {
                var hitCollider = rh.collider;
                // if the hit obj is a toothpick...
                var tp = ToothpickPlaceable.Get(hitCollider);
                ChangeTypeOf(tp);

            }

        }

    }

    private static void ChangeTypeOf(ToothpickPlaceable tp)
    {
        if (tp != null)
        {
            // we hit a toothpick when we clicked.
            tp.GetComponent<ToothpickArtChooser>().ToggleNext();
        }
        else
        {

        }

    }
}