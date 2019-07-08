using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Assignable : MonoBehaviour, ISaveable {

   [SerializeField]
   private int maxAssignees = 5;
   public int GetMaxAssignees() { return maxAssignees; }

   private List<WorkerAI> workers = new List<WorkerAI>();

   public void AddWorker() {
      var resourceManager = ResourceManager.GetInstance();
      WorkerAI worker = null;
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

   public int GetWorkerCount() {
      return workers.Count;
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("workers", workers.Select(x => new SavedGameObject(x.GetComponent<Saveable>())).ToArray());
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("workers", out result)) {
         workers = ((SavedGameObject[])result).Select(savedEntity => {
            var prefab = (GameObject)Resources.Load(savedEntity.prefabPath);
            var instance = Instantiate(prefab, new Vector3(savedEntity.position[0], savedEntity.position[1], savedEntity.position[2]), new Quaternion());
            var loadedComponents = instance.GetComponents<ISaveable>();
            for (int i = 0; i < savedEntity.components.Length && i < loadedComponents.Length; i++) {
               var savedComponent = savedEntity.components[i];
               loadedComponents[i].OnLoad(savedComponent.data);
            }
            return instance.GetComponent<WorkerAI>();
         }).ToList();
         ResourceManager.GetInstance().OffsetMaxPopulation(workers.Count);
      }
   }
}