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
   private int capacity;
   [SerializeField]
   private int startingAmount;
   [SerializeField]
   private List<GameObject> contents;

   private void Start() {
      GetComponent<Selectable>().Description = "Provides storage for " + capacity + " " + type;
   }

   public void OnPlace() {
      ResourceManager.GetInstance()[type].OffsetCapacity(capacity);
      ResourceManager.GetInstance()[type].OffsetValue(startingAmount);
   }

   public void OnUpgrade() {
      ResourceManager.GetInstance()[type].OffsetCapacity(15);
      capacity += 15;
      GetComponent<Selectable>().Description = "Provides storage for " + capacity + " " + type;
   }

   public void OnRemove() {
      if (GetComponent<Placeable>().IsPlaced() && ResourceManager.GetInstance() != null) {
         ResourceManager.GetInstance()[type].OffsetCapacity(-capacity);
      }
   }

   private void Update() {
      var percentFull = (float)ResourceManager.GetInstance()[type].Amount / ResourceManager.GetInstance()[type].Capacity;
      var numEnabled = contents.Count * percentFull;
      for (int i = 0; i < contents.Count; i++) {
         contents[i].SetActive(i < numEnabled && GetComponent<Placeable>().IsPlaced());
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("type", type);
      data.Add("amount", capacity);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("type", out result)) {
         type = (ResourceType)result;
      }
      if (data.TryGetValue("amount", out result)) {
         capacity = (int)result;
      }
   }

   public void OnLoadDependencies(object data) {
      GetComponent<Selectable>().Description = "Provides storage for " + capacity + " " + type;
   }
}
