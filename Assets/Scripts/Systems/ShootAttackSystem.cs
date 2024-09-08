using Unity.Burst;
using Unity.Entities;

namespace DefaultNamespace.Systems
{
    public partial struct ShootAttackSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((
                         RefRW<ShootAttack> shootAttack,
                         RefRO<Target> target) 
                     in SystemAPI.Query<
                         RefRW<ShootAttack>, 
                         RefRO<Target>>())
            {
                if (target.ValueRO.targetEntity == Entity.Null)
                {
                    continue;
                }

                shootAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (shootAttack.ValueRO.timer > 0f)
                {
                    continue;
                }
                shootAttack.ValueRW.timer = shootAttack.ValueRO.timerMax;
            }
        }
    }
}