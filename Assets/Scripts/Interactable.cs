using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Interactable : MonoBehaviour {

   public new string name;

   private const int INVENTORY_SIZE = 3;

   private static Interactable currentlySelected;

   [SerializeField]
   private Inventory inventory = new Inventory();
   [SerializeField]
   private SelectedItemUI UI;

   Renderer[] renderers;
   bool isOver;

   public static Interactable GetSelected() {
      return currentlySelected;
   }

   public static SelectedItemUI GetSelectedItemUI() {
      return currentlySelected != null ? currentlySelected.UI.GetInstance() : null;
   }

   public Inventory GetInventory() {
      return inventory;
   }

   private void Awake() {
      renderers = GetComponentsInChildren<Renderer>();
      inventory.Init();
   }

   private void OnMouseOver() {
      if (EventSystem.current.IsPointerOverGameObject()) {
         isOver = false;
      } else {
         isOver = true;
      }
   }

   private void OnMouseExit() {
      isOver = false;
   }

   private void Update() {

      if (IsSelected()) {
         ChangeColor(Color.green);
      } else if (isOver) {
         ChangeColor(Color.red);
      } else {
         ChangeColor(Color.white);
      }

      if (EventSystem.current.IsPointerOverGameObject()) {
         return;
      }

      if (Input.GetMouseButtonUp(0)) {
         if (isOver) {
            Select();
         } else {
            Deselect();
         }
      }
   }

   private void Select() {
      if (currentlySelected != null) {
         currentlySelected.UI.Hide();
      }
      currentlySelected = this;
      if (UI != null) {
         UI.Show(this);
      } else {
      }
   }

   private void Deselect() {
      if (IsSelected()) {
         if (UI != null) {
            UI.Hide();
         }
         currentlySelected = null;
      }
   }

   private void ChangeColor(Color color) {
      foreach (var renderer in renderers) {
         renderer.material.color = color;
      }
   }

   private bool IsSelected() {
      return currentlySelected == this;
   }

}
