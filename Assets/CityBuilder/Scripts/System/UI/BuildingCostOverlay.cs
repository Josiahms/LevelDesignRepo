using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingCostOverlay : MonoBehaviour {
   [SerializeField]
   private Text foodCost;
   [SerializeField]
   private Text woodCost;
   [SerializeField]
   private Text stoneCost;
   [SerializeField]
   private Text metalCost;

   public static BuildingCostOverlay Instantiate(Transform parent, int woodCost, int stoneCost, int metalCost, int foodCost) {
      var instance = Instantiate(ResourceLoader.GetInstance().BuildingCostOverlay, parent);
      instance.woodCost.text = woodCost.ToString();
      instance.stoneCost.text = stoneCost.ToString();
      instance.metalCost.text = metalCost.ToString();
      instance.foodCost.text = foodCost.ToString();
      return instance;
   }
}
