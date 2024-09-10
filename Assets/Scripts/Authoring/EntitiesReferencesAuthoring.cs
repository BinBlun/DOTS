using Unity.Entities;
using UnityEngine;


public class EntitiesReferencesAuthoring : MonoBehaviour
{
    public GameObject BulletPrefabGameObject;
    
    private class EntitiesReferencesAuthoringBaker : Baker<EntitiesReferencesAuthoring>
    {
        public override void Bake(EntitiesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences
            {
                bulletPrefabEntity = GetEntity(authoring.BulletPrefabGameObject, TransformUsageFlags.Dynamic),
            });
        }
    }
}


public struct EntitiesReferences : IComponentData
{
    public Entity bulletPrefabEntity;
}