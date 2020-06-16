using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Workstation))]
public class Field : MonoBehaviour {

   [SerializeField]
   private float secondsToFullyGrow = 10;
   [SerializeField]
   private float cropGrowth;
   [SerializeField]
   private bool fullyGrown;
   [SerializeField]
   private Transform crops;

   private void Update() {

      cropGrowth += (DayCycleManager.GetInstance().ClockMinuteRate * Time.deltaTime / Mathf.Max(1, secondsToFullyGrow));
      cropGrowth = Mathf.Clamp(cropGrowth, 0.1f, 1);
      crops.localScale = new Vector3(crops.localScale.x, cropGrowth, crops.localScale.z);

      if (cropGrowth == 1 && !fullyGrown) {
         fullyGrown = true;
         GetComponent<Workstation>().SetQuantity(10);
      }

      if (GetComponent<Workstation>().GetQuantity() == 0 && fullyGrown) {
         cropGrowth = 0.1f;
         fullyGrown = false;
      }
   }

}