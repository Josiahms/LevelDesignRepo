using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

   private Button btn;
   private Coroutine showOverlayRoutine;
   private BuildingCostOverlay overlay;

   private void Awake() {
      btn = GetComponent<Button>();
      btn.onClick.AddListener(Upgrade);
   }

   private void Update() {
      var selected = SelectionManager.GetInstance().GetFirstSelected();
      if (selected != null) {
         var placeable = selected.GetComponent<Placeable>();
         if (placeable != null) {
            btn.interactable = ResourceManager.GetInstance().CanAfford(-placeable.WoodUpgradeCost, -placeable.StoneUpgradeCost, -placeable.MetalUpgradeCost, 0);
         }
      }
   }

   private void Upgrade() {
      var selected = SelectionManager.GetInstance().GetFirstSelected();
      if (selected != null && selected.GetComponent<Placeable>() != null) {
         selected.GetComponent<Placeable>().Upgrade();
      }
   }

   public void OnPointerEnter(PointerEventData eventData) {
      showOverlayRoutine = StartCoroutine(ShowCostOverlayDelay());
   }

   private IEnumerator ShowCostOverlayDelay() {
      yield return new WaitForSeconds(0.45f);
      var selected = SelectionManager.GetInstance().GetFirstSelected();
      if (selected != null) {
         var placeable = selected.GetComponent<Placeable>();
         if (placeable != null) {
            overlay = BuildingCostOverlay.Instantiate(transform, placeable.WoodUpgradeCost, placeable.StoneUpgradeCost, placeable.MetalUpgradeCost, 0);
         }
      }
   }

   public void OnPointerExit(PointerEventData eventData) {
      if (showOverlayRoutine != null) {
         StopCoroutine(showOverlayRoutine);
      }
      if (overlay != null) {
         Destroy(overlay.gameObject);
      }
   }

}
