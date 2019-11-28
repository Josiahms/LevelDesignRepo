using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerInput : MonoBehaviour {

   private void Update() {
      if (Input.GetMouseButtonDown(1)) {
         var selected = SelectionManager.GetInstance().GetFirstSelected();
         var selectedWorker = selected != null ? selected.GetComponent<Worker>() : null;
         if (selectedWorker != null) {
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, float.MaxValue)) {
               var assignable = hitInfo.collider.GetComponent<Assignable>();
               if (assignable != null) {
                  selectedWorker.SetDestination(assignable);
               } else {
                  selectedWorker.SetDestination(hitInfo.point);
               }
            }
         }
      }
   }

}
