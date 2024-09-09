using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

public partial struct HealthDeadTestSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((
                     RefRO<Health> health,
                     Entity entity)
                 in SystemAPI.Query<
                     RefRO<Health>>().WithEntityAccess())
        {
            if (health.ValueRO.healthAmount <= 0)
            {
                //this entity is dead
                entityCommandBuffer.DestroyEntity(entity);
                //state.EntityManager.DestroyEntity(entity);
            }
        }

        entityCommandBuffer.Playback(state.EntityManager);
    }
}