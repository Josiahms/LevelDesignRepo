using System;
using UnityEngine;

public class Assignee : MonoBehaviour {

   private Assignable _target;
   public Assignable target { 
      get { return _target; } 
      set { 
         if (value == _target) {
            return;
         }
         if (_target != null) {
            _target.RemoveAssignee(this);
         }
         if (value != null) {
            value.AddAssignee(this);
         }
         _target = value;
      } 
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