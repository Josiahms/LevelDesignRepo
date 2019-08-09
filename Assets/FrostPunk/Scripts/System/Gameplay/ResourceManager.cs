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

   public int WoodCapacity { get; private set; }
   public int StoneCapacity { get; private set; }
   public int MetalCapacity { get; private set; }
   public int FoodCapacity { get; private set; }

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
      woodText.text = WoodAmount + "/" + WoodCapacity;
      stoneText.text = StoneAmount + "/" + StoneCapacity;
      metalText.text = MetalAmount + "/" + MetalCapacity;
      foodText.text = FoodAmount + "/" + FoodCapacity;
      DontDestroyOnLoad(gameObject);
   }

   public bool IsFull(ResourceType type) {
      switch (type) {
         case ResourceType.Wood:
            return WoodAmount >= WoodCapacity;
         case ResourceType.Stone:
            return StoneAmount >= StoneCapacity;
         case ResourceType.Metal:
            return MetalAmount >= MetalCapacity;
         case ResourceType.Food:
            return FoodAmount >= FoodCapacity;
      }
      return true;
   }

   public bool CanAfford(int woodCost, int stoneCost, int metalCost, int foodCost = 0) {
      return WoodAmount - woodCost >= 0 && StoneAmount - stoneCost >= 0 && MetalAmount - metalCost >= 0 && FoodAmount - foodCost >= 0;
   }

   public int AddResource(ResourceType type, int amount) {
      if (amount < 0) {
         return 0;
      }
      var amountAdded = 0;
      switch (type) {
         case ResourceType.Wood:
            amountAdded = Mathf.Min(amount, WoodCapacity - WoodAmount);
            OffsetMaterials(amount, 0, 0);
            break;
         case ResourceType.Stone:
            amountAdded = Mathf.Min(amount, StoneCapacity - StoneAmount);
            OffsetMaterials(0, amount, 0);
            break;
         case ResourceType.Metal:
            amountAdded = Mathf.Min(amount, MetalCapacity - MetalAmount);
            OffsetMaterials(0, 0, amount);
            break;
         case ResourceType.Food:
            amountAdded = Mathf.Min(amount, FoodCapacity - FoodAmount);
            FoodAmount += amount;
            FoodAmount = Mathf.Min(FoodAmount, FoodCapacity);
            foodText.text = FoodAmount + "/" + FoodCapacity;
            break;
      }
      return amountAdded;
   }

   public bool OffsetCapacities(int woodCapacityOffset, int stoneCapacityOffset, int metalCapacityOffset, int foodCapacityOffset) {
      if (WoodCapacity + woodCapacityOffset >= 0 && 
         StoneCapacity + stoneCapacityOffset >= 0 && 
         MetalCapacity + metalCapacityOffset >= 0 &&  
         FoodCapacity + foodCapacityOffset >= 0) {

         WoodCapacity += woodCapacityOffset;
         StoneCapacity += stoneCapacityOffset;
         MetalCapacity += metalCapacityOffset;
         FoodCapacity += foodCapacityOffset;

         WoodAmount = Mathf.Min(WoodAmount, WoodCapacity);
         StoneAmount = Mathf.Min(StoneAmount, StoneCapacity);
         MetalAmount = Mathf.Min(MetalAmount, MetalCapacity);
         FoodAmount = Mathf.Min(FoodAmount, FoodCapacity);

         woodText.text = WoodAmount + "/" + WoodCapacity;
         stoneText.text = StoneAmount + "/" + StoneCapacity;
         metalText.text = MetalAmount + "/" + MetalCapacity;
         foodText.text = FoodAmount + "/" + FoodCapacity;

         return true;
      }
      return false;
   }

   public bool OffsetMaterials(int woodOffset, int stoneOffset, int metalOffset) {
      if (CanAfford(-woodOffset, -stoneOffset, -metalOffset)) {
         WoodAmount += woodOffset;
         StoneAmount += stoneOffset;
         MetalAmount += metalOffset;

         WoodAmount = Mathf.Min(WoodAmount, WoodCapacity);
         StoneAmount = Mathf.Min(StoneAmount, StoneCapacity);
         MetalAmount = Mathf.Min(MetalAmount, MetalCapacity);

         woodText.text = WoodAmount + "/" + WoodCapacity;
         stoneText.text = StoneAmount + "/" + StoneCapacity;
         metalText.text = MetalAmount + "/" + MetalCapacity;
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
         foodText.text = FoodAmount + "/" + FoodCapacity;
         starvingPeopleText.text = starvingPeopleAmount.ToString();
      } else {
         starvingPeopleAmount += (maxPopulation - FoodAmount);
         FoodAmount = 0;
         foodText.text = FoodAmount + "/" + FoodCapacity;
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
      data.Add("woodAmount", WoodAmount);
      data.Add("stoneAmount", StoneAmount);
      data.Add("metalAmount", MetalAmount);
      data.Add("woodCapacity", WoodCapacity);
      data.Add("stoneCapacity", StoneCapacity);
      data.Add("metalCapacity", MetalCapacity);
      data.Add("foodAmount", FoodAmount);
      data.Add("foodCapacity", FoodCapacity);
      data.Add("starvingPeopleAmount", starvingPeopleAmount);
      data.Add("workers", workers.Select(x => x.GetComponent<Saveable>().GetSavedIndex()).ToArray());
      data.Add("maxPopulation", maxPopulation);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("woodCapacity", out result)) {
         WoodCapacity = (int)result;
         woodText.text = WoodAmount + "/" + WoodCapacity;
      }
      if (data.TryGetValue("woodAmount", out result)) {
         WoodAmount = (int)result;
         woodText.text = WoodAmount + "/" + WoodCapacity;
      }
      if (data.TryGetValue("stoneCapacity", out result)) {
         StoneCapacity = (int)result;
         stoneText.text = StoneAmount + "/" + StoneCapacity;
      }
      if (data.TryGetValue("stoneAmount", out result)) {
         StoneAmount = (int)result;
         stoneText.text = StoneAmount + "/" + StoneCapacity;
      }
      if (data.TryGetValue("metalCapacity", out result)) {
         MetalCapacity = (int)result;
         metalText.text = MetalAmount + "/" + MetalCapacity ;
      }
      if (data.TryGetValue("metalAmount", out result)) {
         MetalAmount = (int)result;
         metalText.text = MetalAmount + "/" + MetalCapacity;
      }
      if (data.TryGetValue("foodCapacity", out result)) {
         FoodCapacity = (int)result;
         foodText.text = FoodAmount + "/" + FoodCapacity;
      }
      if (data.TryGetValue("foodAmount", out result)) {
         FoodAmount = (int)result;
         foodText.text = FoodAmount + "/" + FoodCapacity;
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
