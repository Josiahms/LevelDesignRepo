using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachUIToTarget : MonoBehaviour {

   // UI must be anchored to lower left

   [SerializeField]
   private Transform target;

   public void SetTarget(Transform target) {
      this.target = target;
   }

   private void LateUpdate() {
      if (target != null) {
         GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(target.position);
      } else {
         Destroy(gameObject);
      }
   }
}
