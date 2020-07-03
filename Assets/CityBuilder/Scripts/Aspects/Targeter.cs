using System;
using System.Collections.Generic;
using UnityEngine;

public class Targeter : MonoBehaviour, ISaveable {

   public Targetable target { get; private set; }
   public Vector3 targetLocation { get; private set; }

   private void Start() { 
      if (target == null) {
         targetLocation = transform.position;
      }
   }

   private void Update() {
      if (target != null) {
         targetLocation = target.GetSpotForTargeter(this).position;
      }
   }

   public bool SetTarget(Vector3 target) {
      if (SetTarget(null)) {
         targetLocation = target;
         return true;
      }
      return false;
   }

   public bool SetTarget(Targetable newTarget) {
      if (target == newTarget) {
         return true;
      }

      if (target != null) {
         var oldTarget = target;
         target = null;
         oldTarget.RemoveTargeter(this);
      }

      if (newTarget != null) {
         if (!newTarget.AddTargeter(this)) {
            return false;
         }
      }

      if (target == null) {
         targetLocation = transform.position;
      }

      target = newTarget;
      return true;
   }

   private void OnDestroy() {
      if (target != null) {
         target.RemoveTargeter(this);
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("target", target != null ? target.GetComponent<Saveable>().GetSavedIndex() : -1);
      data.Add("targetLocation", new float[] {targetLocation.x, targetLocation.y, targetLocation.z});
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      var tl = (float[])data["targetLocation"];
      targetLocation = new Vector3(tl[0], tl[1], tl[2]);
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("target", out result)) {
         if ((int)result != -1) {
            target = SaveManager.GetInstance().FindLoadedInstanceBySaveIndex((int)result).GetComponent<Targetable>();
         }
      }
   }
}