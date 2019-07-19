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
      var resourceManager = ResourceManager.GetInstance();
      Worker worker = null;
      if (workers.Count < maxAssignees && resourceManager.AssignWorker(transform, ref worker)) {
         workers.Add(worker);
      }
   }

   public void RemoveWorker() {
      if (workers.Count > 0) {
         ResourceManager.GetInstance().ReturnWorker(workers[0]);
         workers.RemoveAt(0);
      }
   }

   public void OnDestroy() {
      try {
         var resourceManager = ResourceManager.GetInstance();
         foreach (var worker in workers) {
            resourceManager.ReturnWorker(worker);
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
         ResourceManager.GetInstance().OffsetMaxPopulation(workers.Count);
      }
   }
}