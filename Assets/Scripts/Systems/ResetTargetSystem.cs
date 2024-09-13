using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
public partial struct ResetTargetSystem : ISystem
{
    private ComponentLookup<LocalTransform> localTransformComponentLookup;
    public EntityStorageInfoLookup entityStorageInfoLookup;
    
    public void OnCreate(ref SystemState state)
    {
        localTransformComponentLookup = state.GetComponentLookup<LocalTransform>(true);
        entityStorageInfoLookup = state.GetEntityStorageInfoLookup();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        localTransformComponentLookup.Update(ref state);
        entityStorageInfoLookup.Update(ref state);
        
        ResetTargetJob resetTargetJob = new ResetTargetJob
        {
            LocalTransformComponentLookup = localTransformComponentLookup,
            entityStorageInfoLookup = entityStorageInfoLookup
        };
        resetTargetJob.ScheduleParallel();
        
        ResetTargetOverrideJob resetTargetOverrideJob = new ResetTargetOverrideJob()
        {
            LocalTransformComponentLookup = localTransformComponentLookup,
            entityStorageInfoLookup = entityStorageInfoLookup
        };
        resetTargetOverrideJob.ScheduleParallel();
        
        /*foreach (RefRW<Target> target in SystemAPI.Query<RefRW<Target>>())
        {
            if (target.ValueRW.targetEntity != Entity.Null)
            {
                if (!SystemAPI.Exists(target.ValueRO.targetEntity) ||
                    !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.targetEntity))
                {
                    target.ValueRW.targetEntity = Entity.Null;
                }
            }
        }*/

        /*foreach (RefRW<TargetOverride> targetOverride in SystemAPI.Query<RefRW<TargetOverride>>())
        {
            if (targetOverride.ValueRW.targetEntity != Entity.Null)
            {
                if (!SystemAPI.Exists(targetOverride.ValueRO.targetEntity) ||
                    !SystemAPI.HasComponent<LocalTransform>(targetOverride.ValueRO.targetEntity))
                {
                    targetOverride.ValueRW.targetEntity = Entity.Null;
                }
            }
        }*/
    }
}

[BurstCompile]
public partial struct ResetTargetJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformComponentLookup;
    [ReadOnly] public EntityStorageInfoLookup entityStorageInfoLookup;
    public void Execute(ref Target target)
    {
        if (target.targetEntity != Entity.Null)
        {
            if (!entityStorageInfoLookup.Exists(target.targetEntity) ||
                !LocalTransformComponentLookup.HasComponent(target.targetEntity))
            {
                target.targetEntity = Entity.Null;
            }
        }
    }
}

[BurstCompile]
public partial struct ResetTargetOverrideJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformComponentLookup;
    [ReadOnly] public EntityStorageInfoLookup entityStorageInfoLookup;
    public void Execute(ref TargetOverride targetOverride)
    {
        if (targetOverride.targetEntity != Entity.Null)
        {
            if (!entityStorageInfoLookup.Exists(targetOverride.targetEntity) ||
                !LocalTransformComponentLookup.HasComponent(targetOverride.targetEntity))
            {
                targetOverride.targetEntity = Entity.Null;
            }
        }
    }
}