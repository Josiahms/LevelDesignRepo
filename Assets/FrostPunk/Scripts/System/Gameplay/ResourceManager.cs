using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum ResourceType { Wood, Stone, Metal, Food }

public class Resource {
   public int Amount { get; private set; }
   public int Capacity { get; private set; }

   private ResourceType type;
   private Text display;

   public Resource(ResourceType type, Text display, int initialValue = 0, int initialCapacity = 0) {
      this.type = type;
      this.display = display;
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
         FloatingText.Instantiate(display.transform, amountAdded, type.ToString(), false, false, 0.7f);
      }
      display.text = Amount + "/" + Capacity;
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
      display.text = Amount + "/" + Capacity;
      return true;
   }
}

public class ResourceManager : Singleton<ResourceManager>, ISaveable {

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
      resources.Add(ResourceType.Wood, new Resource(ResourceType.Wood, woodText));
      resources.Add(ResourceType.Stone, new Resource(ResourceType.Stone, stoneText));
      resources.Add(ResourceType.Metal, new Resource(ResourceType.Metal, metalText));
      resources.Add(ResourceType.Food, new Resource(ResourceType.Food, foodText));
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
         return true;
      }
      return false;
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("woodAmount", resources[ResourceType.Wood].Amount);
      data.Add("stoneAmount", resources[ResourceType.Stone].Amount);
      data.Add("metalAmount", resources[ResourceType.Metal].Amount);
      data.Add("foodAmount", resources[ResourceType.Food].Amount);
      data.Add("woodCapacity", resources[ResourceType.Wood].Capacity);
      data.Add("stoneCapacity", resources[ResourceType.Stone].Capacity);
      data.Add("metalCapacity", resources[ResourceType.Metal].Capacity);
      data.Add("foodCapacity", resources[ResourceType.Food].Capacity);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("woodCapacity", out result)) {
         resources[ResourceType.Wood].OffsetCapacity((int)result);
      }
      if (data.TryGetValue("woodAmount", out result)) {
         resources[ResourceType.Wood].OffsetValue((int)result);
      }
      if (data.TryGetValue("stoneCapacity", out result)) {
         resources[ResourceType.Stone].OffsetCapacity((int)result);
      }
      if (data.TryGetValue("stoneAmount", out result)) {
         resources[ResourceType.Stone].OffsetValue((int)result);
      }
      if (data.TryGetValue("metalCapacity", out result)) {
         resources[ResourceType.Metal].OffsetCapacity((int)result);
      }
      if (data.TryGetValue("metalAmount", out result)) {
         resources[ResourceType.Metal].OffsetValue((int)result);
      }
      if (data.TryGetValue("foodCapacity", out result)) {
         resources[ResourceType.Food].OffsetCapacity((int)result);
      }
      if (data.TryGetValue("foodAmount", out result)) {
         resources[ResourceType.Food].OffsetValue((int)result);
      }
   }

   public void OnLoadDependencies(object savedData) {
      // Ignored
   }
}
