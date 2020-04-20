using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;


public class GameDataManager : MonoBehaviour {
    
    public static GameDataManager instance;

    // Hook to player entity instance 
    public Entity playerEntityInstance;
    // public offsets for playerGuns 
    public float3[] playerGunLocations;
    // public waypoints 
    public float3[] waypoints;
    public Transform[] waypointsGameobjects;

    // Camera offsets
    [Range(-20,20)]
    public float cameraHorizontalOffset;
    [Range(-20,20)]
    public float cameraVerticalOffset;
    [Range(-20,20)]
    public float cameraForwardOffset;

    // UI Hooks
    public Text velocityText;

    void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
        } else {
            instance = this;
        }

        waypoints = new float3[waypointsGameobjects.Length];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = waypointsGameobjects[i].position;
        }
    }
}