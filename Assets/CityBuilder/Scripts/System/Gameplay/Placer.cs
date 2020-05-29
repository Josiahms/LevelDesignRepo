using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Placer : Singleton<Placer> {

   private static Placeable placeableInstance;
   public static bool IsPlacing() { return placeableInstance != null; }

   public Placeable GetPlaceable() {
      return placeableInstance;
   }

   public void SetPlaceable(Placeable prefab) {
      if (placeableInstance != null) {
         placeableInstance.Remove();
      }
      placeableInstance = Instantiate(prefab, Vector3.zero, prefab.transform.rotation);
      var center = SelectionManager.GetInstance().GetFirstSelected().GetComponent<CentralNode>();
      var grid = placeableInstance.GetComponent<SnapToCircleGrid>();
      if (grid != null && center != null) {
         grid.SetCenter(center.MinNumber, center.transform.position);
      }

      SelectionManager.GetInstance().Disable();
      Update();
   }

   public void ClearPlaceable() {
      if (placeableInstance != null) {
         placeableInstance.Remove();
         placeableInstance = null;
         SelectionManager.GetInstance().Enable();
      }
   }

   private void Update() {
      var cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hitInfo;
      if (placeableInstance != null) {

         if (Input.GetKeyDown(KeyCode.R)) {
            placeableInstance.transform.Rotate(new Vector3(0, 90, 0), Space.World);
         }

         if (Input.GetKeyDown(KeyCode.Escape)) {
            ClearPlaceable();
            return;
         }

         if (CanPlace()) {
            placeableInstance.GetComponent<Selectable>().ChangeColor(Color.green);
         } else {
            placeableInstance.GetComponent<Selectable>().ChangeColor(Color.red);
         }

         var selectionManager = SelectionManager.GetInstance();
         if (Physics.Raycast(cameraRay, out hitInfo, float.MaxValue, LayerMask.GetMask("Ground"))) {
            placeableInstance.transform.position = hitInfo.point;
            if (CanPlace() && Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject()) {
               if (ResourceManager.GetInstance().OffsetAll(-placeableInstance.GetWoodCost(), -placeableInstance.GetStoneCost(), -placeableInstance.GetMetalCost(), 0)) {
                  var buildSite = BuildSite.Instantiate(placeableInstance).GetComponent<Assignable>();
                  foreach (var worker in selectionManager.GetSelected().Where(s => s.GetComponent<Worker>() != null).Select(s => s.GetComponent<Worker>()).OrderBy(s => s.IsAssigned())) {
                     if (!buildSite.AddWorker(worker)) {
                        break;
                     }
                  }
                  placeableInstance.GetComponent<Selectable>().ChangeColor(Color.clear);
                  placeableInstance = null;
                  selectionManager.DeselectAll();
                  selectionManager.Enable();
               }
            } else if (Input.GetMouseButtonUp(1)) {
               ClearPlaceable();
               return;
            }
         }
      }
   }

   private bool CanPlace() {
      if (placeableInstance != null) {
         if (ResourceManager.GetInstance().CanAfford(-placeableInstance.GetWoodCost(), -placeableInstance.GetStoneCost(), -placeableInstance.GetMetalCost(), 0)) {
            return !placeableInstance.IsBlocked();
         }
      }
      return false;
   }

}