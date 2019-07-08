using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {

   private static PlayerInventory instance;

   [SerializeField]
   private Inventory inventory;
   [SerializeField]
   private List<Slot> inventorySlots;

   private void Awake() {
      instance = this;
      inventory.Init();

      for (int i = 0; i < inventorySlots.Count; i++) {
         inventorySlots[i].UpdateSlot(inventory, i);
      }
   }

   public static PlayerInventory GetInstance() {
      return instance;
   }

   public Slot GetFirstOpenSlot() {
      var firstOpenIndex = inventory.GetFirstOpenIndex();
      if (firstOpenIndex >= 0 && firstOpenIndex < inventorySlots.Count) {
         return inventorySlots[firstOpenIndex];
      }

      return null;
   }

   public bool ContainsSlot(Slot slot) {
      return inventorySlots.Contains(slot);
   }
}
