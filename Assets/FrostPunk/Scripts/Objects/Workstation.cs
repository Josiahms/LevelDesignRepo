using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

   private FilledCircle timer;
   private float progress;

   private void Awake() {
      timer = FilledCircle.Instantiate(transform);
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
      Destroy(timer.gameObject);
      return quantity;
   }

   private void Update() {
      var workerCount = GetComponent<Assignable>().GetWorkerCount();
      if (workerCount == 0) {
         progress = 0;
      } else {
         progress += Time.deltaTime * workerCount * DayCycleManager.GetInstance().ClockMinuteRate;
      }
      var percentComplete = progress / gatherPeriod;
      timer.SetFill(percentComplete);
      if (percentComplete > 1) {
         ResourceManager.GetInstance().AddResource(type, TakeFromPile(1));
         FloatingText.Instantiate(timer.transform, "+1 " + type.ToString());
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

   public SimulationInformation GetSimulationInformation() {
      var ratePerDay = 1440 / gatherPeriod * GetComponent<Assignable>().GetWorkerCount();
      var expirationTime = DayCycleManager.GetInstance().CurrentTime + (float)quantity / ratePerDay * 1440;
      Debug.Log("Rate per day: " + ratePerDay + ", expiration time: " + expirationTime);
      return new SimulationInformation(type, ratePerDay, expirationTime);
   }
}
