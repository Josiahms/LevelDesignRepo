using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuildButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

   [SerializeField]
   private Placeable buildingPrefab;
   [SerializeField]
   private GameObject buildDrawer;

   private Button button;
   private bool clickedThisFrame;

   private BuildingCostOverlay overlay;
   private Coroutine showOverlayRoutine;

   private void Awake() {
      button = GetComponent<Button>();
      button.onClick.AddListener(OnClick);
   }

   private void OnClick() {
      Builder.GetInstance().SetBuilding(buildingPrefab);
      buildDrawer.SetActive(false);
   }

   private void Update() {
      button.interactable = ResourceManager.GetInstance().CanAfford(buildingPrefab.GetWoodCost(), buildingPrefab.GetStoneCost(), buildingPrefab.GetMetalCost());
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
