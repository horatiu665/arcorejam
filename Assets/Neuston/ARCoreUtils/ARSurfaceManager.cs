using GoogleARCore;
using System.Collections.Generic;
using UnityEngine;

public class ARSurfaceManager : MonoBehaviour
{
    private static ARSurfaceManager _instance;
    public static ARSurfaceManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ARSurfaceManager>();
            }
            return _instance;
        }
    }

    [SerializeField] Material m_surfaceMaterial;
	List<TrackedPlane> m_newPlanes = new List<TrackedPlane>();

    public bool applyMaterial = true;

	void Update()
	{
		if (Session.Status != SessionStatus.Tracking)
		{
			return;
		}

		Session.GetTrackables(m_newPlanes, TrackableQueryFilter.New);

		foreach (var plane in m_newPlanes)
		{
			var surfaceObj = new GameObject("ARSurface");
			var arSurface = surfaceObj.AddComponent<ARSurface>();
			arSurface.SetTrackedPlane(plane, m_surfaceMaterial);
		}
	}
}
