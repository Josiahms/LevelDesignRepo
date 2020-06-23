using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Placeable))]
public class Housing : MonoBehaviour, IPlaceable, ISaveable {

   [SerializeField]
   private NightActivator nightLight;
   
   private List<Worker> workers = new List<Worker>();

   [SerializeField]
   private int capacity = 5;
   public int Capacity { get { return capacity; }}
   public int AvailableCapacity { get { return capacity - workers.Count;  } }

   private void Start() {
      if (!GetComponent<Placeable>().IsPlaced() && nightLight != null) {
         nightLight.ForceOff = true;
      }
   }

   public void OnPlace() {
      PopulationManager.GetInstance().AddHouse(this);
      workers = PopulationManager.GetInstance().GetHomelessWorkers(capacity);
      foreach (var worker in workers) {
         worker.House = this;
      }
      if (nightLight != null) {
         nightLight.ForceOff = false;
      }
   }

   public void OnRemove() {
      PopulationManager.GetInstance().RemoveHouse(this);
      foreach (var worker in workers) {
         var newHome = PopulationManager.GetInstance().GetAvailableHome();
         if (newHome != null) {
            newHome.workers.Add(worker);
            worker.House = newHome;
         }
      }
      workers.Clear();
   }

   public void RemoveWorker(Worker worker) {
      worker.House = null;
      workers.Remove(worker);
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