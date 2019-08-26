using System;
using UnityEngine.Events;

[Serializable]
public abstract class QuestObjective {
   public string Text;
   public int Goal;
   public QuestUI uiInstance { get; private set; }


   // WARN: At this point, this onComplete callback can only target a saveable object.
   public UnityAction onComplete;

   private int amount;
   public int Amount {
      get { return amount; }

      protected set {
         amount = value;
         if (amount >= Goal) {
            QuestManager.GetInstance().CompleteObjective(this);
            onComplete();
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