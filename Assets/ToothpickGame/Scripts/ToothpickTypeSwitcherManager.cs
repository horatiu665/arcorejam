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

    [SerializeField]
    private ToothpickPlacerTest _toothpickPlacer;
    public ToothpickPlacerTest toothpickPlacer
    {
        get
        {
            if (_toothpickPlacer == null)
            {
                _toothpickPlacer = GetComponent<ToothpickPlacerTest>();
            }
            return _toothpickPlacer;
        }
    }


    float clickDownTime = 0;
    public float delayForSwitch = 0.2f;
    ToothpickPlaceable lastClicked;
    Vector2 lastTapPos;

    public float maxDeltaPosForSwitch = 0.05f;

    private void OnEnable()
    {
        toothpickPlacer.OnClickObjectDown += ToothpickPlacer_OnClickObjectDown;
        toothpickPlacer.OnClickObjectUp += ToothpickPlacer_OnClickObjectUp;
    }

    private void OnDisable()
    {
        toothpickPlacer.OnClickObjectDown -= ToothpickPlacer_OnClickObjectDown;
        toothpickPlacer.OnClickObjectUp -= ToothpickPlacer_OnClickObjectUp;
    }

    private void ToothpickPlacer_OnClickObjectDown(Vector2 touchPos, Ray ray, RaycastHit rh, ToothpickPlaceable toothpick)
    {
        clickDownTime = Time.time;
        lastClicked = toothpick;
        lastTapPos = touchPos;
    }

    private void ToothpickPlacer_OnClickObjectUp(Vector2 touchPos, Ray ray, RaycastHit rh, ToothpickPlaceable toothpick)
    {
        if (lastClicked != null)
        {
                Debug.Log(touchPos + " .. " + lastTapPos + " = " + (touchPos - lastTapPos).magnitude);
            if (Time.time - clickDownTime <= this.delayForSwitch)
            {
                if ((touchPos - lastTapPos).magnitude < maxDeltaPosForSwitch)
                {
                    ChangeTypeOf(lastClicked);

                }
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