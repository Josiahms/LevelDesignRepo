using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Saveable))]
[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(Placeable))]
public class Storage : MonoBehaviour, IPlaceable
{
   [SerializeField]
   private ResourceType type;
   [SerializeField]
   private int amount;

   public void OnPlace() {
      ResourceManager.GetInstance()[type].OffsetCapacity(amount);
   }

   public void OnRemove() {
      if (GetComponent<Placeable>().IsPlaced() && ResourceManager.GetInstance() != null) {
         ResourceManager.GetInstance()[type].OffsetCapacity(-amount);
      }
   }
}
