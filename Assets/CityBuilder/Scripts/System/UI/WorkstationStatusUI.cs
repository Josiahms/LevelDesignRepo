using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Workstation))]
[RequireComponent(typeof(AttachUIToTarget))]
public class WorkstationStatusUI : MonoBehaviour {

   [SerializeField]
   private Image circle;
   [SerializeField]
   private Text workerText;
   [SerializeField]
   private Text warning;

   private Workstation workstation;
   private Targetable target;

   public static WorkstationStatusUI Instantiate(Workstation workstation) {
      var instance = Instantiate(ResourceLoader.GetInstance().TimerCirclePreab, MainCanvas.Get().transform);
      instance.transform.SetAsFirstSibling();
      instance.GetComponent<AttachUIToTarget>().SetTarget(workstation.transform);
      instance.workstation = workstation;
      instance.target = workstation.GetComponent<Targetable>();
      return instance;
   }

   private void Start() {
      Update();
   }

   private void Update() {
      circle.fillAmount = workstation.PercentComplete;
      if (target.GetTargeterCount() == 0) {
         workerText.text = "";
      } else {
         workerText.text = target.GetTargeterCount() + "/" + target.GetMaxNumberOfTargeters();
      }


      if (!workstation.IsFunctioning() && target.GetTargeterCount() > 0) {
         warning.enabled = true;
      } else {
         warning.enabled = false;
      }
   }
}
