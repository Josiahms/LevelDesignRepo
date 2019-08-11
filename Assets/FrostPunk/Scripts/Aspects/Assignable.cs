using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Assignable : MonoBehaviour, ISaveable {

   [SerializeField]
   private int maxAssignees = 5;
   public int GetMaxAssignees() { return maxAssignees; }

   private List<Worker> workers = new List<Worker>();

   public void AddWorker() {
      if (workers.Count < maxAssignees) {
         var worker = PopulationManager.GetInstance().PopNearestWorker(transform.position);
         if (worker != null) {
            workers.Add(worker);
            worker.SetDestination(this);
         }
      }
   }

   public void RemoveWorker() {
      if (workers.Count > 0) {
         var worker = workers
            .OrderBy(x => Vector3.Distance(x.transform.position, transform.position))
            .OrderBy(x => Vector3.Distance(x.House.transform.position, transform.position))
            .Last();
         PopulationManager.GetInstance().PushWorker(worker);
         worker.SetDestination(null);
         workers.Remove(worker);
      }
   }

   public bool RemoveWorker(Worker worker) {
      return workers.Remove(worker);
   }

   public void OnDestroy() {
      try {
         var resourceManager = PopulationManager.GetInstance();
         foreach (var worker in workers) {
            resourceManager.PushWorker(worker);
            worker.SetDestination(null);
         }
         workers.Clear();
      } catch (Exception) {

      }
   }

   public int GetWorkersInRange() {
      return workers.Where(x => (x.transform.position - transform.position).magnitude < 1).Count();
   }

   public int GetWorkerCount() {
      return workers.Count;
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("workers", workers.Select(x => x.GetComponent<Saveable>().GetSavedIndex()).ToArray());
      return data;
   }

   public void OnLoad(object savedData) {
      // Ignored
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("workers", out result)) {
         workers = ((int[])result).Select(saveIndex => SaveManager.GetInstance().FindLoadedInstanceBySaveIndex(saveIndex).GetComponent<Worker>()).ToList();
      }
   }
}