using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

public class UnitMoverAuthoring : MonoBehaviour
{
    public float Value;

    public class MoveSpeedBaker : Baker<UnitMoverAuthoring>
    {
        public override void Bake(UnitMoverAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitMover
            {
                Value = authoring.Value,
            });
        }
    }
}

public struct UnitMover : IComponentData
{
    public float Value;
}