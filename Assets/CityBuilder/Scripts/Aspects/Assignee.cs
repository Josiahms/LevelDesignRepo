using System;
using UnityEngine;

public class Assignee : MonoBehaviour {

   public Assignable target { get; private set; }

   public bool SetTarget(Assignable newTarget) {
      if (target == newTarget) {
         return true;
      }

      if (newTarget != null) {
         if (!newTarget.AddAssignee(this)) {
            return false;
         }
      }

      if (target != null) {
         target.RemoveAssignee(this);
      }

      target = newTarget;
      return true;
   }

   private void OnDestroy() {
      if (target != null) {
         target.RemoveAssignee(this);
      }
   }

   public bool SetDestination(Assignable assignment) {
      if (assignment == null || assignment.AddAssignee(this)) {
         if (target != null && target != assignment) {
            target.RemoveAssignee(this);
         }
         target = assignment;
         return true;
      }
      return false;
   }

}