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
   private List<GameObject> contents;
   private Resource resourceInstance;

   private void Start() {
      resourceInstance = ResourceManager.GetInstance()[type];
      GetComponent<Selectable>().Description = "Provides storage for " + amount + " " + type;
   }

   public void OnPlace() {
      ResourceManager.GetInstance()[type].OffsetCapacity(amount);
   }

   public void OnUpgrade() {
      ResourceManager.GetInstance()[type].OffsetCapacity(15);
      amount += 15;
      GetComponent<Selectable>().Description = "Provides storage for " + amount + " " + type;
   }

   public void OnRemove() {
      if (GetComponent<Placeable>().IsPlaced() && ResourceManager.GetInstance() != null) {
         ResourceManager.GetInstance()[type].OffsetCapacity(-amount);
      }
   }

   private void Update() {
      var percentFull = (float)resourceInstance.Amount / resourceInstance.Capacity;
      var numEnabled = contents.Count * percentFull;
      for (int i = 0; i < contents.Count; i++) {
         contents[i].SetActive(i < numEnabled);
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("type", type);
      data.Add("amount", amount);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("type", out result)) {
         type = (ResourceType)result;
      }
      if (data.TryGetValue("amount", out result)) {
         amount = (int)result;
      }
   }

   public void OnLoadDependencies(object data) {
      GetComponent<Selectable>().Description = "Provides storage for " + amount + " " + type;
   }
}
