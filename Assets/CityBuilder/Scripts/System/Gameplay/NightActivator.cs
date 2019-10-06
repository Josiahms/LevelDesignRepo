using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightActivator : MonoBehaviour {

   private void Update() {
      for (int i = 0; i < transform.childCount; i++) {
         transform.GetChild(i).gameObject.SetActive(DayCycleManager.GetInstance().IsRestTime());
      }
   }

}
