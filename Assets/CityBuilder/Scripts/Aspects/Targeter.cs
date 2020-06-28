using System;
using UnityEngine;

public class Targeter : MonoBehaviour {

   public Targetable target { get; private set; }
   public Vector3 targetLocation { get; private set; }

   private void Start() { 
      if (target == null) {
         targetLocation = transform.position;
      }
   }

   private void Update() {
      if (target != null) {
         targetLocation = target.GetSpotForTargeter(this).position;
      }
   }

   public bool SetTarget(Vector3 target) {
      if (SetTarget(null)) {
         targetLocation = target;
         return true;
      }
      return false;
   }

   public bool SetTarget(Targetable newTarget) {
      if (target == newTarget) {
         return true;
      }

      if (target != null) {
         var oldTarget = target;
         target = null;
         oldTarget.RemoveTargeter(this);
      }

      if (newTarget != null) {
         if (!newTarget.AddTargeter(this)) {
            return false;
         }
      }

      if (target == null) {
         targetLocation = transform.position;
      }

      target = newTarget;
      return true;
   }

   private void OnDestroy() {
      if (target != null) {
         target.RemoveTargeter(this);
      }
   }
}