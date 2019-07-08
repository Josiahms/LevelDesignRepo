using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AttachUIToTarget))]
public class FilledCircle : MonoBehaviour {

   [SerializeField]
   private Image circle;

   private AttachUIToTarget attacher;

   public static FilledCircle Instantiate(Transform target) {
      var instance = Instantiate(ResourceLoader.GetInstance().TimerCirclePreab, MainCanvas.Get().transform);
      instance.transform.SetAsFirstSibling();
      instance.GetComponent<AttachUIToTarget>().SetTarget(target);
      return instance;
   }

   public void SetFill(float percent) {
      circle.fillAmount= percent;
   }
}
