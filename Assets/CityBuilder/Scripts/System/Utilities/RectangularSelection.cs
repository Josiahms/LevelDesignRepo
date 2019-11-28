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
      transform.position = position;
      rectTransform.sizeDelta = new Vector2(0, 0);
   }

   public void EndSelection() {
      selectionImage.enabled = false;
   }

   public Rect GetRect() {
      return new Rect();
   }

   private void Update() {
      var mouseDelta = Input.mousePosition - transform.position;
      rectTransform.sizeDelta = new Vector2(mouseDelta.x, mouseDelta.z);
   }

}
