using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

   [SerializeField]
   private Placeable placeablePrefab;
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
      button = transform.GetChild(0).GetComponent<Button>();
      button.onClick.AddListener(OnClick);
      originalColors = additionalGraphics.Select(x => x.color).ToList();
   }

   private void OnClick() {
      Placer.GetInstance().SetPlaceable(placeablePrefab);
      buildDrawer.SetActive(false);
      if (overlay != null) {
         Destroy(overlay.gameObject);
      }
   }

   private void Update() {
      var canAfford = ResourceManager.GetInstance().CanAfford(-placeablePrefab.GetWoodCost(), -placeablePrefab.GetStoneCost(), -placeablePrefab.GetMetalCost(), -placeablePrefab.GetFoodCost());
      button.interactable = canAfford;
      for (int i = 0; i < additionalGraphics.Count; i++) {
         additionalGraphics[i].color = canAfford ? originalColors[i] : button.colors.disabledColor;
      }
      
   }

   private void OnEnable() {
      button.gameObject.SetActive(SelectionManager.GetInstance().GetFirstSelected().GetComponent<BuildingOptions>().GetBuildingOptions().Contains(placeablePrefab));
   }

   public void OnPointerEnter(PointerEventData eventData) {
      showOverlayRoutine = StartCoroutine(ShowCostOverlayDelay());
   }

   private IEnumerator ShowCostOverlayDelay() {
      yield return new WaitForSeconds(0.45f);
      overlay = BuildingCostOverlay.Instantiate(transform, placeablePrefab.GetWoodCost(), placeablePrefab.GetStoneCost(), placeablePrefab.GetMetalCost(), placeablePrefab.GetFoodCost());
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
