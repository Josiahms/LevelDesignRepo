using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface ISelectable {
   void OnSelect();
   void OnDeselect();
}

[RequireComponent(typeof(Collider))]
public class Selectable : MonoBehaviour {

   [SerializeField]
   private string itemName;
   public string GetItemName() { return itemName; }

   [SerializeField]
   private string description;
   public string Description { get { return description; } set { description = value; } }

   private static bool isEnabled = true;
   private static Selectable selectedItem;
   private static Selectable hoveredItem;

   private Color highlightColor;
   private Color selectedColor;
   private Outline outline;

   protected void Awake() {
      selectedColor = new Color(0.97f, 0.88f, 0.76f);
      highlightColor = new Color(0.98f, 0.96f, 0.76f);
      outline = gameObject.AddComponent<Outline>();
      outline.OutlineMode = Outline.Mode.OutlineVisible;
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
         selectedItem = null;
      }
   }

   public void Select() {
      if (selectedItem != null) {
         Deselect();
      }
      selectedItem = this;
      selectedItem.ChangeColor(selectedColor);
      foreach (var selectable in GetComponents<ISelectable>()) {
         selectable.OnSelect();
      }
   }

   public void ChangeColor(Color color) {
      outline.OutlineColor = color;
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
         ChangeColor(Color.clear);
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
