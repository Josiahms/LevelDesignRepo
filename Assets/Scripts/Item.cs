using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Slotable {

   public override void OnClick(Slot slot) {
      var currentlySelectedSlot = Slot.GetSelected();
      if (currentlySelectedSlot == null) {
         slot.Select();
      } else if (currentlySelectedSlot != this) {
         slot.SwapWithSlot(currentlySelectedSlot);
      }
   }

   public override void OnDoubleClick(Slot slot) {
      if (PlayerInventory.GetInstance().ContainsSlot(slot)) {
         slot.SwapWithSlot(Interactable.GetSelectedItemUI().GetFirstOpenSlot());
      } else {
         slot.SwapWithSlot(PlayerInventory.GetInstance().GetFirstOpenSlot());
      }
   }

   public override void OnRightClick(Slot slot) {
      return;
   }

}
