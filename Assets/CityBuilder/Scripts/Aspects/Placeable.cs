using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IPlaceable {
   void OnPlace();
   void OnRemove();
}

public class PlaceableEvent : UnityEvent<Placeable> { }

[RequireComponent(typeof(Collider))]
public class Placeable : MonoBehaviour, ISaveable, IDestructable {

   public PlaceableEvent OnPlaceEvent = new PlaceableEvent();
   public PlaceableEvent OnRemoveEvent = new PlaceableEvent();

   [SerializeField]
   protected int foodCost = 0;
   public int GetFoodCost() { return foodCost; }

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

   [SerializeField]
   private bool instantBuild;
   public bool GetInstantBuild() { return instantBuild; }

   [SerializeField]
   private bool deleteable = true;
   public bool Deleteable { get { return deleteable;  } }

   private bool createdFromSave;

   private int blocked = 0;

   private void Start() {
      if (IsPlaced() && !createdFromSave) {
         foreach (var placeable in GetComponents<IPlaceable>()) {
            placeable.OnPlace();
         }
      }

      if (!IsPlaced()) {
         var rb = gameObject.GetComponent<Rigidbody>();
         // TODO: Not having an RB breaks collision detection when placing the object.  Maybe this should be moved to a separate collision detection object
         // rather than added directly to this game object
         /*if (rb == null) {
            gameObject.AddComponent<Rigidbody>();
         }
         rb.useGravity = false;
         rb.isKinematic = false;
         rb.constraints = RigidbodyConstraints.FreezeAll;*/
      }
   }

   public bool IsBlocked() {
      return !isPlaced && blocked > 0;
   }

   public void Place() {
      isPlaced = true;
      foreach (var placeable in GetComponents<IPlaceable>()) {
         placeable.OnPlace();
      }
      OnPlaceEvent.Invoke(this);
      if (GetComponent<Destructable>() != null) {
         GetComponent<Destructable>().enabled = true;
      }
      // TODO: 
      /*if (GetComponent<Rigidbody>() != null) {
         Destroy(GetComponent<Rigidbody>());
      }*/
   }

   public void Remove() {
      if (isPlaced) {
         ResourceManager.GetInstance().OffsetAll(woodCost, stoneCost, metalCost, foodCost);
         GetComponents<IPlaceable>().ToList().ForEach(x => x.OnRemove());
         OnRemoveEvent.Invoke(this);
      }
      Destroy(gameObject);
   }

   public void OnDestruction() {
      if (isPlaced) {
         GetComponents<IPlaceable>().ToList().ForEach(x => x.OnRemove());
         OnRemoveEvent.Invoke(this);
      }
      Destroy(gameObject);
   }

   private void OnCollisionEnter(Collision collision) {
      if (collision.collider.gameObject.layer != LayerMask.NameToLayer("Ground")) {
         if (!isPlaced) {
            blocked++;
         }
      }
   }

   private void OnCollisionExit(Collision collision) {
      if (collision.collider.gameObject.layer != LayerMask.NameToLayer("Ground")) {
         if (!isPlaced) {
            blocked--;
         }
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("isPlaced", isPlaced);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      isPlaced = (bool)data["isPlaced"];
      createdFromSave = true;
   }

   public void OnLoadDependencies(object savedData) {
      // Ignored
   }
}
