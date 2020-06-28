using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class WorkerInput : MonoBehaviour {

   private void Update() {
      if (Input.GetMouseButtonDown(1)) {
         var selectedAssignees = SelectionManager.GetInstance().GetSelected().Select(x => x.GetComponent<Assignee>()).Where(x => x != null).ToList();
         if (selectedAssignees.Count > 0) {
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, float.MaxValue)) {
               var assignable = hitInfo.collider.GetComponent<Assignable>();
               if (assignable != null) {
                  selectedAssignees.ForEach(x => SetAssignees(x, assignable));
               } else {
                  for (int i = 0; i < selectedAssignees.Count; i++) {
                     var offset = new Vector3(3 * (i % 5), 0, 3 * (i / 5));
                     selectedAssignees[i].SetTarget(null);
                     selectedAssignees[i].GetComponent<Walker>().SetDestination(hitInfo.point + offset);
                  }
               }
            }
         }
      }
   }

   private void SetAssignees(Assignee assignee, Assignable assignable) {
      if (assignee.SetDestination(assignable)) {
         SelectionManager.GetInstance().Deselect(assignee.GetComponent<Selectable>());
      }
   }

}
