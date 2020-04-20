using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


public class ECSManager : MonoBehaviour {

    public GameObject playerShipPrefab;
    [Range(0, 100)] public float forwardThrust;
    [Range(0, 100)] public float backwardThrust;
    [Range(0, 100)] public float sideThrust;
    [Range(0, 100)] public float pitchThrust;

    public GameObject bulletPrefab;
    public GameObject explosionPrefab;

    public GameObject spaceCraftPrefab_1;

    private EntityManager entityManager;
    private Entity playerEntityInstance;
    private BlobAssetStore blobAssetStore;
    private GameObjectConversionSettings settings;

    void Start() {

        Debug.Log("" + Utils.Vector.Forward(quaternion.identity));
        Debug.Log("" + Utils.Vector.Left(quaternion.identity));

        blobAssetStore = new BlobAssetStore();
        // Convert GameObjects to Entities 
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore);
        var shipPrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(playerShipPrefab, settings);
        var bulletPrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(bulletPrefab, settings);
        var explosionPrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(explosionPrefab, settings);

        // Spawn Player
        playerEntityInstance = entityManager.Instantiate(shipPrefabEntity);
        entityManager.SetComponentData(playerEntityInstance, new Translation { Value = new float3(-50, 0, 0)});
        entityManager.SetComponentData(playerEntityInstance, new Rotation { Value = quaternion.identity});
        entityManager.SetComponentData(playerEntityInstance, new PlayerData {
            forwardThrust = forwardThrust,
            backwardThrust = backwardThrust,
            sideThrust = sideThrust,
            pitchThrust = pitchThrust,
            dampener = false,
            reloadTime = 0.5f,
            nextFire = 0.0f,
            bulletPrefabEntity = bulletPrefabEntity,
        });
        // entityManager.AddComponentData(playerEntityInstance, new)
        GameDataManager.instance.playerEntityInstance = playerEntityInstance;
       
        // Set guns offsets 
        List<GameObject> bulletSpawnPoints = new List<GameObject>(); 
        foreach (Transform gun in playerShipPrefab.transform) {
            if (gun.tag == "BulletSpawnPoint") {
                bulletSpawnPoints.Add(gun.gameObject);
            }
        }
        GameDataManager.instance.playerGunLocations = new float3[bulletSpawnPoints.Count];
        for (int i = 0; i < bulletSpawnPoints.Count; i++) {
            GameDataManager.instance.playerGunLocations[i] = bulletSpawnPoints[i].transform.position;
        }

        // Spawn civilian ships
        for (int i = 0; i < 15; i++) {
            spawnCivilianShip(spaceCraftPrefab_1, new CivilianShipData { 
                speed = UnityEngine.Random.Range(15, 45),
                followDistance = 15.0f,
                explosionPrefabEntity = explosionPrefabEntity,
            });
        }

        // Lock mouse visibility on Play Mode
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private Entity spawnCivilianShip(GameObject shipPrefab, CivilianShipData civilianShipData) {
        var civilianShipPrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(shipPrefab, settings);

        float3 positon = new float3(
            UnityEngine.Random.Range(-100, 100),
            UnityEngine.Random.Range(-100, 100),
            UnityEngine.Random.Range(-100, 100)
        );

        Entity entity = entityManager.Instantiate(civilianShipPrefabEntity);
        entityManager.SetComponentData(entity, new Translation { Value = positon });
        entityManager.SetComponentData(entity, new Rotation { Value = quaternion.identity });

        int closestWaypoint = 0; 
        float distance = Mathf.Infinity;
        for (int j = 0; j < GameDataManager.instance.waypoints.Length; j++) {
            if (Vector3.Distance(GameDataManager.instance.waypoints[j], positon) < distance) {
                closestWaypoint = j;
                distance = Vector3.Distance(GameDataManager.instance.waypoints[j], positon);
            }
        }

        entityManager.AddComponentData(entity, new CivilianShipData {
            speed = civilianShipData.speed,
            followDistance = civilianShipData.followDistance,
            currentWaypoint = closestWaypoint,
            explosionPrefabEntity = civilianShipData.explosionPrefabEntity,
        });

        entityManager.AddComponentData(entity, new LifeTimeData {
            alive = true,
            infiniteLifetime = true,
        });

        return entity;
    }

    void OnDestroy() {
        blobAssetStore.Dispose();
    }
}
