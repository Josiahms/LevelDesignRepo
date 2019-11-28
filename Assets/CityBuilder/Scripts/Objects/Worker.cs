﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Walker))]
public class Worker : MonoBehaviour, ISaveable {

   private Vector3? previousLocation;
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
      DayCycleManager.GetInstance().OnStartWorkDay.AddListener(() => {
         if (assignedLocation != null) {
            SetDestination(assignedLocation);
         } else {
            SetDestination(previousLocation.GetValueOrDefault());
         }
      });

      DayCycleManager.GetInstance().OnEndWorkDay.AddListener(() => {
         previousLocation = GetComponent<Walker>().GetDestination();
         GetComponent<Walker>().SetDestination(House != null ? House.transform.position : WorkerSpawner.GetInstance().transform.position);
      });

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

   public bool SetDestination(Assignable assignment) {
      if (assignment == null) {
         return false;
      }

      if (assignedLocation == assignment) {
         return true;
      }

      if (assignment.AddWorker(this)) {
         assignedLocation = assignment;
         if (assignment.GetSpotForWorker(this) != null) {
            GetComponent<Walker>().SetDestination(assignment.GetSpotForWorker(this).position);
         } else {
            GetComponent<Walker>().SetDestination(assignment.transform.position);
         }
         return true;
      }
      return false;
   }

   public void SetDestination(Vector3 destination) {
      if (assignedLocation != null) {
         assignedLocation.RemoveWorker(this);
         assignedLocation = null;
      }
      GetComponent<Walker>().SetDestination(destination);
   }

   public bool IsAssigned() {
      return assignedLocation != null;
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
