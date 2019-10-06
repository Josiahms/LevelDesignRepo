using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IPlaceable {
   void OnPlace();
   void OnRemove();
   void OnUpgrade();
}

public class PlaceableEvent : UnityEvent<Placeable> { }

[RequireComponent(typeof(Collider))]
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

   [SerializeField]
   private bool destructable = true;
   public bool Destructable { get { return destructable; } }

   [SerializeField]
   private bool upgradeable = true;
   public bool Upgradeable { get { return upgradeable; } }

   [SerializeField]
   private int woodUpgradeCost;
   public int WoodUpgradeCost { get { return woodUpgradeCost; } }

   [SerializeField]
   private int stoneUpgradeCost;
   public int StoneUpgradeCost { get { return stoneUpgradeCost; } }

   [SerializeField]
   private int metalUpgradeCost;
   public int MetalUpgradeCost { get { return metalUpgradeCost; } }

   [SerializeField]
   private List<GameObject> levels;

   private int level;
   private bool isLoaded;

   private int blocked = 0;

   private void Start() {
      if (IsPlaced() && !isLoaded) {
         foreach (var placeable in GetComponents<IPlaceable>()) {
            placeable.OnPlace();
         }
      }

      if (!IsPlaced()) {
         var rb = gameObject.AddComponent<Rigidbody>();
         rb.useGravity = false;
         rb.isKinematic = false;
         rb.constraints = RigidbodyConstraints.FreezeAll;
      }
   }

   public bool IsBlocked() {
      return !isPlaced && blocked > 0;
   }

   public bool Place() {
      if (ResourceManager.GetInstance().OffsetAll(-woodCost, -stoneCost, -metalCost, 0)) {
         isPlaced = true;
         foreach (var placeable in GetComponents<IPlaceable>()) {
            placeable.OnPlace();
         }
         OnPlaceEvent.Invoke(this);
         Destroy(GetComponent<Rigidbody>());
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

   public bool Upgrade() {
      if (ResourceManager.GetInstance().OffsetAll(-woodUpgradeCost, -stoneUpgradeCost, -metalUpgradeCost, 0)) {
         foreach (var placeable in GetComponents<IPlaceable>()) {
            placeable.OnUpgrade();
         }

         level++;
         if (level < levels.Count) {
            levels[level].SetActive(true);
            levels[level - 1].SetActive(false);
         }

         return true;
      }
      return false;
   }

   private void OnCollisionEnter(Collision collision) {
      if (!isPlaced) {
         blocked++;
      }
   }

   private void OnCollisionExit(Collision collision) {
      if (!isPlaced) {
         blocked--;
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("isPlaced", isPlaced);
      data.Add("destructable", destructable);
      data.Add("upgradeable", upgradeable);
      data.Add("woodUpgradeCost", woodUpgradeCost);
      data.Add("stoneUpgradeCost", stoneUpgradeCost);
      data.Add("metalUpgradeCost", metalUpgradeCost);
      data.Add("level", level);

      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      isPlaced = (bool)data["isPlaced"];
      destructable = (bool)data["destructable"];
      upgradeable = (bool)data["upgradeable"];
      woodUpgradeCost = (int)data["woodUpgradeCost"];
      stoneUpgradeCost = (int)data["stoneUpgradeCost"];
      metalUpgradeCost = (int)data["metalUpgradeCost"];
      level = (int)data["level"];
      if (levels.Count > 0) {
         levels[0].SetActive(false);
         levels[Mathf.Min(level, levels.Count - 1)].SetActive(true);
      }
      isLoaded = true;
   }

   public void OnLoadDependencies(object savedData) {
      // Ignored
   }
}
