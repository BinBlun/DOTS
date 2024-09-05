using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

partial struct UnitMoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        UnitMoverJob unitMoverJob = new UnitMoverJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
        };
        unitMoverJob.ScheduleParallel();
        
        /*foreach ((
                     RefRW<LocalTransform> localTransform,
                     RefRO<UnitMover> unitMover,
                     RefRW<PhysicsVelocity> physicsVelocity)
                 in SystemAPI.Query<
                     RefRW<LocalTransform>,
                     RefRO<UnitMover>,
                     RefRW<PhysicsVelocity>>())
        {
            float3 moveDirection = unitMover.ValueRO.TargetPosition - localTransform.ValueRO.Position;
            moveDirection = math.normalize(moveDirection);

            //Quay
            localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation,
                quaternion.LookRotation(moveDirection, math.up()),
                SystemAPI.Time.DeltaTime * unitMover.ValueRO.RotationSpeed);

            //Di chuyen
            physicsVelocity.ValueRW.Linear = moveDirection * unitMover.ValueRO.MoveSpeed;
            physicsVelocity.ValueRW.Angular = float3.zero;

            //localTransform.ValueRW.Position += moveDirection * moveSpeed.ValueRO.Value * SystemAPI.Time.DeltaTime;
        }*/
    }
}

[BurstCompile]
public partial struct UnitMoverJob : IJobEntity
{
    public float DeltaTime;
    public void Execute(ref LocalTransform localTransform, in UnitMover unitMover, ref PhysicsVelocity physicsVelocity)
    {
        float3 moveDirection = unitMover.TargetPosition - localTransform.Position;

        float reachedTargetDistanceSq = 2f;
        if (math.lengthsq(moveDirection) < reachedTargetDistanceSq)
        {
            //Reach target
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            return;
        }
        
        moveDirection = math.normalize(moveDirection);

        //Quay
        localTransform.Rotation = math.slerp(localTransform.Rotation,
            quaternion.LookRotation(moveDirection, math.up()),
            DeltaTime * unitMover.RotationSpeed);

        //Di chuyen
        physicsVelocity.Linear = moveDirection * unitMover.MoveSpeed;
        physicsVelocity.Angular = float3.zero;
    }
}