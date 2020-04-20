using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.VFX;
// using UnityEditor.Experimental.VFX;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics;

public class PlayerSystem : JobComponentSystem {

    private bool initialized = false;
    
    // player move & rotate axis 
    private float moveAxisHorizontal = 0;
    private float moveAxisVertical = 0;
    private float moveAxisForwardAngle = 0;
    private float moveAxisUp = 0;
    private float mouseAxisY = 0;
    private float mouseAxisX = 0;

    private NativeArray<float3> gunPositions;

    protected override void OnCreate() {
        base.OnCreate();
    }

    private void Initialize() {
        gunPositions = new NativeArray<float3>(GameDataManager.instance.playerGunLocations, Allocator.Persistent);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {

        // Initialize stuff once, for the god sake 
        if (!initialized) {
            if (GameDataManager.instance != null) {
                Initialize();
                initialized = true;
            } else {
                return inputDeps;
            }
        }

        float deltaTime = Time.DeltaTime;

        // forward & backward axis
        moveAxisVertical = Input.GetAxis("Vertical");
        // side axis
        moveAxisHorizontal = Input.GetAxis("Horizontal");
        // rotation
        mouseAxisY = Input.GetAxis("Mouse Y");
        mouseAxisX = Input.GetAxis("Mouse X");

        // inclination around forward axis
        moveAxisForwardAngle = 0;
        if (Input.GetKey(KeyCode.Q)) {
            moveAxisForwardAngle = -1;
        } else if (Input.GetKey(KeyCode.E)) {
            moveAxisForwardAngle = 1;
        }

        // pitch axis
        moveAxisUp = 0;
        if (Input.GetKey(KeyCode.LeftControl)) {
            moveAxisUp = -1;
        } else if (Input.GetKey(KeyCode.LeftShift)) {
            moveAxisUp = 1;
        }
        
        // Fire keys  
        bool fire = (Input.GetKey(KeyCode.Space) || (Input.GetMouseButton(0)));

        // Dampener Key
        bool dampenerKey = Input.GetKeyDown(KeyCode.T);

        Entities.WithoutBurst().WithStructuralChanges()
        .ForEach((ref Translation position, ref Rotation rotation, ref PhysicsVelocity velocity, ref PhysicsMass mass, ref PlayerData playerData) => {
        
            // VELOCITY // THRUST
            float3 forwardImpulse = new float3(0, 0, 0);
            float3 backwardImpulse = new float3(0, 0, 0);
            float3 leftImpulse = new float3(0, 0, 0);
            float3 rightImpulse = new float3(0, 0, 0);
            float3 upImpulse = new float3(0, 0, 0);
            float3 downImpulse = new float3(0, 0, 0);

            // Front & Back
            if (moveAxisVertical > 0) {
                forwardImpulse = math.forward(rotation.Value) * moveAxisVertical * playerData.forwardThrust;
            } else if (moveAxisVertical < 0) {
                backwardImpulse = math.forward(rotation.Value) * moveAxisVertical * playerData.backwardThrust;
            }

            // Left & Right
            if (moveAxisHorizontal > 0) {
                leftImpulse = Utils.Vector.Right(rotation.Value) * moveAxisHorizontal * playerData.sideThrust;
            } else if (moveAxisHorizontal < 0) {
                rightImpulse = Utils.Vector.Right(rotation.Value) * moveAxisHorizontal * playerData.sideThrust;
            }

            // Up & Down 
            if (moveAxisUp > 0) {
                upImpulse = Utils.Vector.Up(rotation.Value) * moveAxisUp * playerData.pitchThrust;
            } else if (moveAxisUp < 0) {
                downImpulse = Utils.Vector.Up(rotation.Value) * moveAxisUp * playerData.pitchThrust;
            }

            // DAMPENER
            if (dampenerKey) {
                playerData.dampener = !playerData.dampener;
            }
            UIManager.instance.SetDampenerInfo(playerData.dampener);

            if (playerData.dampener) {
                
                // Front & Back Dampener
                if (math.abs(velocity.Linear.z) < 0.05f) {
                    velocity.Linear.z = 0;
                } else {
                    // Front dampener thrust
                    if (Vector3.Angle(Utils.Vector.Forward(rotation.Value), math.normalize(velocity.Linear)) < 90) {
                        forwardImpulse = math.forward(rotation.Value) * playerData.forwardThrust * -1;
                    } 
                    // Back dampener thrust
                    if (Vector3.Angle(Utils.Vector.Backward(rotation.Value), math.normalize(velocity.Linear)) < 90) {
                        backwardImpulse = Utils.Vector.Backward(rotation.Value) * playerData.backwardThrust * -1;
                    }
                }

                // Left & Right Dampener
                if (math.abs(velocity.Linear.x) < 0.05f) {
                    velocity.Linear.x = 0;
                } else {
                    // Left dampener thrust
                    if (Vector3.Angle(Utils.Vector.Left(rotation.Value), math.normalize(velocity.Linear)) < 90) {
                        leftImpulse = Utils.Vector.Left(rotation.Value) * playerData.sideThrust * -1;
                    }
                    // Right dampener thrust
                    if (Vector3.Angle(Utils.Vector.Right(rotation.Value), math.normalize(velocity.Linear)) < 90) {
                        rightImpulse = Utils.Vector.Right(rotation.Value) * playerData.sideThrust * -1;
                    }
                }
                
                // Up & Down Dampener
                if (math.abs(velocity.Linear.y) < 0.05f) {
                    velocity.Linear.y = 0;
                } else {
                    // Up dampener thrust
                    if (Vector3.Angle(Utils.Vector.Up(rotation.Value), math.normalize(velocity.Linear)) < 90) {
                        upImpulse = Utils.Vector.Up(rotation.Value) * playerData.pitchThrust * -1;
                    }
                    // Down dampener thrust
                    if (Vector3.Angle(Utils.Vector.Down(rotation.Value), math.normalize(velocity.Linear)) < 90) {
                        downImpulse = Utils.Vector.Down(rotation.Value) * playerData.pitchThrust * -1;
                    }
                }
            }

            float3 impulse = (forwardImpulse + backwardImpulse + leftImpulse + rightImpulse + upImpulse + downImpulse) * deltaTime;
            Unity.Physics.Extensions.ComponentExtensions.ApplyLinearImpulse(ref velocity, mass, impulse);

            // ROTATION
            rotation.Value = math.mul(rotation.Value, quaternion.Euler(new float3( 
                math.radians(mouseAxisY), 
                math.radians(mouseAxisX),
                -moveAxisForwardAngle * deltaTime
            )));

            // SHOOTING SYSTEM
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

            // Various UI Updates
            UIManager.instance.SetPlayerVelocity(velocity.Linear);

        }).Run();

        return inputDeps;
    }

    protected override void OnDestroy() {
        gunPositions.Dispose();
        base.OnDestroy();
    }
}
