using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Targetable : MonoBehaviour, ISaveable {

   [SerializeField]
   private List<Component> allowableAssignments;

   [SerializeField]
   private int maxTargeters = 5;
   public int GetMaxNumberOfTargeters() { return maxTargeters; }

   [SerializeField]
   private List<Transform> spots;

   private List<Targeter> targeters = new List<Targeter>();

   // Returns true if the targeter was added, or is already assigned to this location
   public bool AddTargeter(Targeter targeter) {

      if (targeter == null) {
         return false;
      }

      if (targeters.Contains(targeter)) {
         return true;
      }

      if (maxTargeters > 0 && targeters.Count >= maxTargeters) {
         return false;
      }

      if (allowableAssignments.Count() > 0 && allowableAssignments.Where(x => targeter.GetComponent(x.GetType()) != null).Count() == 0) {
         return false;
      }

      targeters.Add(targeter);
      targeter.SetTarget(this);

      return true;
   }

   public void RemoveTargeter(Targeter targeter) {
      if (targeters.Remove(targeter)) {
         targeter.SetTarget(null);
      }
   }

   public void OnDestroy() {
      try {
         foreach (var targeter in targeters) {
            targeter.SetTarget(null);
         }
         targeters.Clear();
      } catch (Exception) {

      }
   }

   public int GetTargetersInRange() {
      return targeters.Where(x => DayCycleManager.GetInstance().IsWorkDay() && (x.transform.position - transform.position).magnitude < 4).Count();
   }

   public int GetTargeterCount() {
      return targeters.Count;
   }

   public List<Targeter> GetTargeters() {
      return targeters;
   }

   public Transform GetSpotForTargeter(Targeter targeter) {
      if (!targeters.Contains(targeter)) {
         return transform;
      }

      var index = targeters.IndexOf(targeter);
      if (index < spots.Count) {
         return spots[index];
      } 
      return transform;
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("targeters", targeters.Select(x => x.GetComponent<Saveable>().GetSavedIndex()).ToArray());
      return data;
   }

   public void OnLoad(object savedData) {
      // Ignored
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("targeters", out result)) {
         targeters = ((int[])result).Select(saveIndex => SaveManager.GetInstance().FindLoadedInstanceBySaveIndex(saveIndex).GetComponent<Targeter>()).ToList();
      }
   }
}