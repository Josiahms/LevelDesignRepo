using System;

[Serializable]
public class PlaceQuestObjective : QuestObjective {

   public string placeableName;

   public PlaceQuestObjective(string text, int goal, int amount, string placeableName) : base(text, goal, amount) {
      this.placeableName = placeableName;
   }

   protected override void OnInit() {
      Placeable.OnPlaceEvent.AddListener(OnAdd);
      Placeable.OnRemoveEvent.AddListener(OnRemove);
   }

   protected override void OnRemove() {
      Placeable.OnPlaceEvent.RemoveListener(OnAdd);
      Placeable.OnRemoveEvent.RemoveListener(OnRemove);
   }

   private void OnAdd(Placeable placeable) {
      if (placeable.name == string.Format("{0}(Clone)", placeableName)) {
         Amount++;
      }
   }

   private void OnRemove(Placeable placeable) {
      if (placeable.name == string.Format("{0}(Clone)", placeableName)) {
         Amount--;
      }
   }
}