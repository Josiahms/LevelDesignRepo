using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Assignable : MonoBehaviour, ISaveable {

   [SerializeField]
   private List<Assignee> allowableAssignments;

   [SerializeField]
   private int maxAssignees = 5;
   public int GetMaxAssignees() { return maxAssignees; }

   [SerializeField]
   private List<Transform> spots;

   private List<Assignee> assignees = new List<Assignee>();

   // Returns true if the assignee was added, or is already assigned to this location
   public bool AddAssignee(Assignee assignee) {

      if (!assignees.Contains(assignee)) {

         if (assignee == null || assignees.Count >= maxAssignees) {
            return false;
         }

         assignees.Add(assignee);
         assignee.target = this;
      }

      return true;
   }

   public bool RemoveAssignee(Assignee assignee) {
      if (assignees.Remove(assignee)) {
         assignee.target = null;
         return true;
      }
      return false;
   }

   public void OnDestroy() {
      try {
         foreach (var assignee in assignees) {
            assignee.target = null;
         }
         assignees.Clear();
      } catch (Exception) {

      }
   }

   public int GetAssigneesInRange() {
      return assignees.Where(x => DayCycleManager.GetInstance().IsWorkDay() && (x.transform.position - transform.position).magnitude < 4).Count();
   }

   public int GetAssigneeCount() {
      return assignees.Count;
   }

   public List<Assignee> GetAssignees() {
      return assignees;
   }

   public Transform GetSpotForAssignee(Assignee assignee) {
      if (!assignees.Contains(assignee)) {
         return transform;
      }

      var index = assignees.IndexOf(assignee);
      if (index < spots.Count) {
         return spots[index];
      } 
      return transform;
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("assignees", assignees.Select(x => x.GetComponent<Saveable>().GetSavedIndex()).ToArray());
      return data;
   }

   public void OnLoad(object savedData) {
      // Ignored
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("assignees", out result)) {
         assignees = ((int[])result).Select(saveIndex => SaveManager.GetInstance().FindLoadedInstanceBySaveIndex(saveIndex).GetComponent<Assignee>()).ToList();
      }
   }
}