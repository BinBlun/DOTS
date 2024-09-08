using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace.Authoring
{
    public class ShootAttackAuthoring : MonoBehaviour
    {
        public float timerMax;
        
        private class ShootAttackAuthoringBaker : Baker<ShootAttackAuthoring>
        {
            public override void Bake(ShootAttackAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ShootAttack
                {
                    timerMax = authoring.timerMax,
                });
            }
        }
    }
}

public struct ShootAttack : IComponentData
{
    public float timer;
    public float timerMax;
}

