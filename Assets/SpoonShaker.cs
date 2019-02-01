using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpoonShaker : MonoBehaviour
{
    private float shakeTime = 0f;
    bool shake = false;

    public float shakeDuration = 30f;

    public AnimationCurve shakeCurve = new AnimationCurve() { keys = new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 0, 0) } };

    [SerializeField]
    private Rigidbody _r;

    public float shakeMultiplier = 1f;

    public Rigidbody r
    {
        get
        {
            if (_r == null)
            {
                _r = GetComponent<Rigidbody>();
            }
            return _r;
        }
    }


    public void StartShaking()
    {
        shakeTime = Time.time;
        shake = true;
    }


    void Update()
    {
        if (EggController.instance.gameState == EggController.GameStates.NoEgg)
        {
            shake = false;
        }

        if (EggController.instance.gameState == EggController.GameStates.EggInSpoon_Score)
        {
            if (!shake)
            {
                StartShaking();
                return;
            }
        }

        Update_HandleShake();

    }

    private void Update_HandleShake()
    {
        var shakeAmount = shakeCurve.Evaluate((Time.time - shakeTime) / shakeDuration) * shakeMultiplier;

        r.AddForce(Random.onUnitSphere * shakeAmount, ForceMode.Impulse);

    }
}