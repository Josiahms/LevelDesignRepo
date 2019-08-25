using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class QuestObjective {
   public string Text { get; private set; }
   public int Goal { get; private set; }
   public QuestUI uiInstance { get; private set; }

   private int amount;
   public int Amount {
      get { return amount; }

      protected set {
         amount = value;
         if (amount >= Goal) {
            QuestManager.GetInstance().CompleteObjective(this);
            OnRemove();
         }
         if (uiInstance != null) {
            uiInstance.UpdateUI();
         }
      }
   }

   public QuestObjective(string text, int goal, int amount) {
      Text = text;
      Goal = goal;
      Amount = amount;
   }

   protected abstract void OnInit();
   public void Init(QuestUI ui) {
      uiInstance = ui;
      OnInit();
   }

   protected abstract void OnRemove();
}

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
            AddObjective(new PlaceQuestObjective((string)loadedObjective["text"], (int)loadedObjective["goal"], (int)loadedObjective["amount"], (string)loadedObjective["placeableName"]));
         } else if ((Type)loadedObjective["type"] == typeof(GatherQuestObjective)) {
            AddObjective(new GatherQuestObjective((string)loadedObjective["text"], (int)loadedObjective["goal"], (int)loadedObjective["amount"], (ResourceType)loadedObjective["resourceType"]));
         }
      }
   }

   public void OnLoadDependencies(object data) {
      // Ignored
   }
}
