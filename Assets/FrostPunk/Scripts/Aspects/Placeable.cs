using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IPlaceable {
   void OnPlace();
   void OnRemove();
}

public class PlaceableEvent : UnityEvent<Placeable> { }

public class Placeable : MonoBehaviour, ISaveable {

   public static PlaceableEvent OnPlaceEvent = new PlaceableEvent();
   public static PlaceableEvent OnRemoveEvent = new PlaceableEvent();

   [SerializeField]
   protected int woodCost = 0;
   public int GetWoodCost() { return woodCost; }

   [SerializeField]
   protected int stoneCost = 0;
   public int GetStoneCost() { return stoneCost; }

   [SerializeField]
   protected int metalCost = 0;
   public int GetMetalCost() { return metalCost; }

   [SerializeField]
   private bool isPlaced;
   public bool IsPlaced() { return isPlaced; }

   private bool isLoaded;

   private void Start() {
      if (GetComponent<Placeable>().IsPlaced() && !isLoaded) {
         foreach (var placeable in GetComponents<IPlaceable>()) {
            placeable.OnPlace();
         }
      }
   }

   public bool Place() {
      if (ResourceManager.GetInstance().OffsetAll(-woodCost, -stoneCost, -metalCost, 0)) {
         isPlaced = true;
         foreach (var placeable in GetComponents<IPlaceable>()) {
            placeable.OnPlace();
         }
         OnPlaceEvent.Invoke(this);
         return true;
      }
      return false;
   }

   public void Remove() {
      if (isPlaced) {
         ResourceManager.GetInstance().OffsetAll(woodCost, stoneCost, metalCost, 0);
         GetComponents<IPlaceable>().ToList().ForEach(x => x.OnRemove());
         OnRemoveEvent.Invoke(this);
      }
      Destroy(gameObject);
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("isPlaced", isPlaced);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("isPlaced", out result)) {
         isPlaced = (bool)result;
      }
      isLoaded = true;
   }

   public void OnLoadDependencies(object savedData) {
      // Ignored
   }
}
