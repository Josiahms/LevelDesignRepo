using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingOptions : MonoBehaviour {

   [SerializeField]
   private List<Placeable> buildings;

   public List<Placeable> GetBuildingOptions() {
      return buildings;
   }

}
