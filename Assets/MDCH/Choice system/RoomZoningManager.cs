using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomZoningManager : MonoBehaviour
{
    [Header("References")]
    public GameObject cornerMarkerPrefab;
    public Transform playerHead; // OVRCameraRig -> CenterEyeAnchor

    [Header("Debug")]
    public bool drawGizmos = true;

    // Internal data
    private List<Vector3> roomCorners = new List<Vector3>();
    private Bounds zoneA;
    private Bounds zoneB;

    public enum ZoneResult { None, ZoneA, ZoneB }
    public ZoneResult CurrentZone { get; private set; } = ZoneResult.None;

    public Action<ZoneResult> OnZoneLocked;

    void Start()
    {
        DetectRoom();
        CreateZones();
        SpawnMarkers();
    }

    void Update()
    {
        UpdatePlayerZone();
    }

    // -------------------------------
    // ROOM DETECTION
    // -------------------------------

    void DetectRoom()
    {
        var boundary = new OVRBoundary();
        Vector3[] points = boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);

        if (points == null || points.Length == 0)
        {
            Debug.LogError("No play area detected!");
            return;
        }

        roomCorners.Clear();

        foreach (var p in points)
        {
            Vector3 worldPoint = transform.TransformPoint(p);
            roomCorners.Add(worldPoint);
        }

        Debug.Log($"Detected {roomCorners.Count} room corners");
    }

    // -------------------------------
    // ZONE CREATION
    // -------------------------------

    void CreateZones()
    {
        Bounds roomBounds = new Bounds(roomCorners[0], Vector3.zero);

        foreach (var corner in roomCorners)
            roomBounds.Encapsulate(corner);

        bool splitOnX = roomBounds.size.x > roomBounds.size.z;

        if (splitOnX)
        {
            float midX = roomBounds.center.x;

            zoneA = new Bounds(
                new Vector3(
                    (roomBounds.min.x + midX) / 2f,
                    roomBounds.center.y,
                    roomBounds.center.z),
                new Vector3(
                    roomBounds.size.x / 2f,
                    roomBounds.size.y,
                    roomBounds.size.z));

            zoneB = new Bounds(
                new Vector3(
                    (midX + roomBounds.max.x) / 2f,
                    roomBounds.center.y,
                    roomBounds.center.z),
                zoneA.size);
        }
        else
        {
            float midZ = roomBounds.center.z;

            zoneA = new Bounds(
                new Vector3(
                    roomBounds.center.x,
                    roomBounds.center.y,
                    (roomBounds.min.z + midZ) / 2f),
                new Vector3(
                    roomBounds.size.x,
                    roomBounds.size.y,
                    roomBounds.size.z / 2f));

            zoneB = new Bounds(
                new Vector3(
                    roomBounds.center.x,
                    roomBounds.center.y,
                    (midZ + roomBounds.max.z) / 2f),
                zoneA.size);
        }
    }

    // -------------------------------
    // VISUAL MARKERS
    // -------------------------------

    void SpawnMarkers()
    {
        if (!cornerMarkerPrefab) return;

        foreach (var corner in roomCorners)
            Instantiate(cornerMarkerPrefab, corner, Quaternion.identity);
    }

    // -------------------------------
    // PLAYER TRACKING
    // -------------------------------

    void UpdatePlayerZone()
    {
        if (!playerHead) return;

        Vector3 pos = playerHead.position;

        if (zoneA.Contains(pos))
            CurrentZone = ZoneResult.ZoneA;
        else if (zoneB.Contains(pos))
            CurrentZone = ZoneResult.ZoneB;
        else
            CurrentZone = ZoneResult.None;
    }

    // -------------------------------
    // FINALIZE CHOICE
    // -------------------------------

    public void LockChoice()
    {
        Debug.Log($"Zone locked: {CurrentZone}");
        OnZoneLocked?.Invoke(CurrentZone);
    }

    // -------------------------------
    // DEBUG VISUALIZATION
    // -------------------------------

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(zoneA.center, zoneA.size);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(zoneB.center, zoneB.size);
    }
}
