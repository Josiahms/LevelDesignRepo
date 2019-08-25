using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum ResourceType { Wood, Stone, Metal, Food }

public class GatherResourceEvent : UnityEvent<ResourceType, int> { }

[System.Serializable]
public class Resource {

   public static GatherResourceEvent OnChangeEvent = new GatherResourceEvent();

   public int Amount { get; private set; }
   public int Capacity { get; private set; }

   private ResourceType type;

   public Resource(ResourceType type, int initialValue = 0, int initialCapacity = 0) {
      this.type = type;
      OffsetCapacity(initialCapacity);
      OffsetValue(initialValue);
   }

   public bool IsFull() {
      return Amount >= Capacity;
   }

   public bool CanAfford(int amount) {
      return Amount + amount >= 0;
   }

   public int OffsetValue(int amount) {
      if (Amount + amount < 0) {
         return -1;
      }

      var amountAdded = amount;
      if (Amount + amount > Capacity) {
         amountAdded = Capacity - Amount;
         Amount = Capacity;
      } else {
         Amount += amount;
      }

      if (amountAdded < 0) {
         FloatingText.Instantiate(ResourceManager.GetInstance().GetText(type).transform, amountAdded, type.ToString(), false, false, 0.7f);
      }
      ResourceManager.GetInstance().GetText(type).text = Amount + "/" + Capacity;
      OnChangeEvent.Invoke(type, amountAdded);
      return amountAdded;
   }

   public bool OffsetCapacity(int amount) {
      if (Capacity + amount < 0) {
         return false;
      }

      Capacity += amount;

      if (Amount > Capacity) {
         Amount = Capacity;
      }
      ResourceManager.GetInstance().GetText(type).text = Amount + "/" + Capacity;
      return true;
   }
}

public class ResourceManager : Singleton<ResourceManager>, ISaveable, IAfterLoadCallback {

   [SerializeField]
   private Text woodText;
   [SerializeField]
   private Text stoneText;
   [SerializeField]
   private Text metalText;
   [SerializeField]
   private Text foodText;

   private Dictionary<ResourceType, Resource> resources;

   private new void Awake() {
      base.Awake();
      resources = new Dictionary<ResourceType, Resource>();
      resources.Add(ResourceType.Wood, new Resource(ResourceType.Wood));
      resources.Add(ResourceType.Stone, new Resource(ResourceType.Stone));
      resources.Add(ResourceType.Metal, new Resource(ResourceType.Metal));
      resources.Add(ResourceType.Food, new Resource(ResourceType.Food));
   }

   public Text GetText(ResourceType type) {
      switch(type) {
         case ResourceType.Wood:
            return woodText;
         case ResourceType.Stone:
            return stoneText;
         case ResourceType.Metal:
            return metalText;
         case ResourceType.Food:
            return foodText;
      }
      return null;
   }

   public Resource this[ResourceType i] {
      get {
         Resource result;
         resources.TryGetValue(i, out result);
         return result;
      }
   }

   public bool CanAfford(int woodOffset, int stoneOffset, int metalOffset, int foodOffset) {
      return resources[ResourceType.Wood].CanAfford(woodOffset) &&
         resources[ResourceType.Stone].CanAfford(stoneOffset) &&
         resources[ResourceType.Metal].CanAfford(metalOffset) &&
         resources[ResourceType.Food].CanAfford(foodOffset);
   }

   public bool OffsetAll(int woodOffset, int stoneOffset, int metalOffset, int foodOffset) {
      if (CanAfford(woodOffset, stoneOffset, metalOffset, foodOffset)) {
         resources[ResourceType.Wood].OffsetValue(woodOffset);
         resources[ResourceType.Stone].OffsetValue(stoneOffset);
         resources[ResourceType.Metal].OffsetValue(metalOffset);
         resources[ResourceType.Food].OffsetValue(foodOffset);
         return true;
      }
      return false;
   }

   public void AfterLoad() {
      OffsetAll(0, 0, 0, 0);
   }
}
