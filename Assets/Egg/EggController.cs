using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EggController : MonoBehaviour
{
    private static EggController _instance;
    public static EggController instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<EggController>();
            }
            return _instance;
        }
    }

    [SerializeField]
    private HarInputManageAR _inputMan;
    public HarInputManageAR inputMan
    {
        get
        {
            if (_inputMan == null)
            {
                _inputMan = GetComponent<HarInputManageAR>();
            }
            return _inputMan;
        }
    }

    [Header("Game state stuff")]
    public GameStates gameState;
    public enum GameStates
    {
        NoEgg, // initial
        EggInSpoon_Score, // egg in spoon, count score, game mechanics active
        DeadGameOver, // game over => show score, no counter, can make a new egg. perhaps see replay.... LOL
    }

    public GameObject eggPrefab;

    private GameObject spawnedEgg;
    public Vector3 localSpoonOffset;

    public Transform spoonEggTarget;

    public UnityEngine.UI.Text scoreText;
    public UnityEngine.UI.Text hiscoreText;
    public float distanceTraveled = 0f;
    public float highscoreLocal = 0f;
    private Vector3 oldMainCameraPos;

    [Space]
    public float distToSpoonForFail = 0.1f;

    [Header("Multiplier")]
    public AnimationCurve deltaPosToScore = new AnimationCurve() { keys = new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 0, 0) } };
    public float maxDeltaPos = 0.1f;
    public float curMultiplier = 1f;

    private void OnEnable()
    {
        inputMan.OnUpdateTouch += InputMan_OnUpdateTouch;

    }

    private void OnDisable()
    {
        inputMan.OnUpdateTouch -= InputMan_OnUpdateTouch;
    }

    public UnityEngine.UI.Text titleText;

    private void Update()
    {
        HandleText();

        if (spawnedEgg == null)
        {
            gameState = GameStates.NoEgg;
            return;
        }

        if (gameState == GameStates.EggInSpoon_Score)
        {
            var distToSpoon = (spawnedEgg.transform.position - spoonEggTarget.position).magnitude;
            if (distToSpoon > distToSpoonForFail)
            {
                gameState = GameStates.DeadGameOver;
            }

            var delta = inputMan.mainCamera.transform.position - oldMainCameraPos;
            oldMainCameraPos = inputMan.mainCamera.transform.position;
            delta.y = 0;
            var scoreFromDelta = deltaPosToScore.Evaluate(delta.magnitude / maxDeltaPos);

            distanceTraveled += scoreFromDelta * curMultiplier;
            ScoreText();

            DoHighscore();

        }


    }

    private void HandleText()
    {
        if (titleText != null)
        {
            switch (gameState)
            {
            case GameStates.NoEgg:
                titleText.text = "Tap to EGG";
                break;
            case GameStates.EggInSpoon_Score:
                titleText.text = "Walk the EGG!";
                break;
            case GameStates.DeadGameOver:
                titleText.text = "Oopsie";
                break;
            default:
                break;
            }
        }
    }

    private void ScoreText()
    {
        // count score!!
        scoreText.text = distanceTraveled.ToString("F0") + "m";
    }

    private void InputMan_OnUpdateTouch(HarInputManageAR.TouchData data)
    {
        if (data.touchDown)
        {
            if (gameState == GameStates.NoEgg)
            {
                SpawnEgg();
            }
            if (gameState == GameStates.DeadGameOver)
            {
                ResetGame();
            }
        }
    }

    [DebugButton]
    private void ResetGame()
    {
        gameState = GameStates.NoEgg;
        // or reload scene
        DoHighscore();
        distanceTraveled = 0;

    }

    private void DoHighscore()
    {
        if (distanceTraveled > highscoreLocal)
        {
            highscoreLocal = distanceTraveled;
            if (highscoreLocal > 0)
            {
                hiscoreText.text = highscoreLocal.ToString("F0") + "m";
                hiscoreText.gameObject.SetActive(true);
            }
        }
    }

    internal static void DeadEggRemote(EggDestroy eggDestroy)
    {
        if (eggDestroy.gameObject == instance.spawnedEgg)
        {
            instance.gameState = GameStates.DeadGameOver;
        }

        // show score...?? etc. and allow reset.

    }

    private void SpawnEgg()
    {
        spawnedEgg = Instantiate(eggPrefab, spoonEggTarget.position, spoonEggTarget.rotation);
        var magnet = spawnedEgg.GetComponent<EggMagnetForces>();
        magnet.magnetTo = spoonEggTarget;

        gameState = GameStates.EggInSpoon_Score;

    }
}