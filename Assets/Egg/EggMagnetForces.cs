using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EggMagnetForces : MonoBehaviour
{
    [SerializeField]
    private HarInputManageAR _inputMan;
    public HarInputManageAR inputMan
    {
        get
        {
            return EggController.instance.inputMan;
        }
    }

    [SerializeField]
    private Rigidbody _rb;
    public Rigidbody rb
    {
        get
        {
            if (_rb == null)
            {
                _rb = GetComponent<Rigidbody>();
            }
            return _rb;
        }
    }

    public Transform magnetTo;
    public float magnetForce = 10f;
    public AnimationCurve magnet = new AnimationCurve() { keys = new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 0, 0) } };
    public float magnetDist = 0.05f;

    bool touching;

    private void OnEnable()
    {
        inputMan.OnUpdateTouch += InputMan_OnUpdateTouch;
    }

    private void OnDisable()
    {
        inputMan.OnUpdateTouch -= InputMan_OnUpdateTouch;
    }

    private void InputMan_OnUpdateTouch(HarInputManageAR.TouchData data)
    {
        this.touching = (data.touching);

    }

    private void FixedUpdate()
    {
        if (touching)
        {
            var dir = (magnetTo.position - transform.position);
            var dist = dir.magnitude;

            // higher force when closer... but lower when further, and when super close.
            rb.AddForce(dir.normalized * magnet.Evaluate(dist / magnetDist) * magnetForce);

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * magnetDist);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + Vector3.right * magnetDist, transform.position + Vector3.right * magnetDist * magnet.keys.LastOrDefault().time);
    }
}