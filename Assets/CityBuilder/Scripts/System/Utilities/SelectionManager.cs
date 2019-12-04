using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectangularSelection))]
public class SelectionManager : Singleton<SelectionManager> {

   [SerializeField]
   private float dragThreshold = 10;
   [SerializeField]
   private Color highlightColor = new Color(0.98f, 0.96f, 0.76f);
   [SerializeField]
   private Color selectedColor = new Color(0.97f, 0.88f, 0.76f);

   private RectangularSelection rectangularSelection;
   private Vector3 mouseButtonDownPos;
   private bool isEnabled = true;
   private List<Selectable> selectedItems = new List<Selectable>();
   private List<Selectable> hoveredItems = new List<Selectable>();

   private void Start() {
      rectangularSelection = GetComponent<RectangularSelection>();
      rectangularSelection.EndSelection();
   }

   private void Update() {
      if (!isEnabled) {
         return;
      }

      if (!EventSystem.current.IsPointerOverGameObject()) {

         if (Input.GetMouseButtonDown(0)) {
            mouseButtonDownPos = Input.mousePosition;
         }

         if (Input.GetMouseButton(0) && Vector3.Distance(Input.mousePosition, mouseButtonDownPos) > dragThreshold && !rectangularSelection.IsSelectionStarted()) {
            rectangularSelection.StartSelection(mouseButtonDownPos);
         }
      }

      if (Input.GetMouseButtonUp(0)) {
         if (rectangularSelection.IsSelectionStarted() || !EventSystem.current.IsPointerOverGameObject()) {
            DeselectAll();
            SelectAll(hoveredItems);
         }
         rectangularSelection.EndSelection();
      }
   }

   public void SelectAll(List<Selectable> newItems) {
      if (!isEnabled) {
         return;
      }

      selectedItems.AddRange(newItems);
      newItems.ForEach(x => x.ChangeColor(selectedColor));
      foreach (var selectable in newItems.SelectMany(x => x.GetComponents<ISelectable>())) {
         selectable.OnSelect();
      }
   }

   public void DeselectAll() {
      foreach (var selectable in selectedItems.SelectMany(x => x.GetComponents<ISelectable>())) {
         selectable.OnDeselect();
      }
      selectedItems.ForEach(x => x.ChangeColor(Color.clear));
      selectedItems.Clear(); 
   }

   public void Deselect(Selectable item) {
      foreach (var x in item.GetComponents<ISelectable>()) {
         x.OnDeselect();
      }
      selectedItems.Remove(item);
      item.ChangeColor(Color.clear);
   }

   public void Hover(Selectable hoveredItem) {
      if (!isEnabled) {
         return;
      }

      if (hoveredItems.Contains(hoveredItem)) {
         return;
      }

      hoveredItems.Add(hoveredItem);
      if (!selectedItems.Contains(hoveredItem)) {
         hoveredItem.ChangeColor(highlightColor);
      }
   }

   public void UnHover(Selectable item) {
      if (!hoveredItems.Contains(item)) {
         return;
      }

      if (!selectedItems.Contains(item)) {
         item.ChangeColor(Color.clear);
      }
      hoveredItems.Remove(item);
   }

   public Rect? GetSelectionRect() {
      return rectangularSelection.GetRect();
   }

   public Selectable GetFirstSelected() {
      return selectedItems.Count > 0 ? selectedItems[0] : null;
   }

   public List<Selectable> GetSelected() {
      return selectedItems;
   }

   public List<Selectable> GetHovered() {
      return hoveredItems;
   }

   public void Enable() {
      isEnabled = true;
   }

   public void Disable() {
      isEnabled = false;
   }
}
