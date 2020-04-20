using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;

[UpdateAfter(typeof(MoveProjectileSystem))]
public class CivilianShipsSystem : JobComponentSystem {

    protected override JobHandle OnUpdate(JobHandle inputDeps) {

        float deltaTime = Time.DeltaTime;
        NativeArray<float3> waypoints = new NativeArray<float3>(GameDataManager.instance.waypoints, Allocator.TempJob);
        
        var jobHandle = Entities.WithName("CivilianShipsSystem").ForEach((ref Translation position, ref Rotation rotation, ref CivilianShipData civilianShipData) => {

            float3 destination = waypoints[civilianShipData.currentWaypoint];
            float distance = math.distance(position.Value, destination);
            float3 headingVector = destination - position.Value;

            if (distance > civilianShipData.followDistance) {
                quaternion targetDirection = quaternion.LookRotation(headingVector, math.up());
                rotation.Value = math.slerp(rotation.Value, targetDirection, deltaTime * 5.0f);
                position.Value += deltaTime * civilianShipData.speed * math.forward(rotation.Value);
            } else {
                civilianShipData.currentWaypoint = (civilianShipData.currentWaypoint + 1) % waypoints.Length;
            }

        }).Schedule(inputDeps);

        jobHandle.Complete();
        waypoints.Dispose();

        Entities.WithoutBurst().WithStructuralChanges().ForEach((Entity entity, ref Translation position, ref Rotation rotation, ref CivilianShipData civilianShipData, ref LifeTimeData lifeTimeData ) => {
            if (!lifeTimeData.alive) {
                var explosionEntity = EntityManager.Instantiate(civilianShipData.explosionPrefabEntity);
                EntityManager.SetComponentData(explosionEntity, new Translation { Value = position.Value });
                EntityManager.SetComponentData(explosionEntity, new Rotation { Value = rotation.Value });
                EntityManager.AddComponentData(explosionEntity, new LifeTimeData {
                    alive = true,
                    lifeLeft = 2.0f,
                    infiniteLifetime = false,
                });
            }
        }).Run();

        return jobHandle;
    }

}
