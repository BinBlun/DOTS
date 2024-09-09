using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace.Authoring
{
    public class HealthAuthoring : MonoBehaviour
    {
        public int healthAmount;
        private class Baker : Baker<HealthAuthoring>
        {
            public override void Bake(HealthAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Health
                {
                    healthAmount = authoring.healthAmount,
                });
            }
        }
    }
}

public struct Health : IComponentData
{
    public int healthAmount;
}