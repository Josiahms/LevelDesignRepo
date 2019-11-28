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

   private bool isMouseOver;
   private Outline outline;

   protected void Awake() {
      outline = gameObject.AddComponent<Outline>();
      outline.OutlineMode = Outline.Mode.OutlineVisible;
      outline.OutlineWidth = 5;
      outline.OutlineColor = Color.clear;
   }

   public void ChangeColor(Color color) {
      outline.OutlineColor = color;
   }

   private void OnMouseOver() {
      if (EventSystem.current.IsPointerOverGameObject()) {
         isMouseOver = false;
      } else {
         isMouseOver = true;
      }
   }

   private void OnMouseExit() {
      isMouseOver = false;
   }

   private void Update() {
      var selectionBox = SelectionManager.GetInstance().GetSelectionRect();
      var isSelectionBoxOver = selectionBox.HasValue && selectionBox.Value.Contains((Vector2)Camera.main.WorldToScreenPoint(transform.position), true);

      if (!selectionBox.HasValue && isMouseOver || isSelectionBoxOver) {
         SelectionManager.GetInstance().Hover(this);
      } else {
         SelectionManager.GetInstance().UnHover(this);
      }
   }

   private void OnDestroy() {
      var selectionManager = SelectionManager.GetInstance();
      if (selectionManager != null) {
         if (selectionManager.GetSelected().Contains(this)) {
            selectionManager.Deselect(this);
         }

         if (selectionManager.GetHovered().Contains(this)) {
            selectionManager.UnHover(this);
         }
      }


   }
}
