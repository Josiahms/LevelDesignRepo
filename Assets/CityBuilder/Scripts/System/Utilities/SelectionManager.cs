using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectangularSelection))]
public class SelectionManager : Singleton<SelectionManager> {

   [SerializeField]
   private float dragThreshold = 0.2f;
   [SerializeField]
   private Color highlightColor = new Color(0.98f, 0.96f, 0.76f);
   [SerializeField]
   private Color selectedColor = new Color(0.97f, 0.88f, 0.76f);

   private RectangularSelection rectangularSelection;
   private Vector3 mouseButtonDownPos;
   private bool isEnabled = true;
   private Selectable selectedItem;
   private Selectable hoveredItem;

   private void Start() {
      rectangularSelection = GetComponent<RectangularSelection>();
      rectangularSelection.EndSelection();
   }

   private void Update() {
      if (!isEnabled) {
         return;
      }

      /*if (Input.GetMouseButtonDown(0)) {
         mouseButtonDownPos = Input.mousePosition;
         if (Vector3.Distance(Input.mousePosition, mouseButtonDownPos) > dragThreshold) {
            rectangularSelection.StartSelection(mouseButtonDownPos);
         }
      }

      if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject()) {
         rectangularSelection.EndSelection();
      }*/
   }

   public void Select(Selectable newItem) {
      if (selectedItem != null) {
         Deselect();
      }
      selectedItem = newItem;
      selectedItem.ChangeColor(selectedColor);
      foreach (var selectable in GetComponents<ISelectable>()) {
         selectable.OnSelect();
      }
   }

   public void Deselect() {
      if (selectedItem != null) {
         foreach (var selectable in selectedItem.GetComponents<ISelectable>()) {
            selectable.OnDeselect();
         }
         selectedItem.ChangeColor(Color.clear);
         selectedItem = null;
      }
   }

   public void Hover(Selectable newItem) {
      if (EventSystem.current.IsPointerOverGameObject()) {
         if (hoveredItem == newItem) {
            hoveredItem.ChangeColor(Color.clear);
            hoveredItem = null;
         }
      } else {
         if (hoveredItem != null  && hoveredItem != selectedItem) {
            hoveredItem.ChangeColor(Color.clear);
         }
         hoveredItem = newItem;
         if (hoveredItem != selectedItem) {
            hoveredItem.ChangeColor(highlightColor);
         }
      }
   }

   public void UnHover(Selectable item) {
      if (hoveredItem == item) {
         if (hoveredItem != selectedItem) {
            hoveredItem.ChangeColor(Color.clear);
         }
         hoveredItem = null;
      }
   }

   public Selectable GetSelected() {
      return selectedItem;
   }

   public Selectable GetHovered() {
      return hoveredItem;
   }

   public void Enable() {
      isEnabled = true;
   }

   public void Disable() {
      isEnabled = false;
   }
}
