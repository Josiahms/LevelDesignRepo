using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AttachUIToTarget))]
public class WorkstationStatusUI : MonoBehaviour {

   [SerializeField]
   private Image circle;
   [SerializeField]
   private Text workerText;
   [SerializeField]
   private Text warning;

   private AttachUIToTarget attacher;

   public static WorkstationStatusUI Instantiate(Transform target) {
      var instance = Instantiate(ResourceLoader.GetInstance().TimerCirclePreab, MainCanvas.Get().transform);
      instance.transform.SetAsFirstSibling();
      instance.GetComponent<AttachUIToTarget>().SetTarget(target);
      return instance;
   }

   public void SetFill(float percent) {
      circle.fillAmount= percent;
   }

   public void SetWorkerText(string text) {
      workerText.text = text;
   }

   public void SetWarningActive(bool isActive) {
      warning.enabled = isActive;
   }
}
