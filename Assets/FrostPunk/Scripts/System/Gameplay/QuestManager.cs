using System.Collections.Generic;
using UnityEngine;

public abstract class QuestObjective {
   public string Text { get; private set; }
   public int Goal { get; private set; }
   public QuestUI UiInstance { get; private set; }

   private int amount;

   public int Amount {
      get { return amount; }

      protected set {
         amount = value;
         if (amount >= Goal) {
            QuestManager.GetInstance().CompleteObjective(this);
            OnRemove();
         }
         UiInstance.UpdateUI();
      }
   }

   protected abstract void OnInit();
   public void Init(QuestUI ui) {
      UiInstance = ui;
      OnInit();
   }

   protected abstract void OnRemove();

   public QuestObjective(string text, int goal) {
      Text = text;
      Goal = goal;
   }
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
      Destroy(objective.UiInstance.gameObject);
      for (int i = 0; i < objectives.Count; i++) {
         var obj = objectives[i];
         obj.UiInstance.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -30 * i);
      }
   }

   public void AbandonObjective(QuestObjective objective) {

   }

}
