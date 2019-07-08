using UnityEngine;

public interface IPlaceable {
   bool OnPlace();
   bool OnRemove();
}

public class Placeable : MonoBehaviour {

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

   public bool Place() {
      if (ResourceManager.GetInstance().OffsetMaterials(-woodCost, -stoneCost, -metalCost)) {
         isPlaced = true;
         foreach (var buildable in GetComponents<IPlaceable>()) {
            buildable.OnPlace();
         }
         return true;
      }
      return false;
   }

   public void Remove() {
      if (isPlaced) {
         ResourceManager.GetInstance().OffsetMaterials(woodCost, stoneCost, metalCost);
         foreach (var buildable in GetComponents<IPlaceable>()) {
            buildable.OnRemove();
         }
      }
      Destroy(gameObject);
   }
}
