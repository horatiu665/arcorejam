using GoogleARCore;
using System.Collections.Generic;
using UnityEngine;

public class ARSurface : MonoBehaviour
{
    TrackedPlane m_trackedPlane;
    MeshCollider m_meshCollider;
    MeshFilter m_meshFilter;
    MeshRenderer m_meshRenderer;
    List<Vector3> m_points = new List<Vector3>();
    List<Vector3> m_previousFramePoints = new List<Vector3>();
    Mesh m_mesh;

    bool isVertical;

    public static bool showPlanes = true;
    bool prevShowPlanes;

    void Awake()
    {
        m_meshCollider = gameObject.AddComponent<MeshCollider>();
        m_meshFilter = gameObject.AddComponent<MeshFilter>();
        m_meshRenderer = gameObject.AddComponent<MeshRenderer>();

        m_mesh = new Mesh();
        m_meshFilter.mesh = m_mesh;
        m_meshCollider.sharedMesh = m_mesh;

        // Move the ARSurface up slightly to avoid flickering for those
        // who render additional geometry beneath the ARSurface.
        Vector3 oneCentimeterUp = Vector3.up * 0.01f;
        transform.Translate(oneCentimeterUp, Space.Self);
    }

    private void OnEnable()
    {
        Update_ShowPlanes(true);
    }

    public void SetTrackedPlane(TrackedPlane plane, Material material)
    {
        isVertical = plane.PlaneType == DetectedPlaneType.Vertical;

        m_trackedPlane = plane;
        if (ARSurfaceManager.instance.applyMaterial)
        {
            m_meshRenderer.material = material;
        }
        Update();
    }

    void Update()
    {
        if (m_trackedPlane == null)
        {
            return;
        }
        else if (m_trackedPlane.SubsumedBy != null)
        {
            Destroy(gameObject);
            return;
        }
        else if (Session.Status != SessionStatus.Tracking)
        {
            m_meshRenderer.enabled = false;
            m_meshCollider.enabled = false;
            return;
        }

        m_meshRenderer.enabled = true;
        m_meshCollider.enabled = true;

        UpdateMeshIfNeeded();

        Update_ShowPlanes();

    }

    private void Update_ShowPlanes(bool forced = false)
    {
        if ((showPlanes != prevShowPlanes) || forced)
        {
            prevShowPlanes = showPlanes;
            // how to fix color?????? of mat?????

            var showMat = MaterialRefManager.instance.planePlacement;
            var shadowMat = MaterialRefManager.instance.planeShadows;

            var planeAndShadow = new Material[]
                {
                    showMat,
                    shadowMat,
                };
            var shadowOnly = new Material[]
                {
                    shadowMat,
                };
            var planeOnly = new Material[]
                {
                    showMat,
                };
            var none = new Material[0];

            m_meshRenderer.sharedMaterials = showPlanes ?
                (isVertical ? planeOnly : planeAndShadow)
                : (isVertical ? none : shadowOnly);

        }
    }

    void UpdateMeshIfNeeded()
    {
        m_trackedPlane.GetBoundaryPolygon(m_points);

        if (AreVertexListsEqual(m_previousFramePoints, m_points))
        {
            return;
        }

        int[] indices = TriangulatorXZ.Triangulate(m_points);

        m_mesh.Clear();
        m_mesh.SetVertices(m_points);
        m_mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        m_mesh.RecalculateBounds();

        m_meshCollider.sharedMesh = null;
        m_meshCollider.sharedMesh = m_mesh;
    }

    bool AreVertexListsEqual(List<Vector3> firstList, List<Vector3> secondList)
    {
        if (firstList.Count != secondList.Count)
        {
            return false;
        }

        for (int i = 0; i < firstList.Count; i++)
        {
            if (firstList[i] != secondList[i])
            {
                return false;
            }
        }

        return true;
    }
}