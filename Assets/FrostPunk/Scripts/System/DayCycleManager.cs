using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayCycleManager : Singleton<DayCycleManager>, ISaveable {

   [SerializeField]
   private Text clockText;
   [SerializeField]
   private float clockMinuteRate = 10;

   public float CurrentTimeOfDay { get { return currentTime % MIN_IN_DAY; } }
   public float CurrentTime { get { return currentTime; } }
   public float ClockMinuteRate { get { return clockMinuteRate; } }

   private float clockSpeedMultiplier = 1;
   private float currentTime = 540;
   private bool isRestTime = false;

   public static readonly int MIN_IN_DAY = 1440;
   public static readonly int MIN_IN_HOUR = 60;
   public static readonly int FOOD_HOUR = 8;
   public static readonly int MIN_IN_HALF_DAY = 720;
   public static readonly int HOURS_IN_HALF_DAY = 12;
   public static readonly int HOURS_IN_DAY = 24;

   private void Start() {
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

         if (CurrentTimeOfDay > 22 * MIN_IN_HOUR && IsWorkDay()) {
            EndWorkDay();
         }

         if (CurrentTimeOfDay > FOOD_HOUR * MIN_IN_HOUR && CurrentTimeOfDay < 18 * MIN_IN_HOUR) {
            StartWorkDay();
         }

         var prevTime = (int)(currentTime / clockMinuteRate) * clockMinuteRate;
         yield return new WaitUntil(() => currentTime - prevTime >= clockMinuteRate);
      }
   }

   private void Update() {
      currentTime += (clockMinuteRate * clockSpeedMultiplier) * Time.deltaTime;
   }

   public void EndWorkDay() {
      isRestTime = true;
      ResourceManager.GetInstance().EatMeal();
   }

   public void StartWorkDay() {
      isRestTime = false;
   }

   public bool IsWorkDay() {
      return !isRestTime;
   }

   public bool IsRestTime() {
      return isRestTime;
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
      data.Add("isRestTime", isRestTime);
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
      if (data.TryGetValue("isRestTime", out result)) {
         isRestTime = (bool)result;
      }
   }

   public void OnLoadDependencies(object savedData) {
      // Ignored
   }
}
