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
                  for (int i = 0; i < selectedTargeters.Count(); i++) {
                     var offset = new Vector3(3 * (i % 5), 0, 3 * (i / 5));
                     if (selectedTargeters[i].SetTarget(hitInfo.point + offset)) {
                        SelectionManager.GetInstance().Deselect(selectedTargeters[i].GetComponent<Selectable>());
                     }
                  }
               }
            }
         }
      }
   }

   private void SetTarget(Targeter targeter, Targetable target) {
      if (targeter.SetTarget(target)) {
         SelectionManager.GetInstance().Deselect(targeter.GetComponent<Selectable>());
      }
   }
}
