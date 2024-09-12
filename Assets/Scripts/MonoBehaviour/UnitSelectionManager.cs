using System;
using DefaultNamespace;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;


public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }
    public event EventHandler OnSelectionAreaStart;
    public event EventHandler OnSelectionAreaEnd;

    private Vector2 _selectionStartMousePosition;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _selectionStartMousePosition = Input.mousePosition;
            OnSelectionAreaStart?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 _selectionEndMousePosition = Input.mousePosition;

            //Khong the su dung SystemAPI o ngoai ISystem phai Build
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<Selected> selectedArray = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);
            for (int i = 0; i < entityArray.Length; i++)
            {
                entityManager.SetComponentEnabled<Selected>(entityArray[i], false);
                Selected selected = selectedArray[i];
                selected.onDeselected = true;
                selectedArray[i] = selected;
                entityManager.SetComponentData(entityArray[i], selected);
            }

            Rect selectionAreaRect = GetSelectionAreaRect();
            float selectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
            float multipleSelectionSizeMin = 40f;
            bool isMultipleSelection = selectionAreaSize > multipleSelectionSizeMin;

            if (isMultipleSelection)
            {
                entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>()
                    .WithPresent<Selected>()
                    .Build(entityManager);

                entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<LocalTransform> localTransformsArray =
                    entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                for (int i = 0; i < localTransformsArray.Length; i++)
                {
                    LocalTransform unitLocalTransform = localTransformsArray[i];
                    Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(unitLocalTransform.Position);
                    if (selectionAreaRect.Contains(unitScreenPosition))
                    {
                        //Unit inside the selection area
                        entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
                        Selected selected = entityManager.GetComponentData<Selected>(entityArray[i]);
                        selected.onSelected = true;
                        entityManager.SetComponentData(entityArray[i], selected);
                    }
                }
            }
            else
            {
                //Single select
                //Cach 1: De query 
                //entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>().Build(entityManager);

                //Cach 2:
                entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
                PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
                CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
                UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastInput raycastInput = new RaycastInput
                {
                    Start = cameraRay.GetPoint(0f),
                    End = cameraRay.GetPoint(9999f),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << GameAssets.UNITS_LAYER,
                        GroupIndex = 0,
                    }
                };
                if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
                {
                    if (entityManager.HasComponent<Unit>(raycastHit.Entity) &&
                        entityManager.HasComponent<Selected>(raycastHit.Entity))
                    {
                        entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);
                        Selected selected = entityManager.GetComponentData<Selected>(raycastHit.Entity);
                        selected.onSelected = true;
                        entityManager.SetComponentData(raycastHit.Entity, selected);
                    }
                }
            }


            OnSelectionAreaEnd?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();

            //Khong the su dung SystemAPI o ngoai ISystem phai Build
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
            PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
            UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastInput raycastInput = new RaycastInput
            {
                Start = cameraRay.GetPoint(0f),
                End = cameraRay.GetPoint(9999f),
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.UNITS_LAYER,
                    GroupIndex = 0,
                }
            };
            bool isAttackingSingletarget = false;
            if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
            {
                if (entityManager.HasComponent<Unit>(raycastHit.Entity))
                {
                    Unit unit = entityManager.GetComponentData<Unit>(raycastHit.Entity);
                    if (unit.faction == Faction.Zombie)
                    {
                        //right click on a Zombie
                        isAttackingSingletarget = true;
                        
                        entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>()
                            .WithPresent<TargetOverride>()
                            .Build(entityManager);
                        
                        NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                        NativeArray<TargetOverride> targetOverrideArray = entityQuery.ToComponentDataArray<TargetOverride>(Allocator.Temp);
                        for (int i = 0; i < targetOverrideArray.Length; i++)
                        {
                            TargetOverride targetOverride = targetOverrideArray[i];
                            targetOverride.targetEntity = raycastHit.Entity;
                            targetOverrideArray[i] = targetOverride;
                            entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], false);
                        }

                        entityQuery.CopyFromComponentDataArray(targetOverrideArray);
                    }
                }
            }

            if (!isAttackingSingletarget)
            {
                entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>()
                    .WithPresent<MoveOverride, TargetOverride>()
                    .Build(entityManager);

                //Cho vao native array va day la data goc 
                NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                //Query het UnitMover cho vao native array (local array)
                NativeArray<MoveOverride> moveOverrideArray = entityQuery.ToComponentDataArray<MoveOverride>(Allocator.Temp);
                NativeArray<TargetOverride> targetOverrideArray = entityQuery.ToComponentDataArray<TargetOverride>(Allocator.Temp);
                NativeArray<float3> movePositionArray =
                    GenerateMovePositionArray(mouseWorldPosition, entityArray.Length);
                for (int i = 0; i < moveOverrideArray.Length; i++)
                {
                    //Tao copy 
                    MoveOverride moveOverride = moveOverrideArray[i];
                    //Modify copy
                    moveOverride.targetPosition = movePositionArray[i];
                    moveOverrideArray[i] = moveOverride;
                    entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], true);
                    
                    TargetOverride targetOverride = targetOverrideArray[i];
                    targetOverride.targetEntity = Entity.Null;
                    targetOverrideArray[i] = targetOverride;
                }

                entityQuery.CopyFromComponentDataArray(moveOverrideArray);
                entityQuery.CopyFromComponentDataArray(targetOverrideArray);
            }
        }
    }

    public Rect GetSelectionAreaRect()
    {
        Vector2 selectionEndMousePosition = Input.mousePosition;
        Vector2 lowerLeftCorner = new Vector2(
            Mathf.Min(_selectionStartMousePosition.x, selectionEndMousePosition.x),
            Mathf.Min(_selectionStartMousePosition.y, selectionEndMousePosition.y));
        Vector2 upperRightCorner = new Vector2(
            Mathf.Max(_selectionStartMousePosition.x, selectionEndMousePosition.x),
            Mathf.Max(_selectionStartMousePosition.y, selectionEndMousePosition.y));

        return new Rect(
            lowerLeftCorner.x,
            lowerLeftCorner.y,
            upperRightCorner.x - lowerLeftCorner.x,
            upperRightCorner.y - lowerLeftCorner.y
        );
    }

    private NativeArray<float3> GenerateMovePositionArray(float3 targetPosition, int positionCount)
    {
        NativeArray<float3> positionArray = new NativeArray<float3>(positionCount, Allocator.Temp);
        if (positionCount == 0)
        {
            return positionArray;
        }

        positionArray[0] = targetPosition;
        if (positionCount == 1)
        {
            return positionArray;
        }

        float ringSize = 2.2f;
        int ring = 0;
        int positionIndex = 1;

        while (positionIndex < positionCount)
        {
            int ringPositionCount = 3 + ring * 2;

            for (int i = 0; i < ringPositionCount; i++)
            {
                float angle = i * (math.PI2 / ringPositionCount);
                float3 ringVector = math.rotate(quaternion.RotateY(angle), new float3(ringSize * (ring + 1), 0, 0));
                float3 ringPosition = targetPosition + ringVector;

                positionArray[positionIndex] = ringPosition;
                positionIndex++;

                if (positionIndex >= positionCount)
                {
                    break;
                }
            }

            ring++;
        }

        return positionArray;
    }
}