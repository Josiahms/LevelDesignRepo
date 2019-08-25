using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlaceQuestObjective : QuestObjective {

   public string placeableName { get; private set; }

   public PlaceQuestObjective(string text, int goal, string placeableName) : base(text, goal) {
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