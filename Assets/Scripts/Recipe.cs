using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Recipe : MonoBehaviour {

   [SerializeField]
   private List<ItemSlotPair> requirements;
   [SerializeField]
   private List<ItemSlotPair> inputs;
   [SerializeField]
   private List<ItemSlotPair> outputs;

   private void Awake() {
      GetComponent<Button>().onClick.AddListener(Craft);
   }

   private void Craft() {
      Inventory inventory = Interactable.GetSelected().GetInventory();
      var success = inventory.Begin() && 
         inventory.ContainsAll(requirements) && 
         inventory.RemoveAll(inputs) && 
         inventory.AddAll(outputs) && 
         inventory.Commit();

      if (!success) {
         inventory.Rollback();
      }

      Interactable.GetSelectedItemUI().UpdateAllSlots(Interactable.GetSelected());
   }
}
