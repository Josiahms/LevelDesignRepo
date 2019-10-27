using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AttachUIToTarget))]
public class FillerBar : MonoBehaviour {
   [SerializeField]
   private Image topImage;
   [SerializeField]
   private Image bottomImage;

   [SerializeField]
   private Text text;

   public static FillerBar Instantiate(Transform target, float curVal, float maxVal, Color topColor, Color bottomColor, Color textColor) {
      var instance = Instantiate(ResourceLoader.GetInstance().FillerBar, MainCanvas.Get().transform);
      instance.GetComponent<AttachUIToTarget>().SetTarget(target);
      instance.SetPercent(curVal, maxVal);
      instance.topImage.color = topColor;
      instance.bottomImage.color = bottomColor;
      instance.text.color = textColor;
      return instance;
   }

   public void SetPercent(float curVal, float maxVal) {
      if (curVal < 0 || maxVal <= 0 || maxVal < curVal) {
         return;
      }

      SetVisible(curVal != maxVal);

      topImage.transform.localScale = new Vector3(curVal / maxVal, 1, 1);
      text.text = curVal + " / " + maxVal;
   }

   public void SetVisible(bool isVisible) {
      bottomImage.enabled = isVisible;
      topImage.enabled = isVisible;
      text.enabled = isVisible;
   }
}
