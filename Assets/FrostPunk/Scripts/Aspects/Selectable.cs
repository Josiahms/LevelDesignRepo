using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface ISelectable {
   void OnSelect();
   void OnDeselect();
}

public class Selectable : MonoBehaviour {

   [SerializeField]
   private string itemName;
   public string GetItemName() { return itemName; }

   [SerializeField]
   private string description;
   public string GetDescription() { return description; }

   private static bool isEnabled = true;
   private static Selectable selectedItem;
   private static Selectable hoveredItem;

   private Color highlightColor;
   private Color selectedColor;
   private Renderer[] renderers;

   private GameObject uiInstance;

   protected void Awake() {
      renderers = GetComponentsInChildren<Renderer>();
      selectedColor = new Color(0.97f, 0.88f, 0.76f);
      highlightColor = new Color(0.98f, 0.96f, 0.76f);
   }

   public static Selectable GetSelected() {
      return selectedItem;
   }

   public static void Enable() {
      isEnabled = true;
   }

   public static void Disable() {
      isEnabled = false;
   }

   public static void Deselect() {
      if (selectedItem != null) {
         foreach (var selectable in selectedItem.GetComponents<ISelectable>()) {
            selectable.OnDeselect();
         }
         selectedItem.ChangeColor(Color.white);
         if (UIRenderLocation.GetInstance() != null) {
            Destroy(UIRenderLocation.GetInstance().transform.GetChild(0).gameObject);
         }
         selectedItem = null;
      }
   }

   public void Select() {
      if (selectedItem != null) {
         Deselect();
      }
      selectedItem = this;
      selectedItem.ChangeColor(selectedColor);
      Instantiate(UIRenderLocation.GetInstance().GetUIPrefab(), UIRenderLocation.GetInstance().transform);
      foreach (var selectable in GetComponents<ISelectable>()) {
         selectable.OnSelect();
      }
   }

   public void ChangeColor(Color color) {
      foreach (var renderer in renderers) {
         renderer.material.color = color;
      }
   }

   private void OnMouseOver() {
      if (EventSystem.current.IsPointerOverGameObject()) {
         if (hoveredItem == this) {
            hoveredItem = null;
         }
      } else {
         hoveredItem = this;
      }
   }

   private void OnMouseExit() {
      if (hoveredItem == this) {
         hoveredItem = null;
      }
   }

   private void Update() {

      if (!isEnabled) {
         return;
      }

      if (selectedItem == this) {
         ChangeColor(selectedColor);
      } else if (hoveredItem == this) {
         ChangeColor(highlightColor);
      } else {
         ChangeColor(Color.white);
      }

      if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject()) {
         if (hoveredItem == this) {
            Select();
         } else if (selectedItem == this) {
            Deselect();
         }
      }
   }

   private void OnDestroy() {
      if (selectedItem == this) {
         Deselect();
      }
   }
}
