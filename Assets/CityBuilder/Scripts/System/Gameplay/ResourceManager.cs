using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum ResourceType { Wood, Stone, Metal, Food }

public class GatherResourceEvent : UnityEvent<ResourceType, int> { }

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
      var savedResources = new Dictionary<ResourceType, object>();
      foreach(var resource in resources) {
         savedResources.Add(resource.Key, resource.Value.OnSave());
      }
      data.Add("resources", savedResources);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      var savedResources = (Dictionary<ResourceType, object>)data["resources"];
      foreach(var resource in savedResources) {
         resources[resource.Key].OnLoad(resource.Value);
      }
   }

   public void OnLoadDependencies(object savedData) {
      // Ignored
   }
}

public class Resource : ISaveable {

   public static GatherResourceEvent OnChangeEvent = new GatherResourceEvent();

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
      display.text = Amount + "/" + Capacity;
      return true;
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("amount", Amount);
      data.Add("capacity", Capacity);
      data.Add("type", type);
      return data;
   }

   public void OnLoad(object data) {
      var savedData = (Dictionary<string, object>)data;
      Amount = (int)savedData["amount"];
      Capacity = (int)savedData["capacity"];
      type = (ResourceType)savedData["type"];
      display.text = Amount + "/" + Capacity;
   }

   public void OnLoadDependencies(object data) {
      // Ignored
   }
}
