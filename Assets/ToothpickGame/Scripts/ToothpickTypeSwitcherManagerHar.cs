using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ToothpickTypeSwitcherManagerHar : MonoBehaviour
{
    [SerializeField]
    private ToothpickPlacer _placer;
    public ToothpickPlacer placer
    {
        get
        {
            if (_placer == null)
            {
                _placer = GetComponent<ToothpickPlacer>();
            }
            return _placer;
        }
    }


    float clickDownTime = 0;
    public float delayForSwitch = 0.2f;
    ToothpickPlaceable lastClicked;
    Vector3 lastTapPos;

    public float maxDeltaPosForSwitch = 0.05f;

    private void OnEnable()
    {
        placer.raycaster.OnTapOnObject += Raycaster_OnTapOnObject;
        placer.inputMan.OnUpdateTouch += InputMan_OnUpdateTouch;
    }

    private void InputMan_OnUpdateTouch(HarInputManageAR.TouchData data)
    {
        if (data.touchUp)
        {
            if (lastClicked != null)
            {
                // Debug.Log(touchPos + " .. " + lastTapPos + " = " + (touchPos - lastTapPos).magnitude);
                if (Time.time - clickDownTime <= this.delayForSwitch)
                {
                    if ((data.touchPosition - lastTapPos).magnitude < maxDeltaPosForSwitch)
                    {
                        ChangeTypeOf(lastClicked);

                    }
                }
            }
        }
    }

    private void Raycaster_OnTapOnObject(HarInputManageAR.TouchData touchData, Ray ray, RaycastHit rh, ToothpickPlaceable tp)
    {
        clickDownTime = Time.time;
        lastClicked = tp;
        lastTapPos = touchData.touchPosition;
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