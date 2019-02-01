using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.XR;
using Random = UnityEngine.Random;

public class ToothpickPlacer : MonoBehaviour
{
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

    [SerializeField]
    private HarRaycaster _raycaster;
    public HarRaycaster raycaster
    {
        get
        {
            if (_raycaster == null)
            {
                _raycaster = GetComponent<HarRaycaster>();
            }
            return _raycaster;
        }
    }


    public GameObject toothpickPrefab;

    public HashSet<ToothpickPlaceable> selection = new HashSet<ToothpickPlaceable>();
    public HashSet<ToothpickPlaceable> highlighted = new HashSet<ToothpickPlaceable>();

    public List<GameObject> allSpawned = new List<GameObject>();
    public GameObject clearParticlesPrefab;

    private void OnEnable()
    {
        raycaster.OnRaycastOverObject += Raycaster_OnRaycastOverObject;
        raycaster.OnTapOnObject += Raycaster_OnTapOnObject;
        raycaster.OnRaycastOverNothing += Raycaster_OnRaycastOverNothing;
        raycaster.OnTapOnArPlanes += Raycaster_OnTapOnArPlanes;
        inputMan.OnUpdateTouch += InputMan_OnUpdateTouch;
    }

    private void OnDisable()
    {
        raycaster.OnRaycastOverObject -= Raycaster_OnRaycastOverObject;
        raycaster.OnTapOnObject -= Raycaster_OnTapOnObject;
        raycaster.OnRaycastOverNothing -= Raycaster_OnRaycastOverNothing;
        raycaster.OnTapOnArPlanes -= Raycaster_OnTapOnArPlanes;
        inputMan.OnUpdateTouch -= InputMan_OnUpdateTouch;
    }

    public event System.Action<ToothpickPlaceable> OnSelect;

    public void ClearSpawnedItems()
    {
        // particles on each
        for (int i = 0; i < allSpawned.Count; i++)
        {
            var s = allSpawned[i];
            Destroy(s.gameObject);
            if (clearParticlesPrefab != null)
            {
                var pp = Instantiate(clearParticlesPrefab, s.transform.position, Quaternion.identity);
                Destroy(pp, 7f);
            }
        }
        allSpawned.Clear();
    }

    private void InputMan_OnUpdateTouch(HarInputManageAR.TouchData data)
    {
        if (data.touchUp)
        {
            Select(null);
        }
    }

    private void Raycaster_OnTapOnArPlanes(HarInputManageAR.TouchData touchData, Ray ray, HarRaycaster.RaycastARData raycastARData)
    {
        var tp = SpawnToothpick(raycastARData);

        allSpawned.Add(tp.gameObject);

        Select(tp);
        Highlight(tp);

    }

    // spawns thing and makes anchor for it...
    private ToothpickPlaceable SpawnToothpick(HarRaycaster.RaycastARData raycastARData)
    {
        var hit = raycastARData.trackableHit;

        // Choose the Andy model for the Trackable that got hit.
        GameObject prefab;
        prefab = toothpickPrefab;

        // Instantiate Andy model at the hit pose.
        var andyObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

        // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
        // world evolves.
        var anchor = hit.Trackable.CreateAnchor(hit.Pose);

        // Make Andy model a child of the anchor.
        andyObject.transform.parent = anchor.transform;

        var tp = andyObject.GetComponent<ToothpickPlaceable>();
        return tp;
    }

    private void Raycaster_OnRaycastOverNothing()
    {
        Highlight(null);
    }

    private void Raycaster_OnTapOnObject(HarInputManageAR.TouchData touchData, Ray ray, RaycastHit rh, ToothpickPlaceable tp)
    {
        Select(tp);
    }

    private void Raycaster_OnRaycastOverObject(ToothpickPlaceable obj)
    {
        Highlight(obj);
    }

    private void Select(ToothpickPlaceable tp)
    {
        if (tp == null)
        {
            selection.Clear();
            return;
        }

        selection.Clear();
        selection.Add(tp);

        OnSelect?.Invoke(tp);
    }

    // optimize me???,,,
    private void Highlight(ToothpickPlaceable obj)
    {
        if (selection.Count > 0)
        {
            foreach (var s in selection)
            {
                s.Highlight(true);
                highlighted.Add(s);
            }
            return;
        }

        foreach (var h in highlighted)
        {
            if (h != obj)
            {
                h.Unhighlight();
            }
        }
        highlighted.Clear();

        if (obj != null)
        {
            obj.Highlight();
            highlighted.Add(obj);
        }
    }
    public void RefreshHighlightingExternal()
    {
        Highlight(null);
    }


}
