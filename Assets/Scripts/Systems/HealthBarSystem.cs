using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct HealthBarSystem : ISystem
{
    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Vector3 cameraForward = Vector3.zero;
        if (Camera.main != null)
        {
            cameraForward = Camera.main.transform.forward;
        }

        foreach ((
                     RefRW<LocalTransform> localTransform,
                     RefRO<HealthBar> healthBar)
                 in SystemAPI.Query<
                     RefRW<LocalTransform>,
                     RefRO<HealthBar>>())
        {
            LocalTransform parentLocalTransform =
                SystemAPI.GetComponent<LocalTransform>(healthBar.ValueRO.healthEntity);
            
            if (localTransform.ValueRO.Scale == 1f)
            {
                //Healthbar is visible
                localTransform.ValueRW.Rotation =
                    parentLocalTransform.InverseTransformRotation(quaternion.LookRotation(cameraForward, math.up()));
            }
           
            Health health = SystemAPI.GetComponent<Health>(healthBar.ValueRO.healthEntity);

            if (!health.onHealthChanged)
            {
                continue;
            }
            
            float healthNormalized = (float)health.healthAmount / health.healthAmountMax;
            
            localTransform.ValueRW.Scale = healthNormalized == 1f ? 0f : 1f;

            RefRW<PostTransformMatrix> barVisualPostTransformMatrix =
                SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.barVisualEntity);
           
            barVisualPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);
        }
    }
}