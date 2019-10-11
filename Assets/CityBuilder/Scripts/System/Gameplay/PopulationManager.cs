﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulationManager : Singleton<PopulationManager>, ISaveable {
   private int starvingPeople = 0;
   private int housingCapacity;

   private List<Worker> workers = new List<Worker>();
   private List<Worker> IdleWorkers { get { return workers.Where(x => !x.IsAssigned()).ToList(); } }

   [SerializeField]
   private Text starvingPeopleText;
   [SerializeField]
   private Text populationText;
   [SerializeField]
   private Text idleWorkersText;

   private new void Awake() {
      base.Awake();
      DontDestroyOnLoad(gameObject);
   }

   private void Update() {
      populationText.text = workers.Count + "/" + housingCapacity;
      idleWorkersText.text = IdleWorkers.Count.ToString();
      starvingPeopleText.text = starvingPeople.ToString();
   }

   public bool EatMeal() {
      var food = ResourceManager.GetInstance()[ResourceType.Food];
      if (food.Amount - starvingPeople < 0) {
         Debug.Log("Game Over!");
         return false;
      }
      var previousFoodAmount = food.Amount;
      var previousStarvingPeopleAmount = starvingPeople;

      food.OffsetValue(-starvingPeople);
      starvingPeople = 0;
      if (food.Amount - workers.Count >= 0) {
         food.OffsetValue(-workers.Count);
      } else {
         starvingPeople += (workers.Count - food.Amount);
         food.OffsetValue(-food.Amount);
      }

      var deltaFood = food.Amount - previousFoodAmount;
      var deltaStarving = starvingPeople - previousStarvingPeopleAmount;
      if (deltaStarving != 0) {
         FloatingText.Instantiate(starvingPeopleText.transform, deltaStarving, "Starving", false, deltaStarving < 0, 0.7f);
      }

      return true;
   }

   public Worker GetNearestWorker(Vector3 destination) {
      if (IdleWorkers.Count == 0) {
         return null;
      }
      var worker = IdleWorkers
            .OrderBy(x => Vector3.Distance(x.transform.position, destination))
            .OrderBy(x => x.House == null ? float.MaxValue : Vector3.Distance(x.House.transform.position, destination))
            .First();
      return worker;
   }

   public void AddToWorkforce(Worker worker) {
      workers.Add(worker);
   }

   public void RemoveFromWorkforce(Worker worker) {
      workers.Remove(worker);
   }

   public void AddHouse(House house) {
      housingCapacity += house.Capacity;
   }

   public void RemoveHouse(House house) {
      housingCapacity -= house.Capacity;
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("starvingPeople", starvingPeople);
      data.Add("workers", workers.Select(x => x.GetComponent<Saveable>().GetSavedIndex()).ToArray());
      data.Add("housingCapacity", housingCapacity);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      starvingPeople = (int)data["starvingPeople"];
      housingCapacity = (int)data["housingCapacity"];
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      workers = ((int[])data["workers"])
         .Select(saveIndex => SaveManager.GetInstance().FindLoadedInstanceBySaveIndex(saveIndex).GetComponent<Worker>())
         .ToList();
   }

}
