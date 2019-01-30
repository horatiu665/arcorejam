using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ToothpickPlaceable : MonoBehaviour
{
    private Dictionary<Collider, ToothpickPlaceable> collidersToToothpicks = new Dictionary<Collider, ToothpickPlaceable>();

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
}

