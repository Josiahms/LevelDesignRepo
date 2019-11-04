using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Walker))]
public class Worker : MonoBehaviour, ISaveable {   
   private Transform currentDestination;
   private Assignable assignedLocation;
   private bool createdFromScene = true;

   public House House { get; set; }

   public static Worker Instantiate(House house, Vector3 position, Quaternion rotation) {
      var result = Instantiate(ResourceLoader.GetInstance().WorkerPrefab, position + Vector3.Scale(Random.insideUnitSphere, new Vector3(3f, 0, 3f)), rotation);
      result.House = house;
      result.createdFromScene = false;
      PopulationManager.GetInstance().AddToWorkforce(result);
      return result;
   }

   private void Start() {
      if (createdFromScene) {
         PopulationManager.GetInstance().AddToWorkforce(this);
      }
   }

   private void OnDestroy() {
      if (House != null) {
         House.RemoveWorker(this);
      }
      if (ResourceManager.GetInstance() != null) {
         PopulationManager.GetInstance().RemoveFromWorkforce(this);
      }
      if (assignedLocation != null) {
         assignedLocation.RemoveWorker(this);
      }
   }

   public void SetDestination(Assignable destination) {
      assignedLocation = destination;
   }

   public bool IsAssigned() {
      return assignedLocation != null;
   }

   private void Update() {
      if (DayCycleManager.GetInstance().IsRestTime()) {
         currentDestination = House == null ? DefaultGatheringPoint.GetInstance().transform : House.transform;
      } else {
         currentDestination = assignedLocation == null ? null : assignedLocation.GetSpotForWorker(this);
      }
      GetComponent<Walker>().SetDestination(currentDestination == null ? null : (Vector3?)currentDestination.position);
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("destination", assignedLocation != null ? assignedLocation.GetComponent<Saveable>().GetSavedIndex() : -1);
      data.Add("house", House == null ? null : (int?)House.GetComponent<Saveable>().GetSavedIndex());
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      createdFromScene = false;
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("house", out result)) {
         var houseIndex = (int?)result;
         if (houseIndex.HasValue) {
            House = SaveManager.GetInstance().FindLoadedInstanceBySaveIndex(houseIndex.Value).GetComponent<House>();
         } else {
            House = null;
         }
      }
      if (data.TryGetValue("destination", out result)) {
         assignedLocation = (int)result == -1 ? null : SaveManager.GetInstance().FindLoadedInstanceBySaveIndex((int)result).GetComponent<Assignable>();
      }
   }
}
