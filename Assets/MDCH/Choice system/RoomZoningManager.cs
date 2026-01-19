using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomZoningManager : MonoBehaviour
{
    [Header("OVR")]
    public Transform playerHead; // OVRCameraRig → CenterEyeAnchor

    [Header("Markers")]
    public GameObject zoneMarkerPrefab;
    public float markerHeightOffset = 0.02f;

    public Color zoneAColor = Color.blue;
    public Color zoneBColor = Color.red;

    [Header("Debug")]
    public bool drawGizmos = true;
    public ZoneResult debugCurrentZone;

    public enum ZoneResult { None, ZoneA, ZoneB }
    public ZoneResult CurrentZone { get; private set; }

    private Vector3 headsetStartWorld;
    private bool initialized = false;

    private List<Vector3> roomCornersLocal = new List<Vector3>();
    private Bounds zoneALocal;
    private Bounds zoneBLocal;

    // -------------------------------
    // UNITY
    // -------------------------------

    void Start()
    {
        StartCoroutine(Initialize());
    }

    void Update()
    {
        if (!initialized) return;
        UpdatePlayerZone();
    }

    // -------------------------------
    // INITIALIZATION
    // -------------------------------

    IEnumerator Initialize()
    {
        while (!OVRManager.boundary.GetConfigured())
            yield return null;

        headsetStartWorld = playerHead.position;

        DetectRoomLocal();
        CreateZonesLocal();

        SpawnZoneVisuals(zoneALocal, zoneAColor, "Zone A");
        SpawnZoneVisuals(zoneBLocal, zoneBColor, "Zone B");

        initialized = true;
    }

    // -------------------------------
    // ROOM DETECTION
    // -------------------------------

    void DetectRoomLocal()
    {
        roomCornersLocal.Clear();

        Vector3[] points =
            OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);

        foreach (var p in points)
            roomCornersLocal.Add(p);
    }

    // -------------------------------
    // ZONE CREATION
    // -------------------------------

    void CreateZonesLocal()
    {
        Bounds roomBounds = new Bounds(roomCornersLocal[0], Vector3.zero);
        foreach (var p in roomCornersLocal)
            roomBounds.Encapsulate(p);

        bool splitOnX = roomBounds.size.x >= roomBounds.size.z;

        if (splitOnX)
        {
            float midX = roomBounds.center.x;
            zoneALocal = CreateSubBounds(roomBounds, true, roomBounds.min.x, midX);
            zoneBLocal = CreateSubBounds(roomBounds, true, midX, roomBounds.max.x);
        }
        else
        {
            float midZ = roomBounds.center.z;
            zoneALocal = CreateSubBounds(roomBounds, false, roomBounds.min.z, midZ);
            zoneBLocal = CreateSubBounds(roomBounds, false, midZ, roomBounds.max.z);
        }
    }

    Bounds CreateSubBounds(Bounds original, bool splitX, float min, float max)
    {
        Vector3 center = original.center;
        Vector3 size = original.size;

        if (splitX)
        {
            center.x = (min + max) * 0.5f;
            size.x = Mathf.Abs(max - min);
        }
        else
        {
            center.z = (min + max) * 0.5f;
            size.z = Mathf.Abs(max - min);
        }

        return new Bounds(center, size);
    }

    // -------------------------------
    // VISUALS
    // -------------------------------

    void SpawnZoneVisuals(Bounds zoneLocal, Color color, string label)
    {
        // Center marker
        SpawnMarker(zoneLocal.center, color, $"{label} Center");

        // Corner markers
        Vector3 min = zoneLocal.min;
        Vector3 max = zoneLocal.max;

        SpawnMarker(new Vector3(min.x, 0, min.z), color, $"{label} Corner");
        SpawnMarker(new Vector3(min.x, 0, max.z), color, $"{label} Corner");
        SpawnMarker(new Vector3(max.x, 0, min.z), color, $"{label} Corner");
        SpawnMarker(new Vector3(max.x, 0, max.z), color, $"{label} Corner");
    }

    void SpawnMarker(Vector3 localPos, Color color, string name)
    {
        Vector3 worldPos =
            headsetStartWorld + localPos + Vector3.up * markerHeightOffset;

        GameObject marker = Instantiate(
            zoneMarkerPrefab,
            worldPos,
            Quaternion.identity);

        marker.name = name;

        var renderer = marker.GetComponentInChildren<Renderer>();
        if (renderer)
        {
            renderer.material = new Material(renderer.material);
            renderer.material.color = color;
        }
    }

    // -------------------------------
    // PLAYER TRACKING
    // -------------------------------

    void UpdatePlayerZone()
    {
        Vector3 playerLocal = playerHead.position - headsetStartWorld;

        if (zoneALocal.Contains(playerLocal))
            CurrentZone = ZoneResult.ZoneA;
        else if (zoneBLocal.Contains(playerLocal))
            CurrentZone = ZoneResult.ZoneB;
        else
            CurrentZone = ZoneResult.None;

        debugCurrentZone = CurrentZone;
    }

    // -------------------------------
    // GIZMOS
    // -------------------------------

    void OnDrawGizmos()
    {
        if (!drawGizmos || !initialized) return;

        Gizmos.color = zoneAColor;
        Gizmos.DrawWireCube(
            headsetStartWorld + zoneALocal.center,
            zoneALocal.size);

        Gizmos.color = zoneBColor;
        Gizmos.DrawWireCube(
            headsetStartWorld + zoneBLocal.center,
            zoneBLocal.size);
    }
}