using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayCycleManager : Singleton<DayCycleManager>, ISaveable {

   [SerializeField]
   private Text clockText;
   [SerializeField]
   private float clockMinuteRate = 10;

   public float CurrentTime { get { return currentTime; } }
   public float ClockMinuteRate { get { return clockMinuteRate; } }

   private float clockSpeedMultiplier = 1;
   private float currentTime = 540;
   private float lastFoodTime;

   public static readonly int MIN_IN_DAY = 1440;

   private const int FOOD_HOUR = 8;
   private const int MIN_IN_HOUR = 60;
   private const int MIN_IN_HALF_DAY = 720;
   private const int HOURS_IN_HALF_DAY = 12;
   private const int HOURS_IN_DAY = 24;

   private void Start() {
      if (lastFoodTime == 0) {
         lastFoodTime = (FOOD_HOUR + HOURS_IN_HALF_DAY) * MIN_IN_HOUR;
      }
      StartCoroutine(ClockCycle());
   }

   public void AdjustGameplaySpeed(float speedMultiplier, bool affectWorkingRate = true) {
      if (affectWorkingRate) {
         Time.timeScale = speedMultiplier;
      } else {
         Time.timeScale = 1;
      }
      clockSpeedMultiplier = speedMultiplier;
   }

   private IEnumerator ClockCycle() {
      while (true) {
         int days = (int)currentTime / MIN_IN_DAY;
         var minutesMod60 = (int)currentTime % MIN_IN_HOUR;
         var minutesText = minutesMod60 < 10 ? "0" + minutesMod60 : minutesMod60.ToString();
         int hours = (int)(currentTime / MIN_IN_HOUR % HOURS_IN_HALF_DAY);
         if (hours == 0) {
            hours = HOURS_IN_HALF_DAY;
         }

         var isMorning = (currentTime % MIN_IN_DAY) < MIN_IN_HALF_DAY;
         clockText.text = "Day: " + (days + 1) + " " + hours + ":" + minutesText + (isMorning ? "am" : "pm");
         
         if (currentTime > lastFoodTime) {
            lastFoodTime += MIN_IN_HALF_DAY;
            ResourceManager.GetInstance().EatMeal();
         }
         var prevTime = (int)(currentTime / clockMinuteRate) * clockMinuteRate;
         yield return new WaitUntil(() => currentTime - prevTime >= clockMinuteRate);
      }
   }

   private void Update() {
      currentTime += (clockMinuteRate * clockSpeedMultiplier) * Time.deltaTime;
   }

   public bool IsNight() {
      return currentTime % MIN_IN_DAY < FOOD_HOUR * MIN_IN_HOUR || currentTime % MIN_IN_DAY > (FOOD_HOUR + HOURS_IN_HALF_DAY) * MIN_IN_HOUR;
   }

   public int GetCurrentHour() {
      int hours = (int)(currentTime / MIN_IN_HOUR % HOURS_IN_DAY);
      if (hours == 0) {
         hours = HOURS_IN_HALF_DAY;
      }
      return hours;
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("clockMinuteRate", clockMinuteRate);
      data.Add("clockSpeedMultiplier", clockSpeedMultiplier);
      data.Add("currentTime", currentTime);
      data.Add("lastFoodTime", lastFoodTime);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("clockMinuteRate", out result)) {
         clockMinuteRate = (float)result;
      }
      if (data.TryGetValue("clockSpeedMultiplier", out result)) {
         clockSpeedMultiplier = (float)result;
      }
      if (data.TryGetValue("currentTime", out result)) {
         currentTime = (float)result;
      }
      if (data.TryGetValue("lastFoodTime", out result)) {
         lastFoodTime = (float)result;
      }
   }

   public void OnLoadDependencies(object savedData) {
      // Ignored
   }
}
