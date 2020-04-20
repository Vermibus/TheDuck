using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
// using UnityEditor.Experimental.VFX;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;

public class PlayerSystem : JobComponentSystem {
    

    protected override void OnCreate() {
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {

        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Break();
        }

        float deltaTime = Time.DeltaTime;

        float moveAxisHorizontal = Input.GetAxis("Horizontal");
        float moveAxisVertical = Input.GetAxis("Vertical");
        float mouseAxisY = Input.GetAxis("Mouse Y");
        float mouseAxisX = Input.GetAxis("Mouse X");

        float moveAxisForwardAngle = 0;
        if (Input.GetKey(KeyCode.Q)) {
            moveAxisForwardAngle = -1;
        } else if (Input.GetKey(KeyCode.E)) {
            moveAxisForwardAngle = 1;
        }

        float moveAxisUp = 0;
        if (Input.GetKey(KeyCode.LeftControl)) {
            moveAxisUp = -1;
        } else if (Input.GetKey(KeyCode.LeftShift)) {
            moveAxisUp = 1;
        }

        bool fire = (Input.GetKey(KeyCode.Space) || (Input.GetMouseButton(0)));

        NativeArray<float3> gunPositions = new NativeArray<float3>(GameDataManager.instance.playerGunLocations, Allocator.TempJob);
        
        Entities.WithoutBurst().WithStructuralChanges().ForEach(( ref Translation position, ref Rotation rotation, ref PlayerData playerData, ref InputData inputData) => {

            float3 forwardMove = math.forward(rotation.Value) * moveAxisVertical; 
            float3 sideMove = math.forward(math.mul(rotation.Value, quaternion.Euler(0, math.radians(90), 0))) * moveAxisHorizontal;
            float3 upMove = math.forward(math.mul(rotation.Value, quaternion.Euler(math.radians(-90), 0, 0))) * moveAxisUp;

            position.Value += (forwardMove + sideMove + upMove) * playerData.speed * deltaTime;

            rotation.Value = math.mul(rotation.Value, quaternion.Euler(new float3( 
                math.radians(mouseAxisY), 
                math.radians(mouseAxisX),
                -moveAxisForwardAngle * deltaTime
            )));

            if (fire && playerData.nextFire <= 0.0f) {
                foreach (float3 gunPosition in gunPositions) {
                    var instance = EntityManager.Instantiate(playerData.bulletPrefabEntity);
                    EntityManager.SetComponentData(instance, new Translation { Value = position.Value + math.mul(rotation.Value, gunPosition)});
                    EntityManager.SetComponentData(instance, new Rotation { Value = rotation.Value });
                    EntityManager.SetComponentData(instance, new LifeTimeData { 
                        lifeLeft = 2f,
                        alive = true
                    });
                    EntityManager.SetComponentData(instance, new ProjectileData { 
                        speed = 200,
                    });
                }

                playerData.nextFire += playerData.reloadTime;
            } else {
                playerData.nextFire = math.clamp(playerData.nextFire - deltaTime, 0.0f, playerData.reloadTime);
            }

        }).Run();

        gunPositions.Dispose();

        return inputDeps;
    }
}
