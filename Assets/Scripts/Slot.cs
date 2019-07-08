using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

   private static Slot currentlySelectedSlot;

   [SerializeField]
   private Button slottedItemBtn;
   [SerializeField]
   private Button emptySlot;

   private Inventory inventory;
   private int index;
   private bool isOver;
   private float timeSelected;

   private void Awake() {
      slottedItemBtn.onClick.AddListener(() => {
         inventory.GetAt(index).OnClick(this);
         var isDoubleClick = Time.time - timeSelected < 0.25f;
         if (isDoubleClick) {
            inventory.GetAt(index).OnDoubleClick(this);
         }
         timeSelected = Time.time;
      });

      emptySlot.onClick.AddListener(() => {
         if (currentlySelectedSlot != null) {
            SwapWithSlot(currentlySelectedSlot);
         }
      });
   }

   public void SwapWithSlot(Slot other) {
      if (other == null) {
         return;
      }

      var myInventory = inventory;
      var myIndex = index;
      var otherInventory = other.inventory;
      var otherIndex = other.index;
      var myItem = myInventory.GetAt(myIndex);
      var otherItem = otherInventory.GetAt(otherIndex);

      var mySuccess = myInventory.Begin() && myInventory.ReplaceAt(myIndex, otherItem) && myInventory.Commit();
      var otherSuccess = otherInventory.Begin() && otherInventory.ReplaceAt(otherIndex, myItem) && otherInventory.Commit();

      UpdateSlot(myInventory, myIndex);
      other.UpdateSlot(otherInventory, otherIndex);
      currentlySelectedSlot = null;
   }

   private void SafelyInsertInventoryItem(List<Slotable> inventory, int index, Slotable item) {
      while (inventory.Count <= index) {
         inventory.Add(null);
      }
      inventory[index] = item;
   }

   private void Update() {
      if (Input.GetMouseButtonUp(0)) {
         if (!isOver && currentlySelectedSlot == this) {
            currentlySelectedSlot = null;
         }
      }

      if (Input.GetMouseButtonUp(1) && isOver) {
         inventory.GetAt(index).OnRightClick(this);
      }

      if (currentlySelectedSlot == this) {
         ((Image)slottedItemBtn.targetGraphic).color = Color.red;
      } else {
         ((Image)slottedItemBtn.targetGraphic).color = Color.white;
      }
   }

   public void UpdateSlot(Inventory inventory, int index) {
      var item = inventory.GetAt(index);

      if (item != null) {
         // There seems to be a bug where not setting sprite to null results in squashing
         ((Image)slottedItemBtn.targetGraphic).sprite = null;
         ((Image)slottedItemBtn.targetGraphic).sprite = item.GetSprite();
      } else {
         ((Image)slottedItemBtn.targetGraphic).sprite = null;
      }

      slottedItemBtn.gameObject.SetActive(item != null);
      emptySlot.gameObject.SetActive(item == null);
      
      this.inventory = inventory;
      this.index = index;
   }

   public void OnPointerEnter(PointerEventData data) {
      isOver = true;
   }

   public void OnPointerExit(PointerEventData data) {
      isOver = false;
   }

   public static Slot GetSelected() {
      return currentlySelectedSlot;
   }

   public void Select() {
      currentlySelectedSlot = this;
   }
}
