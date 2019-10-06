using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuildButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

   [SerializeField]
   private Placeable buildingPrefab;
   [SerializeField]
   private GameObject buildDrawer;
   [SerializeField]
   private List<MaskableGraphic> additionalGraphics;
   private List<Color> originalColors;

   private Button button;
   private bool clickedThisFrame;

   private BuildingCostOverlay overlay;
   private Coroutine showOverlayRoutine;

   private void Awake() {
      button = GetComponent<Button>();
      button.onClick.AddListener(OnClick);
      originalColors = additionalGraphics.Select(x => x.color).ToList();
   }

   private void OnClick() {
      Builder.GetInstance().SetBuilding(buildingPrefab);
      buildDrawer.SetActive(false);
      if (overlay != null) {
         Destroy(overlay.gameObject);
      }
   }

   private void Update() {
      var canAfford = ResourceManager.GetInstance().CanAfford(-buildingPrefab.GetWoodCost(), -buildingPrefab.GetStoneCost(), -buildingPrefab.GetMetalCost(), 0);
      button.interactable = canAfford;
      for (int i = 0; i < additionalGraphics.Count; i++) {
         additionalGraphics[i].color = canAfford ? originalColors[i] : button.colors.disabledColor;
      }
   }

   public void OnPointerEnter(PointerEventData eventData) {
      showOverlayRoutine = StartCoroutine(ShowCostOverlayDelay());
   }

   private IEnumerator ShowCostOverlayDelay() {
      yield return new WaitForSeconds(0.45f);
      overlay = BuildingCostOverlay.Instantiate(transform, buildingPrefab.GetWoodCost(), buildingPrefab.GetStoneCost(), buildingPrefab.GetMetalCost());
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
