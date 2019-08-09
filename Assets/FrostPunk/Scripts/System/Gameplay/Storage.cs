using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Saveable))]
[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(Placeable))]
public class Storage : MonoBehaviour, IPlaceable
{
   [SerializeField]
   private int woodCapacity;
   [SerializeField]
   private int stoneCapacity;
   [SerializeField]
   private int metalCapacity;
   [SerializeField]
   private int foodCapacity;

   public void OnPlace() {
      ResourceManager.GetInstance().OffsetCapacities(woodCapacity, stoneCapacity, metalCapacity, foodCapacity);
   }

   public void OnRemove() {
      if (GetComponent<Placeable>().IsPlaced() && ResourceManager.GetInstance() != null) {
         ResourceManager.GetInstance().OffsetCapacities(-woodCapacity, -stoneCapacity, -metalCapacity, -foodCapacity);
      }
   }
}
