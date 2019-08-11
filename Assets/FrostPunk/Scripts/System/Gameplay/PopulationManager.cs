using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulationManager : Singleton<PopulationManager>, ISaveable {
   private int starvingPeopleAmount = 0;
   private int maxPopulation;

   private List<Worker> workers = new List<Worker>();

   [SerializeField]
   private Text starvingPeopleText;
   [SerializeField]
   private Text populationText;

   private new void Awake() {
      base.Awake();
      DontDestroyOnLoad(gameObject);
   }

   public bool EatMeal() {
      var FoodAmount = ResourceManager.GetInstance()[ResourceType.Food].Amount;
      if (FoodAmount - starvingPeopleAmount < 0) {
         Debug.Log("Game Over!");
         return false;
      }
      var previousFoodAmount = FoodAmount;
      var previousStarvingPeopleAmount = starvingPeopleAmount;

      FoodAmount -= starvingPeopleAmount;
      starvingPeopleAmount = 0;
      if (FoodAmount - maxPopulation >= 0) {
         FoodAmount -= maxPopulation;
         starvingPeopleText.text = starvingPeopleAmount.ToString();
      } else {
         starvingPeopleAmount += (maxPopulation - FoodAmount);
         FoodAmount = 0;
         starvingPeopleText.text = starvingPeopleAmount.ToString();
      }

      var deltaFood = FoodAmount - previousFoodAmount;
      var deltaStarving = starvingPeopleAmount - previousStarvingPeopleAmount;
      if (deltaStarving != 0) {
         FloatingText.Instantiate(starvingPeopleText.transform, deltaStarving, "Starving", false, deltaStarving < 0, 0.7f);
      }

      return true;
   }

   public Worker PopNearestWorker(Vector3 destination) {
      if (workers.Count == 0) {
         return null;
      }
      var worker = workers.OrderBy(x => Vector3.Distance(x.transform.position, destination))
            .OrderBy(x => Vector3.Distance(x.House.transform.position, destination))
            .First();
      if (workers.Remove(worker)) {
         populationText.text = workers.Count + "/" + maxPopulation;
         return worker;
      }
      return null;
   }

   public void PushWorker(Worker worker) {
      workers.Add(worker);
      populationText.text = workers.Count + "/" + maxPopulation;
   }

   public void AddToWorkforce(Worker worker) {
      workers.Add(worker);
      maxPopulation++;
      populationText.text = workers.Count + "/" + maxPopulation;
   }

   public void RemoveFromWorkforce(Worker worker) {
      workers.Remove(worker);
      maxPopulation--;
      populationText.text = workers.Count + "/" + maxPopulation;
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("starvingPeopleAmount", starvingPeopleAmount);
      data.Add("workers", workers.Select(x => x.GetComponent<Saveable>().GetSavedIndex()).ToArray());
      data.Add("maxPopulation", maxPopulation);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("starvingPeopleAmount", out result)) {
         starvingPeopleAmount = (int)result;
         starvingPeopleText.text = starvingPeopleAmount.ToString();
      }
      if (data.TryGetValue("maxPopulation", out result)) {
         maxPopulation = (int)result;
      }
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("workers", out result)) {
         workers = ((int[])result).Select(saveIndex => SaveManager.GetInstance().FindLoadedInstanceBySaveIndex(saveIndex).GetComponent<Worker>()).ToList();
      }
      populationText.text = workers.Count + "/" + maxPopulation;
   }

}
