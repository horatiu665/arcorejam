using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ToothpickPlaceable : MonoBehaviour
{
    private static Dictionary<Collider, ToothpickPlaceable> collidersToToothpicks = new Dictionary<Collider, ToothpickPlaceable>();
    public static ToothpickPlaceable Get(Collider c)
    {
        ToothpickPlaceable bb;
        collidersToToothpicks.TryGetValue(c, out bb);
        return bb;
    }

    [Header("This object can be placed by the ToothpickPlacer")]
    [SerializeField]
    private Collider _collider;
    public Collider collider
    {
        get
        {
            if (_collider == null)
            {
                _collider = GetComponentInChildren<Collider>(false);
            }
            return _collider;
        }
    }

    public Material highlightMat;

    private void OnEnable()
    {
        collidersToToothpicks.Add(collider, this);
    }

    private void OnDisable()
    {
        collidersToToothpicks.Remove(collider);
    }

    public void RefreshModel()
    {
        // reset collider to the new enabled model
        collidersToToothpicks.Remove(collider);
        _collider = null;
        collidersToToothpicks.Add(collider, this);

        // maybe do some other shit..?? particles?
    }

    // OPTIMIZE ME...??
    internal void Unhighlight()
    {
        //Debug.Log("Un ", gameObject);
        var r = collider.GetComponent<Renderer>();
        r.sharedMaterials = new Material[]
        {
            r.sharedMaterials[0]
        };
    }

    // OPTIMIZE ME...??
    internal void Highlight()
    {
        //Debug.Log("HILIGHT ", gameObject);

        var r = collider.GetComponent<Renderer>();
        r.sharedMaterials = new Material[]
        {
            r.sharedMaterials[0],
            highlightMat
        };
    }
}

