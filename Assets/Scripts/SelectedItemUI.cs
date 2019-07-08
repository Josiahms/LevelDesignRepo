using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectedItemUI : MonoBehaviour {

   [SerializeField]
   private GameObject root;
   [SerializeField]
   private Text nameText;
   [SerializeField]
   private List<Slot> inventorySlots;

   private SelectedItemUI instance;

   public SelectedItemUI GetInstance() {
      return instance;
   }

   public void Show(Interactable selectedObj) {
      instance = Instantiate(this, MainCanvas.Get().transform);
      instance.ShowInstance(selectedObj);
   }

   public void Hide() {
      Destroy(instance.gameObject);
   }

   public Slot GetFirstOpenSlot() {
      return instance.GetFirstOpenSlotOnInstance();
   }

   public bool ContainsSlot(Slot slot) {
      return instance.inventorySlots.Contains(slot);
   }

   public void UpdateAllSlots(Interactable selectedObj) {
      for (int i = 0; i < inventorySlots.Count; i++) {
         inventorySlots[i].UpdateSlot(selectedObj.GetInventory(), i);
      }
   }

   private void ShowInstance(Interactable selectedObj) {
      root.SetActive(true);
      nameText.text = selectedObj.name;
      UpdateAllSlots(selectedObj);
   }

   private Slot GetFirstOpenSlotOnInstance() {
      var selectedObj = Interactable.GetSelected();
      if (selectedObj != null) {
         var firstOpenIndex = selectedObj.GetInventory().GetFirstOpenIndex();
         if (firstOpenIndex >= 0 && firstOpenIndex < inventorySlots.Count) {
            return inventorySlots[firstOpenIndex];
         }
      }

      return null;
   }
}
