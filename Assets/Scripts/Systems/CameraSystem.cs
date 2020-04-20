using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;

[UpdateAfter(typeof(PlayerSystem))]
public class CameraSystem : JobComponentSystem {

    protected override JobHandle OnUpdate(JobHandle inputDeps) {

        float deltaTime = Time.DeltaTime;

        float3 playerPosition = EntityManager.GetComponentData<Translation>(GameDataManager.instance.playerEntityInstance).Value;
        quaternion playerRotation = EntityManager.GetComponentData<Rotation>(GameDataManager.instance.playerEntityInstance).Value;
        
        float3 cameraOffset = new float3(
            GameDataManager.instance.cameraHorizontalOffset,
            GameDataManager.instance.cameraVerticalOffset,
            GameDataManager.instance.cameraForwardOffset
        );
        
        bool changeCameraMode = Input.GetKeyDown(KeyCode.V);

        Entities.WithoutBurst().ForEach((Camera camera, ref Translation position, ref Rotation rotation, ref CameraData cameraData ) => {

            if (changeCameraMode) {
                cameraData.cameraMode = (cameraData.cameraMode + 1) % 2; // Currently only 2 modes are implemented
            }

            if (cameraData.cameraMode == 0) {
                float3 cameraPosition = playerPosition
                    + math.forward(math.mul(playerRotation, quaternion.Euler(math.radians(90), 0, 0))) * -2f
                    + math.forward(playerRotation) * -1f;
                position.Value = math.lerp(position.Value, cameraPosition, deltaTime * 50);
                rotation.Value = Quaternion.Slerp(playerRotation, rotation.Value, deltaTime * 0.25f);
            } else { // cameraMode == 1
                float3 cameraPosition = playerPosition 
                    + math.forward(math.mul(playerRotation, quaternion.Euler(0, math.radians(90), 0))) * cameraOffset.x
                    + math.forward(math.mul(playerRotation, quaternion.Euler(math.radians(90), 0, 0))) * cameraOffset.y
                    + math.forward(playerRotation) * cameraOffset.z;
                position.Value =  math.lerp(position.Value, cameraPosition, deltaTime * 8);
                rotation.Value = Quaternion.Slerp(playerRotation, rotation.Value, deltaTime * 0.05f);
            }
        }).Run();

        return inputDeps;
    }

}
