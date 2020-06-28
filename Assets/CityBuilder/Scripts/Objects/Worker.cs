using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Targetable))]
[RequireComponent(typeof(Walker))]
public class Worker : MonoBehaviour, ISaveable {

   private Vector3? previousLocation;
   private bool createdFromScene = true;

   public Housing House { get; set; }

   public static Worker Instantiate(Housing house, Vector3 position, Quaternion rotation) {
      var result = Instantiate(ResourceLoader.GetInstance().WorkerPrefab, position + Vector3.Scale(Random.insideUnitSphere, new Vector3(3f, 0, 3f)), rotation);
      result.House = house;
      result.createdFromScene = false;
      PopulationManager.GetInstance().AddToWorkforce(result);
      return result;
   }

   private void Start() {
      DayCycleManager.GetInstance().OnStartWorkDay.AddListener(() => {
         GetComponent<Walker>().SetDestination(previousLocation.GetValueOrDefault());
      });

      DayCycleManager.GetInstance().OnEndWorkDay.AddListener(() => {
         previousLocation = GetComponent<Walker>().GetDestination();
         GetComponent<Walker>().SetDestination(House != null ? House.transform.position : WorkerSpawner.GetInstance().transform.position);
      });

      if (createdFromScene) {
         PopulationManager.GetInstance().AddToWorkforce(this);
      }
   }

   private void Update() {
      SetWalkerDestination(GetComponent<Targeter>().targetLocation);
   }

   private void OnDestroy() {
      if (House != null) {
         House.RemoveWorker(this);
      }
      if (ResourceManager.GetInstance() != null) {
         PopulationManager.GetInstance().RemoveFromWorkforce(this);
      }
   }

   private void SetWalkerDestination(Vector3? destination) {
      if (DayCycleManager.GetInstance().IsRestTime()) {
         previousLocation = destination;
      } else {
         GetComponent<Walker>().SetDestination(destination, 0.7f);
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("previousLocation", previousLocation.HasValue ? new float[] { previousLocation.Value.x, previousLocation.Value.y, previousLocation.Value.z } : null);
      data.Add("house", House == null ? null : (int?)House.GetComponent<Saveable>().GetSavedIndex());
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      createdFromScene = false;
      var prevLoc = (float[])data["previousLocation"];
      if (prevLoc != null) {
         previousLocation = new Vector3(prevLoc[0], prevLoc[1], prevLoc[2]);
      }
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      var houseIndex = (int?)data["house"];
      House = houseIndex.HasValue ? SaveManager.GetInstance().FindLoadedInstanceBySaveIndex(houseIndex.Value).GetComponent<Housing>() : null;
   }
}
