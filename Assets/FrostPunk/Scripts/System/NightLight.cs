using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class NightLight : MonoBehaviour {

   private Light li;

   private void Awake() {
      li = GetComponent<Light>();
   }

   private void Update() {
      li.enabled = DayCycleManager.GetInstance().IsRestTime();
   }

}
