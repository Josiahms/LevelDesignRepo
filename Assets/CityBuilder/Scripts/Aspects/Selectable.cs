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
      SelectionManager.GetInstance().Hover(this);
   }

   private void OnMouseExit() {
      SelectionManager.GetInstance().UnHover(this);
   }

   private void Update() {
      if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject()) {
         if (SelectionManager.GetInstance().GetSelected() == this) {
            SelectionManager.GetInstance().Deselect();
         } else if (SelectionManager.GetInstance().GetHovered() == this) {
            SelectionManager.GetInstance().Select(this);
         }
      }
   }

   private void OnDestroy() {
      if (SelectionManager.GetInstance() != null && SelectionManager.GetInstance().GetSelected() == this) {
         SelectionManager.GetInstance().Deselect();
      }
   }
}
