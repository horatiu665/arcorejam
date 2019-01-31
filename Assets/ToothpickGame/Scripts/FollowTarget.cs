using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class FollowTarget : MonoBehaviour
{
    public Transform target;

    public float posSmoothing = 0.1f;
    public float rotSmoothing = 0.1f;

    public bool update = true, fixedUpdate, lateUpdate;

    void Update()
    {
        if (update)
            UpdatePosRot();
    }

    private void UpdatePosRot()
    {

        transform.position = Vector3.Lerp(transform.position, target.position, posSmoothing);
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, rotSmoothing);
    }

    private void FixedUpdate()
    {
        if (fixedUpdate)
            UpdatePosRot();
    }

    private void LateUpdate()
    {
        if (lateUpdate)
            UpdatePosRot();
    }

}