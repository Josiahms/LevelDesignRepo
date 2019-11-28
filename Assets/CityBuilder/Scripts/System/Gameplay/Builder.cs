using UnityEngine;
using UnityEngine.EventSystems;

public class Builder : Singleton<Builder> {

   private static Placeable buildingInstance;
   public static bool IsBuilding() { return buildingInstance != null; }

   private Placeable buildingPrefab;

   public Placeable GetBuilding() {
      return buildingInstance;
   }

   public void SetBuilding(Placeable buildingPrefab, TownCenter townCenter) {
      if (buildingInstance != null) {
         buildingInstance.Remove();
      }
      this.buildingPrefab = buildingPrefab;
      buildingInstance = Instantiate(buildingPrefab, Vector3.zero, buildingPrefab.transform.rotation);
      buildingInstance.TownCenter = townCenter;
      SelectionManager.GetInstance().Deselect();
      SelectionManager.GetInstance().Disable();
   }

   public void ClearBuilding() {
      if (buildingInstance != null) {
         buildingInstance.Remove();
         buildingInstance = null;
         SelectionManager.GetInstance().Enable();
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
            buildingInstance.transform.position = hitInfo.point;
            if (CanBuild() && Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject()) {
               if (ResourceManager.GetInstance().OffsetAll(-buildingInstance.GetWoodCost(), -buildingInstance.GetStoneCost(), -buildingInstance.GetMetalCost(), 0)) {
                  BuildSite.Instantiate(buildingInstance);
                  buildingInstance = null;
                  SelectionManager.GetInstance().Enable();
               }
            } else if (Input.GetMouseButtonUp(1)) {
               ClearBuilding();
               return;
            }
         }
      }
   }

   private bool CanBuild() {
      if (buildingInstance != null) {
         if (ResourceManager.GetInstance().CanAfford(-buildingInstance.GetWoodCost(), -buildingInstance.GetStoneCost(), -buildingInstance.GetMetalCost(), 0)) {
            return !buildingInstance.IsBlocked();
         }
      }
      return false;
   }

}