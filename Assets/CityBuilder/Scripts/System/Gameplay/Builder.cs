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
      MeshDeformer.GetInstance().AddMesh(buildingInstance.transform);
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
            buildingInstance.transform.LookAt(MeshDeformer.GetInstance().transform);

            if (CanBuild() && Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject()) {
               ResourceManager.GetInstance().OffsetAll(-buildingInstance.GetWoodCost(), -buildingInstance.GetStoneCost(), -buildingInstance.GetMetalCost(), 0);
               BuildSite.Instantiate(buildingInstance);
               buildingInstance = null;
               Selectable.Enable();
            } else if (Input.GetMouseButtonUp(1)) {
               ClearBuilding();
               return;
            }
         }
      }
   }

   private Vector3 ToGrid(Vector3 input) {
      const int GRID_SACLE = 12;
      const int MIN_NUMBER = 8;

      var center = Vector3.Scale(MeshDeformer.GetInstance().transform.position, new Vector3(1, 0, 1));
      var distanceVect = Vector3.Scale(input, new Vector3(1, 0, 1)) - center;
      var numBuildingsInCircle = (int)Mathf.Max(Mathf.Ceil((distanceVect.magnitude + 6) / (GRID_SACLE / 2 / Mathf.PI)), MIN_NUMBER);
      var numBuildingsSnapped = ((numBuildingsInCircle - 8) / 8 * 8) + 8;
      var radius = numBuildingsSnapped * GRID_SACLE / 2 / Mathf.PI;

      var currentAngle = Vector3.SignedAngle(new Vector3(1, 0, 0), distanceVect, Vector3.down);
      var angleIncrement = 360f / numBuildingsSnapped;
      var snappedAngle = Mathf.Floor((currentAngle + angleIncrement / 2f) / angleIncrement) * angleIncrement;

      var snappedAngleVect = new Vector3(Mathf.Cos(Mathf.Deg2Rad * snappedAngle), 0, Mathf.Sin(Mathf.Deg2Rad * snappedAngle));

      return center + radius * snappedAngleVect;
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