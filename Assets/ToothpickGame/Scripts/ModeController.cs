using GoogleARCore.Examples.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ModeController : MonoBehaviour
{
    [SerializeField]
    private ToothpickPlacerTest _placer;
    public ToothpickPlacerTest placer
    {
        get
        {
            if (_placer == null)
            {
                _placer = GetComponent<ToothpickPlacerTest>();
            }
            return _placer;
        }
    }

    [SerializeField]
    private ToothpickTypeSwitcherManager _changePickModeScript;
    public ToothpickTypeSwitcherManager changePickModeScript
    {
        get
        {
            if (_changePickModeScript == null)
            {
                _changePickModeScript = GetComponent<ToothpickTypeSwitcherManager>();
            }
            return _changePickModeScript;
        }
    }



    [Header("Game Mode")]
    public GameMode gameMode;
    public enum GameMode
    {
        PlacerMode, // placer, changer, rotator.
        Guess, // guesser => people can say "give up" and find the answer......
    }

    public Button gameModeButton;

    private void OnEnable()
    {
        gameModeButton.onClick.AddListener(Button_ToggleMode);
        Button_ToggleMode();
        Button_ToggleMode();
        Button_ToggleMode();
    }

    private void OnDisable()
    {
        gameModeButton.onClick.RemoveListener(Button_ToggleMode);
    }

    private void Button_ToggleMode()
    {
        SetMode((GameMode)(((int)(gameMode) + 1) % 2));
        gameModeButton.GetComponentInChildren<Text>().text = gameMode.ToString();
    }

    public void SetMode(GameMode mode)
    {
        gameMode = mode;
        placer.enabled = mode == GameMode.PlacerMode;
        changePickModeScript.enabled = mode == GameMode.PlacerMode;

        // do not show planes in guess mode.
        DetectedPlaneVisualizer.showPlanes = mode != GameMode.Guess;
    }

}