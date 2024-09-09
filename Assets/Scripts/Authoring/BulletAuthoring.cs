using Unity.Entities;
using UnityEngine;


public class BulletAuthoring : MonoBehaviour
{
    public float speed;
    public int damageAmount;
    private class BulletAuthoringBaker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Bullet
            {
                speed = authoring.speed,
                damageAmount = authoring.damageAmount,
            });
        }
    }
}


public struct Bullet : IComponentData
{
    public float speed;
    public Entity targetEntity;
    public int damageAmount;
}