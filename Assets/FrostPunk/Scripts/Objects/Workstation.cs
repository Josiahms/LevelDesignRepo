using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(Assignable))]
public class Workstation : MonoBehaviour, ISaveable, ISimulatable {

   [SerializeField]
   private ResourceType resourceType;

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
      var workerCount = GetComponent<Assignable>().GetWorkersInRange();
      if (workerCount == 0 || DayCycleManager.GetInstance().IsRestTime()) {
         progress = 0;
      } else if (!ResourceManager.GetInstance()[resourceType].IsFull()) {
         progress += Time.deltaTime * workerCount * DayCycleManager.GetInstance().ClockMinuteRate;
      }
      var percentComplete = progress / gatherPeriod;
      timer.SetFill(percentComplete);
      if (percentComplete > 1) {
         ResourceManager.GetInstance()[resourceType].OffsetValue(TakeFromPile(1));
         FloatingText.Instantiate(timer.transform, "+1 " + resourceType.ToString());
         progress = progress - gatherPeriod;
      }
   }

   public SimulationInformation GetSimulationInformation() {
      var ratePerDay = DayCycleManager.MIN_IN_DAY / gatherPeriod * GetComponent<Assignable>().GetWorkerCount();
      var expirationTime = DayCycleManager.GetInstance().CurrentTime + (float)quantity / ratePerDay * DayCycleManager.MIN_IN_DAY;
      Debug.Log("Rate per day: " + ratePerDay + ", expiration time: " + expirationTime);
      return new SimulationInformation(resourceType, ratePerDay, expirationTime);
   }
}
