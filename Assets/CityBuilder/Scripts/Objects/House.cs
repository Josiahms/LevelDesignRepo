using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Placeable))]
public class House : MonoBehaviour, IPlaceable, ISaveable {

   [SerializeField]
   private Transform spawnpoint;
   [SerializeField]
   private List<Worker> workers = new List<Worker>();

   [SerializeField]
   private int capacity = 5;
   public int Capacity { get { return capacity; } }

   public void OnPlace() {
      PopulationManager.GetInstance().AddHouse(this);
   }

   public void OnUpgrade() {
      // Ignored
   }

   public void RemoveWorker(Worker worker) {
      workers.Remove(worker);
   }

   public void OnRemove() {
      PopulationManager.GetInstance().RemoveHouse(this);
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("workers", workers.Select(x => x.GetComponent<Saveable>().GetSavedIndex()).ToArray());
      return data;
   }

   public void OnLoad(object saveData) {
      // Ignored
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("workers", out result)) {
         workers = ((int[])result).Select(workerSavedIndex => {
            return SaveManager.GetInstance().FindLoadedInstanceBySaveIndex(workerSavedIndex).GetComponent<Worker>();
         }).ToList();
      }
   }
}