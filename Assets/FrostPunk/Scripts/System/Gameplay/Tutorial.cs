using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class TutorialStep : ISaveable {

   [TextArea]
   public string message;
   public Transform cameraTarget;
   public CameraZoom zoomLevel;
   public RectTransform popupTarget;

   public GatherQuestObjective quest;

   private bool dismissed;

   public void Initiate(UnityAction onComplete) {
      if (!dismissed) {
         TutorialPopup.Instantiate(popupTarget, message, () => {
            dismissed = true;
            if (!string.IsNullOrEmpty(quest.Text)) {
               quest.onComplete = onComplete;
               QuestManager.GetInstance().AddObjective(quest);
            } else {
               onComplete();
            }
         });
         TopDownCamera.GetInstance().SetTarget(cameraTarget.position, zoomLevel);
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      if (cameraTarget != null && cameraTarget.GetComponent<Saveable>() != null) {
         data.Add("cameraTarget", cameraTarget.GetComponent<Saveable>().GetSavedIndex());
      } else {
         data.Add("cameraTarget", null);
      }
      if (popupTarget != null && popupTarget.GetComponent<Saveable>() != null) {
         data.Add("popupTarget", popupTarget.GetComponent<Saveable>().GetSavedIndex());
      } else {
         data.Add("popupTarget", null);
      }
      data.Add("dismissed", dismissed);
      return data;
   }

   public void OnLoad(object data) {
      var savedData = (Dictionary<string, object>)data;
      dismissed = (bool)savedData["dismissed"];
   }

   public void OnLoadDependencies(object data) {
      var savedData = (Dictionary<string, object>)data;
      if (savedData["cameraTarget"] != null) {
         cameraTarget = SaveManager.GetInstance().FindLoadedInstanceBySaveIndex((int)savedData["cameraTarget"]).transform;
      }
      if (savedData["popupTarget"] != null) {
         cameraTarget = SaveManager.GetInstance().FindLoadedInstanceBySaveIndex((int)savedData["popupTarget"]).transform;
      }
   }
}

public class Tutorial : Singleton<Tutorial>, ISaveable {

   [SerializeField]
   private List<TutorialStep> tutorialSteps;

   private int currentStep;

   private void Start() {
      CurrentStep();
   }

   private void CurrentStep() {
      if (currentStep < tutorialSteps.Count) {
         tutorialSteps[currentStep].Initiate(NextStep);
      }
   }

   private void NextStep() {
      if (currentStep < tutorialSteps.Count - 1) {
         tutorialSteps[++currentStep].Initiate(NextStep);
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("currentStep", currentStep);
      var savedSteps = new List<object>();
      foreach(var step in tutorialSteps) {
         savedSteps.Add(step.OnSave());
      }
      data.Add("tutorialSteps", savedSteps);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      currentStep = (int)data["currentStep"];

      var steps = (List<object>)data["tutorialSteps"];
      for (int i = 0; i < tutorialSteps.Count; i++) {
         tutorialSteps[i].OnLoad(steps[i]);
      }
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      var steps = (List<object>)data["tutorialSteps"];
      for (int i = 0; i < tutorialSteps.Count; i++) {
         tutorialSteps[i].OnLoadDependencies(steps[i]);
      }
   }

}