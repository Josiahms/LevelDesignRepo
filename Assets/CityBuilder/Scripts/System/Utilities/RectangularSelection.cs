using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class RectangularSelection : MonoBehaviour {

   private RectTransform rectTransform;
   private Image selectionImage;

   private void Awake() {
      rectTransform = GetComponent<RectTransform>();
      selectionImage = GetComponent<Image>();
   }

   public void StartSelection(Vector3 position) {
      selectionImage.enabled = true;
      rectTransform.anchoredPosition = position;
      rectTransform.sizeDelta = new Vector2(0, 0);
   }

   public Rect EndSelection() {
      selectionImage.enabled = false;
      return rectTransform.rect;
   }

   public Rect GetRect() {
      if (!selectionImage.enabled) {
         return new Rect();
      }
      return rectTransform.rect;
   }

   private void Update() {
      var mouseDelta =Input.mousePosition - rectTransform.anchoredPosition3D;
      rectTransform.sizeDelta = new Vector2(Mathf.Abs(mouseDelta.x), Mathf.Abs(mouseDelta.y));
      rectTransform.localScale = new Vector2(Mathf.Sign(mouseDelta.x), Mathf.Sign(mouseDelta.y));
   }

}
