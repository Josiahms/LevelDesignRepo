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
   private Assignable assignable;

   public static WorkstationStatusUI Instantiate(Workstation workstation) {
      var instance = Instantiate(ResourceLoader.GetInstance().TimerCirclePreab, MainCanvas.Get().transform);
      instance.transform.SetAsFirstSibling();
      instance.GetComponent<AttachUIToTarget>().SetTarget(workstation.transform);
      instance.workstation = workstation;
      instance.assignable = workstation.GetComponent<Assignable>();
      return instance;
   }

   private void Start() {
      Update();
   }

   private void Update() {
      circle.fillAmount = workstation.PercentComplete;
      if (assignable.GetWorkerCount() == 0) {
         workerText.text = "";
      } else {
         workerText.text = assignable.GetWorkerCount() + "/" + assignable.GetMaxAssignees();
      }


      if (!workstation.IsFunctioning() && assignable.GetWorkerCount() > 0) {
         warning.enabled = true;
      } else {
         warning.enabled = false;
      }
   }
}
