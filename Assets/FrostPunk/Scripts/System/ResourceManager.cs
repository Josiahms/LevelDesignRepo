using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ResourceType { Wood, Stone, Metal, Food }

public class ResourceManager : Singleton<ResourceManager>, ISaveable {

   private int woodAmount = 5;
   public int WoodAmount { get { return woodAmount; } }
   private int stoneAmount = 0;
   public int StoneAmount { get { return stoneAmount; } }
   private int metalAmount = 0;
   public int MetalAmount { get { return metalAmount; } }
   private int foodAmount = 0;
   public int FoodAmount { get { return foodAmount; } }
   private int starvingPeopleAmount = 0;
   private int population = 0;
   public int Population { get { return population; } }
   private int maxPopulation = 0;
   public int MaxPopulation { get { return maxPopulation; } }
   private List<Worker> workers = new List<Worker>();

   [SerializeField]
   private Text woodText;
   [SerializeField]
   private Text stoneText;
   [SerializeField]
   private Text metalText;
   [SerializeField]
   private Text foodText;
   [SerializeField]
   private Text starvingPeopleText;
   [SerializeField]
   private Text populationText;
   [SerializeField]
   private Worker workerPrefab;

   private new void Awake() {
      base.Awake();
      woodText.text = woodAmount.ToString();
      stoneText.text = stoneAmount.ToString();
      metalText.text = metalAmount.ToString();
      foodText.text = foodAmount.ToString();
      populationText.text = population + "/" + maxPopulation;
      DontDestroyOnLoad(gameObject);
   }

   public bool CanAfford(int woodCost, int stoneCost, int metalCost, int foodCost = 0) {
      return woodAmount - woodCost >= 0 && stoneAmount - stoneCost >= 0 && metalAmount - metalCost >= 0 && foodAmount - foodCost >= 0;
   }

   public void AddResource(ResourceType type, int amount) {
      if (amount < 0) {
         return;
      }
      switch(type) {
         case ResourceType.Wood:
            OffsetMaterials(amount, 0, 0);
            return;
         case ResourceType.Stone:
            OffsetMaterials(0, amount, 0);
            return;
         case ResourceType.Metal:
            OffsetMaterials(0, 0, amount);
            return;
         case ResourceType.Food:
            foodAmount += amount;
            foodText.text = foodAmount.ToString();
            return;
      }
   }

   public bool OffsetMaterials(int woodOffset, int stoneOffset, int metalOffset) {
      if (CanAfford(-woodOffset, -stoneOffset, -metalOffset)) {
         woodAmount += woodOffset;
         stoneAmount += stoneOffset;
         metalAmount += metalOffset;
         woodText.text = woodAmount.ToString();
         stoneText.text = stoneAmount.ToString();
         metalText.text = metalAmount.ToString();
         if (woodOffset < 0) {
            FloatingText.Instantiate(woodText.transform, woodOffset, ResourceType.Wood.ToString(), false, false, 0.7f);
         }
         if (stoneOffset < 0) {
            FloatingText.Instantiate(woodText.transform, stoneOffset, ResourceType.Stone.ToString(), false, false, 0.7f);
         }
         if (metalOffset < 0) {
            FloatingText.Instantiate(woodText.transform, metalOffset, ResourceType.Metal.ToString(), false, false, 0.7f);
         }

         return true;
      }
      return false;
   }

   public bool EatMeal() {
      if (foodAmount - starvingPeopleAmount < 0) {
         Debug.Log("Game Over!");
         return false;
      }
      var previousFoodAmount = foodAmount;
      var previousStarvingPeopleAmount = starvingPeopleAmount;

      foodAmount -= starvingPeopleAmount;
      starvingPeopleAmount = 0;
      if (foodAmount - maxPopulation >= 0) {
         foodAmount -= maxPopulation;
         foodText.text = foodAmount.ToString();
         starvingPeopleText.text = starvingPeopleAmount.ToString();
      } else {
         starvingPeopleAmount += (maxPopulation - foodAmount);
         foodAmount = 0;
         foodText.text = foodAmount.ToString();
         starvingPeopleText.text = starvingPeopleAmount.ToString();
      }

      var deltaFood = foodAmount - previousFoodAmount;
      if (deltaFood < 0) {
         FloatingText.Instantiate(foodText.transform, foodAmount - previousFoodAmount, ResourceType.Food.ToString(), false, false, 0.7f);
      }
      var deltaStarving = starvingPeopleAmount - previousStarvingPeopleAmount;
      if (deltaStarving != 0) {
         FloatingText.Instantiate(starvingPeopleText.transform, deltaStarving, "Starving", false, deltaStarving < 0, 0.7f);
      }

      return true;
   }

   public bool AssignWorker(Transform destination, ref Worker worker) {
      if (OffsetWorkerCount(-1)) {
         worker = workers[0];
         workers.Remove(worker);
         worker.SetDestination(destination.position);
         return true;
      }
      return false;
   }

   public void ReturnWorker(Worker worker) {
      if (OffsetWorkerCount(1)) {
         workers.Add(worker);
         worker.SetDestination(null);
      }
   }

   public void AddToWorkforce(Worker worker) {
      OffsetPopulation(1);
      workers.Add(worker);
   }

   public void AddExistingToWorkforce(Worker worker) {
      OffsetPopulation(1);
      workers.Add(worker);
   }

   private void OffsetPopulation(int offsetAmount) {
      population += offsetAmount;
      maxPopulation += offsetAmount;
      Mathf.Max(0, population);
      Mathf.Max(0, maxPopulation);
      populationText.text = population + "/" + maxPopulation;
   }

   public void OffsetMaxPopulation(int offsetAmount) {
      maxPopulation += offsetAmount;
      maxPopulation = Mathf.Max(population, maxPopulation);
      populationText.text = population + "/" + maxPopulation;
   }

   private bool OffsetWorkerCount(int offsetAmount) {
      if (population + offsetAmount >= 0) {
         population += offsetAmount;
         populationText.text = population + "/" + maxPopulation;
         return true;
      }
      return false;
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("woodAmount", woodAmount);
      data.Add("stoneAmount", stoneAmount);
      data.Add("metalAmount", metalAmount);
      data.Add("foodAmount", foodAmount);
      data.Add("starvingPeopleAmount", starvingPeopleAmount);      
      data.Add("workers", workers.Select(x => x.GetComponent<Saveable>().GetSavedIndex()).ToArray());
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("woodAmount", out result)) {
         woodAmount = (int)result;
         woodText.text = woodAmount.ToString();
      }
      if (data.TryGetValue("stoneAmount", out result)) {
         stoneAmount = (int)result;
         stoneText.text = stoneAmount.ToString();
      }
      if (data.TryGetValue("metalAmount", out result)) {
         metalAmount = (int)result;
         metalText.text = metalAmount.ToString();
      }
      if (data.TryGetValue("foodAmount", out result)) {
         foodAmount = (int)result;
         foodText.text = foodAmount.ToString();
      }
      if (data.TryGetValue("starvingPeopleAmount", out result)) {
         starvingPeopleAmount = (int)result;
         starvingPeopleText.text = starvingPeopleAmount.ToString();
      }
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("workers", out result)) {
         workers = ((int[])result).Select(workerSavedIndex => {
            Debug.Log(SaveManager.GetInstance());
            return SaveManager.GetInstance().FindLoadedInstanceBySaveIndex(workerSavedIndex).GetComponent<Worker>();
         }).ToList();
         OffsetPopulation(workers.Count);
      }
   }
}
