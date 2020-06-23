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
         var nextUpgrade = selected.GetComponent<Upgradeable>() != null ? selected.GetComponent<Upgradeable>().GetNextUpgrade() : null;
         if (nextUpgrade != null) {
            btn.interactable = ResourceManager.GetInstance().CanAfford(-nextUpgrade.wood, -nextUpgrade.stone, -nextUpgrade.metal, 0);
            return;
         }
      }
      btn.interactable = false;
   }

   private void Upgrade() {
      var selected = SelectionManager.GetInstance().GetFirstSelected();
      if (selected != null && selected.GetComponent<Upgradeable>() != null) {
         selected.GetComponent<Upgradeable>().Upgrade();
      }
   }

   public void OnPointerEnter(PointerEventData eventData) {
      showOverlayRoutine = StartCoroutine(ShowCostOverlayDelay());
   }

   private IEnumerator ShowCostOverlayDelay() {
      yield return new WaitForSeconds(0.45f);
      var selected = SelectionManager.GetInstance().GetFirstSelected();
      if (selected != null) {
         var nextUpgrade = selected.GetComponent<Upgradeable>() != null ? selected.GetComponent<Upgradeable>().GetNextUpgrade() : null;
         if (nextUpgrade != null) {
            btn.interactable = ResourceManager.GetInstance().CanAfford(-nextUpgrade.wood, -nextUpgrade.stone, -nextUpgrade.metal, 0);
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
