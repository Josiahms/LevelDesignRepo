using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestManager : Singleton<QuestManager>, ISaveable {

   private List<QuestObjective> objectives = new List<QuestObjective>();

   public void AddObjective(QuestObjective objective) {
      var uiInstance = QuestUI.Instantiate(transform, objective);
      uiInstance.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -30 * objectives.Count);
      objective.Init(uiInstance);
      objectives.Add(objective);
   }

   public void CompleteObjective(QuestObjective objective) {
      objectives.Remove(objective);
      Destroy(objective.uiInstance.gameObject);
      for (int i = 0; i < objectives.Count; i++) {
         var obj = objectives[i];
         obj.uiInstance.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -30 * i);
      }
   }

   public void AbandonObjective(QuestObjective objective) {

   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      var savedObjectives = new List<Dictionary<string, object>>();
      foreach (var objective in objectives) {
         var savedObjective = new Dictionary<string, object>();
         savedObjective.Add("text", objective.Text);
         savedObjective.Add("goal", objective.Goal);
         savedObjective.Add("amount", objective.Amount);
         savedObjective.Add("type", objective.GetType());
         if (objective.GetType() == typeof(PlaceQuestObjective)) {
            savedObjective.Add("placeableName", ((PlaceQuestObjective)objective).placeableName);
         } else if (objective.GetType() == typeof(GatherQuestObjective)) {
            savedObjective.Add("resourceType", ((GatherQuestObjective)objective).resourceType);
         }
         if (objective.onComplete != null) {
            var onCompleteCallback = new Dictionary<string, object>();
            onCompleteCallback.Add("method", objective.onComplete.Method);
            onCompleteCallback.Add("target", ((Component)objective.onComplete.Target).GetComponent<Saveable>().GetSavedIndex());
            onCompleteCallback.Add("componentType", objective.onComplete.Target.GetType());
            savedObjective.Add("onComplete", onCompleteCallback);
         } else {
            savedObjective.Add("onComplete", null);
         }
         savedObjectives.Add(savedObjective);
      }

      data.Add("objectives", savedObjectives);

      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      var loadedObjectives = (List<Dictionary<string, object>>)data["objectives"];
      foreach(var loadedObjective in loadedObjectives) {
         if ((Type)loadedObjective["type"] == typeof(PlaceQuestObjective)) {
            var objective = new PlaceQuestObjective((string)loadedObjective["text"], (int)loadedObjective["goal"], (int)loadedObjective["amount"], (string)loadedObjective["placeableName"]);
            objective.onComplete = (UnityAction)loadedObjective["onComplete"];
            AddObjective(objective);
         } else if ((Type)loadedObjective["type"] == typeof(GatherQuestObjective)) {
            var objective = new GatherQuestObjective((string)loadedObjective["text"], (int)loadedObjective["goal"], (int)loadedObjective["amount"], (ResourceType)loadedObjective["resourceType"]);
            AddObjective(objective);
         }
      }
   }

   public void OnLoadDependencies(object data) {
      var savedData = (Dictionary<string, object>)data;
      var loadedObjectives = (List<Dictionary<string, object>>)savedData["objectives"];
      for (int i = 0; i < objectives.Count; i++) {
         var onCompleteCallback = (Dictionary<string, object>)loadedObjectives[i]["onComplete"];
         if (onCompleteCallback != null) {
            var target = SaveManager.GetInstance().FindLoadedInstanceBySaveIndex((int)onCompleteCallback["target"]);
            var targetComponent = target.GetComponent((Type)onCompleteCallback["componentType"]);
            var callback = (MethodInfo)onCompleteCallback["method"];
            objectives[i].onComplete = () => callback.Invoke(targetComponent, new object[] { });
         }
      }
      
   }
}
