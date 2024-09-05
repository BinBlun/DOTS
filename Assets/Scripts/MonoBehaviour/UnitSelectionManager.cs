using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();
            
            //Khong the su dung SystemAPI o ngoai ISystem phai Build
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover>().Build(entityManager);

            //Cho vao native array va day la data goc 
            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            //Query het UnitMover cho vao native array (local array)
            NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);
            for (int i = 0; i < unitMoverArray.Length; i++)
            {
                //Tao copy 
                UnitMover unitMover = unitMoverArray[i];
                //Modify copy
                unitMover.TargetPosition = mouseWorldPosition;
                
                //Cach 1:
                //Save data copy vao data goc
                entityManager.SetComponentData(entityArray[i], unitMover);
                
                //Cach 2:
                //unitMoverArray[i] = unitMover;
            }
            //entityQuery.CopyFromComponentDataArray(unitMoverArray);
        }
    }
}
