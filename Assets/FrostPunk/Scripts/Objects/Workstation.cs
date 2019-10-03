using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(Assignable))]
public class Workstation : MonoBehaviour, ISaveable, ISimulatable {

   [SerializeField]
   private ResourceType type;

   [SerializeField]
   private int quantity;
   public int GetQuantity() { return quantity; }

   [SerializeField]
   [Range(1, 900)]
   private int gatherPeriod = 5;

   private WorkstationStatusUI statusUI;
   private float progress;

   public bool canFunction = true;
   public float PercentComplete { get { return progress / gatherPeriod; } }

   private void Awake() {
      statusUI = WorkstationStatusUI.Instantiate(this);
   }

   public int TakeFromPile(int amount) {
      if (quantity == -1) {
         return amount;
      }
      if (quantity - amount > 0) {
         quantity -= amount;
         return amount;
      }
      Destroy(gameObject);
      Destroy(statusUI.gameObject);
      return quantity;
   }

   public bool IsFunctioning() {
      return !ResourceManager.GetInstance()[type].IsFull() && canFunction;
   }

   private void Update() {
      var assignable = GetComponent<Assignable>();
      if (IsFunctioning()) {
         progress += Time.deltaTime * assignable.GetWorkersInRange() * DayCycleManager.GetInstance().ClockMinuteRate;
      }

      if (assignable.GetWorkersInRange() == 0 || DayCycleManager.GetInstance().IsRestTime()) {
         progress = 0;
      }
      
      if (PercentComplete > 1) {
         ResourceManager.GetInstance()[type].OffsetValue(TakeFromPile(1));
         FloatingText.Instantiate(statusUI.transform, "+1 " + type.ToString());
         progress = progress - gatherPeriod;
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("type", type);
      data.Add("quantity", quantity);
      data.Add("gatherPeriod", gatherPeriod);
      data.Add("progress", progress);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("type", out result)) {
         type = (ResourceType)result;
      }
      if (data.TryGetValue("quantity", out result)) {
         quantity = (int)result;
      }
      if (data.TryGetValue("gatherPeriod", out result)) {
         gatherPeriod = (int)result;
      }
      if (data.TryGetValue("progress", out result)) {
         progress = (float)result;
      }
   }

   public void OnLoadDependencies(object savedData) {
      // Ignored
   }

   public SimulationInformation GetSimulationInformation() {
      var ratePerDay = DayCycleManager.MIN_IN_DAY / gatherPeriod * GetComponent<Assignable>().GetWorkerCount();
      var expirationTime = DayCycleManager.GetInstance().CurrentTime + (float)quantity / ratePerDay * DayCycleManager.MIN_IN_DAY;
      Debug.Log("Rate per day: " + ratePerDay + ", expiration time: " + expirationTime);
      return new SimulationInformation(type, ratePerDay, expirationTime);
   }
}
