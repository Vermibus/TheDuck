using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class ProjectileCollisionEventSystem : JobComponentSystem {

    BuildPhysicsWorld buildPhysicsWorld;
    StepPhysicsWorld stepPhysicsWorld;

    protected override void OnCreate() {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld  = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {

        JobHandle jobHandle = new ProjectileCollisionEventImpulseJob {
            ProjectileGroup = GetComponentDataFromEntity<ProjectileData>(),
            CivilianShipGroup = GetComponentDataFromEntity<CivilianShipData>(),
            LifeTimeGroup = GetComponentDataFromEntity<LifeTimeData>(),
        }.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);

        jobHandle.Complete();

        return jobHandle;
    }

    struct ProjectileCollisionEventImpulseJob : ICollisionEventsJob {

        [ReadOnly] public ComponentDataFromEntity<ProjectileData> ProjectileGroup;
        public ComponentDataFromEntity<CivilianShipData> CivilianShipGroup;
        public ComponentDataFromEntity<LifeTimeData> LifeTimeGroup;

        public void Execute(CollisionEvent collisionEvent) {

            Entity entityA = collisionEvent.Entities.EntityA;
            Entity entityB = collisionEvent.Entities.EntityB;

            bool isShipA = CivilianShipGroup.Exists(entityA);
            bool isShipB = CivilianShipGroup.Exists(entityB);

            bool isProjectileA = ProjectileGroup.Exists(entityA);
            bool isProjectileB = ProjectileGroup.Exists(entityB);

            if ((isProjectileA && isShipB) || (isProjectileB && isShipA)) {

                var localEntityA = LifeTimeGroup[entityA];
                var localEntityB = LifeTimeGroup[entityB];

                localEntityA.alive = false;
                localEntityB.alive = false; 

                LifeTimeGroup[entityA] = localEntityA;
                LifeTimeGroup[entityB] = localEntityB;
            }

            // if (isProjectileA && isShipB) {
            //     var projectileEntity = LifeTimeGroup[entityA];
            //     var shipEntity = CivilianShipGroup[entityB];

            //     projectileEntity.alive = false;
            //     shipEntity.alive = false;

            //     LifeTimeGroup[entityA] = projectileEntity;
            //     CivilianShipGroup[entityB] = shipEntity;
            
            // } else if (isProjectileB && isShipA) {
            //     var projectileEntity = LifeTimeGroup[entityB];
            //     var shipEntity = CivilianShipGroup[entityA];

            //     projectileEntity.alive = false;
            //     shipEntity.alive = false;

            //     LifeTimeGroup[entityB] = projectileEntity;
            //     CivilianShipGroup[entityA] = shipEntity;
            // }
        }
    }
}