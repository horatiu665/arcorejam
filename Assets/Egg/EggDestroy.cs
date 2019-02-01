using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EggDestroy : MonoBehaviour
{
    [SerializeField]
    private Rigidbody _r;
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

    public GameObject artForOkEgg, artForDeadEgg;

    public SmartSound eggDead;

    public float minEggY = 0.1f;

    public float destroyDelay = 7f;

    public float mniVelToDie = 1f;

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponentInParent<SpoonTag>() != null)
        {
            return;
        }


        if (collision.relativeVelocity.magnitude
            > mniVelToDie)
        {
            DeadEgg();
        }

    }

    private void Update()
    {
        if (EggController.instance.gameState == EggController.GameStates.EggInSpoon_Score)
        {
            if (transform.position.y <= minEggY)
            {
                DeadEgg();
            }
        }

    }

    public void DeadEgg()
    {
        // replace egg graphics
        r.isKinematic = true;

        r.rotation = Quaternion.identity;

        artForOkEgg.SetActive(false);
        artForDeadEgg.SetActive(true);

        if (eggDead != null)
        {
            eggDead.Play();
        }

        EggController.DeadEggRemote(this);


        Destroy(gameObject, destroyDelay);
    }
}