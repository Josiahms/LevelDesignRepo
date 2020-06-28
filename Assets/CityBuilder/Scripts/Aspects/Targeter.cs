using System;
using UnityEngine;

public class Targeter : MonoBehaviour {

   public Targetable target { get; private set; }

   public bool SetTarget(Targetable newTarget) {
      if (target == newTarget) {
         return true;
      }

      if (newTarget != null) {
         if (!newTarget.AddTargeter(this)) {
            return false;
         }
      }

      if (target != null) {
         target.RemoveTargeter(this);
      }

      target = newTarget;
      return true;
   }

   private void OnDestroy() {
      if (target != null) {
         target.RemoveTargeter(this);
      }
   }

   public bool SetDestination(Targetable assignment) {
      if (assignment == null || assignment.AddTargeter(this)) {
         if (target != null && target != assignment) {
            target.RemoveTargeter(this);
         }
         target = assignment;
         return true;
      }
      return false;
   }

}