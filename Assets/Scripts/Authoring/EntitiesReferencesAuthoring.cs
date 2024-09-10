using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;


public class EntitiesReferencesAuthoring : MonoBehaviour
{
    public GameObject bulletPrefabGameObject;
    public GameObject zombiePrefabGameObject;
    
    private class EntitiesReferencesAuthoringBaker : Baker<EntitiesReferencesAuthoring>
    {
        public override void Bake(EntitiesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences
            {
                bulletPrefabEntity = GetEntity(authoring.bulletPrefabGameObject, TransformUsageFlags.Dynamic),
                zombiePrefabEntity = GetEntity(authoring.zombiePrefabGameObject, TransformUsageFlags.Dynamic),
            });
        }
    }
}


public struct EntitiesReferences : IComponentData
{
    public Entity bulletPrefabEntity;
    public Entity zombiePrefabEntity;
}