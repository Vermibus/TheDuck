using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;


[UpdateAfter(typeof(MoveProjectileSystem)), UpdateAfter(typeof(CivilianShipsSystem))]
public class LifeTimeSystem : JobComponentSystem {

    protected override JobHandle OnUpdate(JobHandle inputDeps) {

        float deltaTime = Time.DeltaTime;

        var jobHandle = Entities.WithName("LifeTimeSystem").ForEach((ref LifeTimeData lifeTimeData) => {

            if (!lifeTimeData.infiniteLifetime) {
                if (lifeTimeData.lifeLeft > 0) {
                    lifeTimeData.lifeLeft -= deltaTime;
                } else {
                    lifeTimeData.alive = false;
                }
            }

        }).Schedule(inputDeps);
        jobHandle.Complete();

        Entities.WithoutBurst().WithStructuralChanges().ForEach((Entity entity, ref LifeTimeData lifeTimeData) => {
            if (!lifeTimeData.alive) {
                EntityManager.DestroyEntity(entity);
            }
        }).Run();

        return jobHandle;
    }
}