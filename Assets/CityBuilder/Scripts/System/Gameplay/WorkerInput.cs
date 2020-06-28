using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class WorkerInput : MonoBehaviour {

   private void Update() {
      if (Input.GetMouseButtonDown(1)) {
         var selectedTargeters = SelectionManager.GetInstance().GetSelected().Select(x => x.GetComponent<Targeter>()).Where(x => x != null).ToList();
         if (selectedTargeters.Count > 0) {
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, float.MaxValue)) {
               var target = hitInfo.collider.GetComponent<Targetable>();
               if (target != null) {
                  selectedTargeters.ForEach(x => SetTarget(x, target));
               } else {
                  for (int i = 0; i < selectedTargeters.Count; i++) {
                     var offset = new Vector3(3 * (i % 5), 0, 3 * (i / 5));
                     selectedTargeters[i].SetTarget(null);
                     selectedTargeters[i].GetComponent<Walker>().SetDestination(hitInfo.point + offset);
                  }
               }
            }
         }
      }
   }

   private void SetTarget(Targeter targeter, Targetable target) {
      if (targeter.SetDestination(target)) {
         SelectionManager.GetInstance().Deselect(targeter.GetComponent<Selectable>());
      }
   }

}
