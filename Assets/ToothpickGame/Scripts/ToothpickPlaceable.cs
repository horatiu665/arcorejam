using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ToothpickPlaceable : MonoBehaviour
{
    private static Dictionary<Collider, ToothpickPlaceable> collidersToToothpicks = new Dictionary<Collider, ToothpickPlaceable>();
    private static Dictionary<GameObject, ToothpickPlaceable> gameObjectsToToothpicks = new Dictionary<GameObject, ToothpickPlaceable>();

    public static ToothpickPlaceable Get(Collider c)
    {
        ToothpickPlaceable bb;
        collidersToToothpicks.TryGetValue(c, out bb);
        return bb;
    }

    public static ToothpickPlaceable Get(GameObject go)
    {
        ToothpickPlaceable bb;
        gameObjectsToToothpicks.TryGetValue(go, out bb);
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

    [SerializeField]
    private Rigidbody _rigidbody;
    public Rigidbody rigidbody
    {
        get
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }
            return _rigidbody;
        }
    }


    public Material highlightMat;
    public Material highlightMatSelected;

    private void OnEnable()
    {
        collidersToToothpicks.Add(collider, this);
        gameObjectsToToothpicks.Add(gameObject, this);

    }

    private void OnDisable()
    {
        collidersToToothpicks.Remove(collider);
        gameObjectsToToothpicks.Remove(gameObject);

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
    internal void Highlight(bool selected = false)
    {
        //Debug.Log("HILIGHT ", gameObject);

        var r = collider.GetComponent<Renderer>();
        r.sharedMaterials = new Material[]
        {
            r.sharedMaterials[0],
            selected ? highlightMatSelected : highlightMat,
        };
    }

}

