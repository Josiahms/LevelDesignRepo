using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class WorkerInput : MonoBehaviour {

   private void Update() {
      if (Input.GetMouseButtonDown(1)) {
         var selectedWorkers = SelectionManager.GetInstance().GetSelected().Select(x => x.GetComponent<Worker>()).Where(x => x != null).ToList();
         if (selectedWorkers.Count > 0) {
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, float.MaxValue)) {
               var assignable = hitInfo.collider.GetComponent<Assignable>();
               if (assignable != null) {
                  selectedWorkers.ForEach(x => SetWorkerDestination(x, assignable));
               } else {
                  for (int i = 0; i < selectedWorkers.Count; i++) {
                     var offset = new Vector3(3 * (i % 5), 0, 3 * (i / 5));
                     selectedWorkers[i].SetDestination(hitInfo.point + offset);
                  }
               }
            }
         }
      }
   }

   private void SetWorkerDestination(Worker w, Assignable a) {
      if (w.SetDestination(a)) {
         SelectionManager.GetInstance().Deselect(w.GetComponent<Selectable>());
      }
   }

}
