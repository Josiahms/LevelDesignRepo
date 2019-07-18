using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class DayNightLight : MonoBehaviour {

   private Light li;

   [SerializeField]
   private Gradient lightColor;

   private void Awake() {
      li = GetComponent<Light>();
   }

   private void Update() {
      li.color = lightColor.Evaluate((DayCycleManager.GetInstance().CurrentTime % DayCycleManager.MIN_IN_DAY) / DayCycleManager.MIN_IN_DAY);
   }

}
