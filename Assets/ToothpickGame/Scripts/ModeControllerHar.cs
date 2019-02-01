using GoogleARCore.Examples.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ModeControllerHar : MonoBehaviour
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
    [SerializeField]
    private ToothpickTypeSwitcherManagerHar _switcher;
    public ToothpickTypeSwitcherManagerHar switcher
    {
        get
        {
            if (_switcher == null)
            {
                _switcher = GetComponent<ToothpickTypeSwitcherManagerHar>();
            }
            return _switcher;
        }
    }

    [Header("Game Mode")]
    public GameMode gameMode;
    public enum GameMode
    {
        PlacerMode, // placer, changer, rotator.
        Guess, // guesser => people can say "give up" and find the answer......
    }
    public const int GAME_MODES = 2;

    public Button gameModeButton;
    public InputField guessField;

    public string whatIsThis;

    public Button title;
    public Button instructions;
    public GameObject guessModeText;

    private void OnEnable()
    {
        gameModeButton.onClick.AddListener(Button_ToggleMode);

        // reset the game hehheheheh to the first state...
        for (int i = 0; i < GAME_MODES; i++)
        {
            Button_ToggleMode();
        }

        guessField.onValueChanged.AddListener(OnGuessFieldChange);
        title.onClick.AddListener(OnTitleClick);
        instructions.onClick.AddListener(OnInstructionsClick);
    }

    private void OnInstructionsClick()
    {
        instructions.gameObject.SetActive(false);

        guessField.gameObject.SetActive(true);
        gameModeButton.gameObject.SetActive(true);
        guessModeText.SetActive(true);
    }


    private void OnTitleClick()
    {
        instructions.gameObject.SetActive(true);

        guessField.gameObject.SetActive(false);
        gameModeButton.gameObject.SetActive(false);
        guessModeText.SetActive(false);
    }

    private void OnGuessFieldChange(string arg0)
    {
        whatIsThis = arg0;
    }

    private void OnDisable()
    {
        gameModeButton.onClick.RemoveListener(Button_ToggleMode);

        guessField.onValueChanged.RemoveListener(OnGuessFieldChange);
        title.onClick.RemoveListener(OnTitleClick);
        instructions.onClick.RemoveListener(OnInstructionsClick);
    }

    private void Button_ToggleMode()
    {
        SetMode((GameMode)(((int)(gameMode) + 1) % GAME_MODES));
    }

    public void SetMode(GameMode mode)
    {
        gameMode = mode;
        placer.enabled = mode == GameMode.PlacerMode;
        switcher.enabled = mode == GameMode.PlacerMode;

        // do not show planes in guess mode...?
        DetectedPlaneVisualizer.showPlanes = mode != GameMode.Guess;
        ARSurface.showPlanes = mode != GameMode.Guess;

        placer.RefreshHighlightingExternal();

        gameModeButton.GetComponentInChildren<Text>().text =
            mode == GameMode.Guess ?
            "GIVE UP" :
            "CREATE";

        if (mode == GameMode.Guess)
        {
            guessField.gameObject.SetActive(false);
            guessModeText.SetActive(true);
        }
        else
        {
            guessField.gameObject.SetActive(true);
            guessModeText.SetActive(false);
        }
    }

}