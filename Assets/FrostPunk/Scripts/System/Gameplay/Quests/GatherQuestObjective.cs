using System;
using UnityEngine.Events;

[Serializable]
public class GatherQuestObjective : QuestObjective {

   public ResourceType resourceType;

   private UnityAction action;

   public GatherQuestObjective(string text, int goal, int amount, ResourceType type) : base(text, goal, amount) {
      resourceType = type;
   }

   protected override void OnInit() {
      Resource.OnChangeEvent.AddListener(Action);
   }

   protected override void OnRemove() {
      Resource.OnChangeEvent.RemoveListener(Action);
   }

   private void Action(ResourceType type, int amount) {
      if (type == resourceType) {
         Amount += amount;
      }
   }
}