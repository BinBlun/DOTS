using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public class UnitMoverAuthoring : MonoBehaviour
{
    public float MoveSpeed;
    public float RotationSpeed;

    public class MoveSpeedBaker : Baker<UnitMoverAuthoring>
    {
        public override void Bake(UnitMoverAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitMover
            {
                MoveSpeed = authoring.MoveSpeed,
                RotationSpeed = authoring.RotationSpeed,
            });
        }
    }
}

public struct UnitMover : IComponentData
{
    public float MoveSpeed;
    public float RotationSpeed;
    public float3 TargetPosition;
}