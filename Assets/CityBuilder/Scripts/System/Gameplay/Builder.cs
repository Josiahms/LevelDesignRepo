using UnityEngine;
using UnityEngine.EventSystems;

public class Builder : Singleton<Builder> {

   private static Placeable buildingInstance;
   public static bool IsBuilding() { return buildingInstance != null; }

   private Placeable buildingPrefab;

   public void SetBuilding(Placeable buildingPrefab) {
      SetBuilding(buildingPrefab, Vector3.zero);
   }

   public Placeable GetBuilding() {
      return buildingInstance;
   }

   public void SetBuilding(Placeable buildingPrefab, Vector3 position) {
      if (buildingInstance != null) {
         buildingInstance.Remove();
      }
      this.buildingPrefab = buildingPrefab;
      buildingInstance = Instantiate(buildingPrefab, position, buildingPrefab.transform.rotation);
      Selectable.Deselect();
      Selectable.Disable();
   }

   public void ClearBuilding() {
      if (buildingInstance != null) {
         buildingInstance.Remove();
         buildingInstance = null;
         Selectable.Enable();
      }
   }

   private void Update() {
      var cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hitInfo;
      if (buildingInstance != null) {

         if (Input.GetKeyDown(KeyCode.R)) {
            buildingInstance.transform.Rotate(new Vector3(0, 90, 0), Space.World);
         }

         if (Input.GetKeyDown(KeyCode.Escape)) {
            ClearBuilding();
            return;
         }

         if (CanBuild()) {
            buildingInstance.GetComponent<Selectable>().ChangeColor(Color.green);
         } else {
            buildingInstance.GetComponent<Selectable>().ChangeColor(Color.red);
         }

         if (Physics.Raycast(cameraRay, out hitInfo, float.MaxValue, LayerMask.GetMask("Ground"))) {
            buildingInstance.transform.position = ToGrid(hitInfo.point);

            if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject()) {
               if (buildingInstance.Place()) {
                  buildingInstance.GetComponent<Selectable>().ChangeColor(Color.white);
                  buildingInstance = null;
                  SetBuilding(buildingPrefab, hitInfo.point);
               }
            } else if (Input.GetMouseButtonUp(1)) {
               ClearBuilding();
               return;
            }
         }
      }
   }

   private Vector3 ToGrid(Vector3 input) {
      const int gridScale = 12;
      var offsetVect = new Vector3(input.x < 0 ? -1 : 1, 0, input.z < 0 ? -1 : 1) * gridScale / 2;
      return new Vector3((int)input.x / gridScale, 0, (int)input.z / gridScale) * gridScale + offsetVect;
   }


   private bool CanBuild() {
      if (buildingInstance != null) {
         if (ResourceManager.GetInstance().CanAfford(-buildingInstance.GetWoodCost(), -buildingInstance.GetStoneCost(), -buildingInstance.GetMetalCost(), 0)) {
            // TODO: If the spot isn't blocked
            return true;
         }
      }
      return false;
   }

}