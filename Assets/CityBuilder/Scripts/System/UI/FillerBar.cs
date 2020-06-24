using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FillerBar : MonoBehaviour {
   [SerializeField]
   private Image topImage;
   [SerializeField]
   private Image bottomImage;

   [SerializeField]
   private Text text;

   private Destructable destructableParent;

   /*public static FillerBar Instantiate(Transform target, float curVal, float maxVal, Color topColor, Color bottomColor, Color textColor) {
      var instance = Instantiate(ResourceLoader.GetInstance().FillerBar, target);
      instance.SetPercent(curVal, maxVal);
      instance.topImage.color = topColor;
      instance.bottomImage.color = bottomColor;
      instance.text.color = textColor;
      return instance;
   }*/

   private void Start() {
      destructableParent = GetComponentInParent<Destructable>();
   }

   private void Update() {
      var camTrans = Camera.main.transform;
      var cameraNormal = camTrans.forward.normalized;

      bottomImage.transform.LookAt(bottomImage.transform.position + cameraNormal);
      if (destructableParent != null) {
         SetPercent(destructableParent.Health, destructableParent.MaxHealth);
         SetVisible(destructableParent.Health != destructableParent.MaxHealth);
      } else {
         SetVisible(false);
      }
   }

   private void SetPercent(float curVal, float maxVal) {
      if (curVal < 0 || maxVal <= 0 || maxVal < curVal) {
         return;
      }

      topImage.transform.localScale = new Vector3(curVal / maxVal, 1, 1);
      text.text = curVal + " / " + maxVal;
   }

   private void SetVisible(bool isVisible) {
      bottomImage.enabled = isVisible;
      topImage.enabled = isVisible;
      text.enabled = isVisible;
   }
}
