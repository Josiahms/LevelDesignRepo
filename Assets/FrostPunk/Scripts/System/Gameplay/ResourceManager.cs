using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ResourceType { Wood, Stone, Metal, Food }

public class ResourceManager : Singleton<ResourceManager>, ISaveable {

   public int WoodAmount { get; private set; }
   public int StoneAmount { get; private set; }
   public int MetalAmount { get; private set; }
   public int FoodAmount { get; private set; }

   private int starvingPeopleAmount = 0;
   private int maxPopulation;

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

   private new void Awake() {
      base.Awake();
      WoodAmount = 5;
      woodText.text = WoodAmount.ToString();
      stoneText.text = StoneAmount.ToString();
      metalText.text = MetalAmount.ToString();
      foodText.text = FoodAmount.ToString();
      DontDestroyOnLoad(gameObject);
   }

   public bool CanAfford(int woodCost, int stoneCost, int metalCost, int foodCost = 0) {
      return WoodAmount - woodCost >= 0 && StoneAmount - stoneCost >= 0 && MetalAmount - metalCost >= 0 && FoodAmount - foodCost >= 0;
   }

   public void AddResource(ResourceType type, int amount) {
      if (amount < 0) {
         return;
      }
      switch (type) {
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
            FoodAmount += amount;
            foodText.text = FoodAmount.ToString();
            return;
      }
   }

   public bool OffsetMaterials(int woodOffset, int stoneOffset, int metalOffset) {
      if (CanAfford(-woodOffset, -stoneOffset, -metalOffset)) {
         WoodAmount += woodOffset;
         StoneAmount += stoneOffset;
         MetalAmount += metalOffset;
         woodText.text = WoodAmount.ToString();
         stoneText.text = StoneAmount.ToString();
         metalText.text = MetalAmount.ToString();
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
         foodText.text = FoodAmount.ToString();
         starvingPeopleText.text = starvingPeopleAmount.ToString();
      } else {
         starvingPeopleAmount += (maxPopulation - FoodAmount);
         FoodAmount = 0;
         foodText.text = FoodAmount.ToString();
         starvingPeopleText.text = starvingPeopleAmount.ToString();
      }

      var deltaFood = FoodAmount - previousFoodAmount;
      if (deltaFood < 0) {
         FloatingText.Instantiate(foodText.transform, FoodAmount - previousFoodAmount, ResourceType.Food.ToString(), false, false, 0.7f);
      }
      var deltaStarving = starvingPeopleAmount - previousStarvingPeopleAmount;
      if (deltaStarving != 0) {
         FloatingText.Instantiate(starvingPeopleText.transform, deltaStarving, "Starving", false, deltaStarving < 0, 0.7f);
      }

      return true;
   }

   public Worker PopNearestWorker(Vector3 destination) {
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
      data.Add("woodAmount", WoodAmount);
      data.Add("stoneAmount", StoneAmount);
      data.Add("metalAmount", MetalAmount);
      data.Add("foodAmount", FoodAmount);
      data.Add("starvingPeopleAmount", starvingPeopleAmount);
      data.Add("workers", workers.Select(x => x.GetComponent<Saveable>().GetSavedIndex()).ToArray());
      data.Add("maxPopulation", maxPopulation);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("woodAmount", out result)) {
         WoodAmount = (int)result;
         woodText.text = WoodAmount.ToString();
      }
      if (data.TryGetValue("stoneAmount", out result)) {
         StoneAmount = (int)result;
         stoneText.text = StoneAmount.ToString();
      }
      if (data.TryGetValue("metalAmount", out result)) {
         MetalAmount = (int)result;
         metalText.text = MetalAmount.ToString();
      }
      if (data.TryGetValue("foodAmount", out result)) {
         FoodAmount = (int)result;
         foodText.text = FoodAmount.ToString();
      }
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
