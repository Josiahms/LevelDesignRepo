using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public interface IInstructable {
   void OnInstruct(Vector3 location, GameObject target, List<Selectable> originalSelection);
}

public class WorkerInput : MonoBehaviour {

   private void Update() {
      if (Input.GetMouseButtonDown(1)) {
         var currentSelection = SelectionManager.GetInstance().GetSelected();
         var copy = new Selectable[currentSelection.Count];
         currentSelection.CopyTo(copy);
         var selectedItems = currentSelection.ToList();
         if (selectedItems.Count > 0) {
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, float.MaxValue)) {
               selectedItems.ForEach(x => x.GetComponents<IInstructable>().ToList().ForEach(y => y.OnInstruct(hitInfo.point, hitInfo.collider.gameObject, selectedItems)));
            }
         }
      }
   }
}
