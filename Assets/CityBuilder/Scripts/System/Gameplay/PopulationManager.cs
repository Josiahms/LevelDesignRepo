using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulationManager : Singleton<PopulationManager>, ISaveable {
   private int starvingPeople = 0;
   private int populationSize;
   private int housingCapacity;

   private List<Worker> idleWorkers = new List<Worker>();

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
      if (food.Amount - populationSize >= 0) {
         food.OffsetValue(-populationSize);
         starvingPeopleText.text = starvingPeople.ToString();
      } else {
         starvingPeople += (populationSize - food.Amount);
         food.OffsetValue(-food.Amount);
         starvingPeopleText.text = starvingPeople.ToString();
      }

      var deltaFood = food.Amount - previousFoodAmount;
      var deltaStarving = starvingPeople - previousStarvingPeopleAmount;
      if (deltaStarving != 0) {
         FloatingText.Instantiate(starvingPeopleText.transform, deltaStarving, "Starving", false, deltaStarving < 0, 0.7f);
      }

      return true;
   }

   public Worker PopNearestWorker(Vector3 destination) {
      if (idleWorkers.Count == 0) {
         return null;
      }
      var worker = idleWorkers
            .OrderBy(x => Vector3.Distance(x.transform.position, destination))
            .OrderBy(x => x.House == null ? float.MaxValue : Vector3.Distance(x.House.transform.position, destination))
            .First();
      if (idleWorkers.Remove(worker)) {
         idleWorkersText.text = idleWorkers.Count.ToString();
         return worker;
      }
      return null;
   }

   public void PushWorker(Worker worker) {
      idleWorkers.Add(worker);
      idleWorkersText.text = idleWorkers.Count.ToString();
   }

   public void AddToWorkforce(Worker worker) {
      idleWorkers.Add(worker);
      populationSize++;
      populationText.text = populationSize + "/" + housingCapacity;
      idleWorkersText.text = idleWorkers.Count.ToString();
   }

   public void RemoveFromWorkforce(Worker worker) {
      idleWorkers.Remove(worker);
      populationSize--;
      populationText.text = populationSize + "/" + housingCapacity;
      idleWorkersText.text = idleWorkers.Count.ToString();
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("starvingPeople", starvingPeople);
      data.Add("workers", idleWorkers.Select(x => x.GetComponent<Saveable>().GetSavedIndex()).ToArray());
      data.Add("populationSize", populationSize);
      data.Add("housingCapacity", housingCapacity);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      starvingPeople = (int)data["starvingPeople"];
      starvingPeopleText.text = starvingPeople.ToString();
      populationSize = (int)data["populationSize"];
      housingCapacity = (int)data["housingCapacity"];
      populationText.text = populationSize + "/" + housingCapacity;
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      idleWorkers = ((int[])data["workers"])
         .Select(saveIndex => SaveManager.GetInstance().FindLoadedInstanceBySaveIndex(saveIndex).GetComponent<Worker>())
         .ToList();
      idleWorkersText.text = idleWorkers.Count.ToString();
   }

}
