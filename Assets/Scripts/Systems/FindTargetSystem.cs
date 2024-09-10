using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

    public partial struct FindTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
            NativeList<DistanceHit> distanceHitsList = new NativeList<DistanceHit>(Allocator.Temp);

            foreach ((RefRO<LocalTransform> localTransform,
                         RefRW<FindTarget> findTarget,
                         RefRW<Target> target)
                     in SystemAPI.Query<
                         RefRO<LocalTransform>,
                         RefRW<FindTarget>, 
                         RefRW<Target>>())
            {
                findTarget.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (findTarget.ValueRO.timer > 0f)
                {
                    continue;
                }
                findTarget.ValueRW.timer = findTarget.ValueRO.timerMax;

                distanceHitsList.Clear();
                CollisionFilter collisionFilter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.UNITS_LAYER,
                    GroupIndex = 0,
                };
                if (collisionWorld.OverlapSphere(localTransform.ValueRO.Position, findTarget.ValueRO.range,
                        ref distanceHitsList, collisionFilter))
                {
                    foreach (var distanceHit in distanceHitsList)
                    {
                        if (!SystemAPI.Exists(distanceHit.Entity) || !SystemAPI.HasComponent<Unit>(distanceHit.Entity))
                        {
                            continue;
                        }
                        Unit targetUnit = SystemAPI.GetComponent<Unit>(distanceHit.Entity);
                        if (targetUnit.faction == findTarget.ValueRO.targetFaction)
                        {
                            target.ValueRW.targetEntity = distanceHit.Entity;
                            break; 
                        }
                        
                    }
                }
            }
        }
    }
