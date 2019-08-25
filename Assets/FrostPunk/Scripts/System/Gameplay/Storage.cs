using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Saveable))]
[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(Placeable))]
public class Storage : MonoBehaviour, IPlaceable, ISaveable
{
   [SerializeField]
   private ResourceType type;
   [SerializeField]
   private int amount;

   [SerializeField]
   public List<GameObject> contents;

   public void OnPlace() {
      ResourceManager.GetInstance()[type].OffsetCapacity(amount);
   }

   public void OnRemove() {
      if (GetComponent<Placeable>().IsPlaced() && ResourceManager.GetInstance() != null) {
         ResourceManager.GetInstance()[type].OffsetCapacity(-amount);
      }
   }

   private void Update() {
      var percentFull = (float)ResourceManager.GetInstance()[type].Amount / ResourceManager.GetInstance()[type].Capacity;
      var numEnabled = contents.Count * percentFull;
      for (int i = 0; i < contents.Count; i++) {
         contents[i].SetActive(i < numEnabled);
      }
   }
}
