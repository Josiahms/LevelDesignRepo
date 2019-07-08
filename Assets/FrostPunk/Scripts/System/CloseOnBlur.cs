using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CloseOnBlur : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

   [SerializeField]
   private GameObject target;

   private bool isOver;

   public void OnPointerEnter(PointerEventData eventData) {
      isOver = true;
   }

   public void OnPointerExit(PointerEventData eventData) {
      isOver = false;
   }

   private void Update() {

      if (!isOver && (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(2))) {
         if (target == null) {
            gameObject.SetActive(false);
         } else {
            target.SetActive(false);
         }
      }
   }

}
