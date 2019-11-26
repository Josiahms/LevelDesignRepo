using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DayCycleManager : Singleton<DayCycleManager>, ISaveable {

   public UnityEvent OnStartWorkDay = new UnityEvent();
   public UnityEvent OnEndWorkDay = new UnityEvent();

   [SerializeField]
   private Text clockText;
   [SerializeField]
   private float clockMinuteRate = 10;
   private float clockMinuteRateBackup;

   public float CurrentTimeOfDay { get { return currentTime % MIN_IN_DAY; } }
   public float CurrentTime { get { return currentTime; } }
   public float ClockMinuteRate { get { return clockMinuteRate; } }
   public int Day { get { return (int)currentTime / MIN_IN_DAY + 1; } }

   private float currentTime = 540;
   private bool isRestTime = false;

   public static readonly int MIN_IN_DAY = 1440;
   public static readonly int MIN_IN_HOUR = 60;
   public static readonly int START_OF_DAY = 8 * MIN_IN_HOUR;
   public static readonly int MIN_IN_HALF_DAY = 720;
   public static readonly int HOURS_IN_HALF_DAY = 12;
   public static readonly int HOURS_IN_DAY = 24;
   public static readonly int MAX_END_WORK_DAY = 22 * MIN_IN_HOUR;
   public static readonly int MIN_END_WORK_DAY = 18 * MIN_IN_HOUR;

   private void Start() {
      clockMinuteRateBackup = clockMinuteRate;
      StartCoroutine(ClockCycle());
   }

   private IEnumerator ClockCycle() {
      while (true) {
         var minutesMod60 = (int)currentTime % MIN_IN_HOUR;
         var minutesText = minutesMod60 < 10 ? "0" + minutesMod60 : minutesMod60.ToString();
         int hours = (int)(currentTime / MIN_IN_HOUR % HOURS_IN_HALF_DAY);
         if (hours == 0) {
            hours = HOURS_IN_HALF_DAY;
         }

         var isMorning = (currentTime % MIN_IN_DAY) < MIN_IN_HALF_DAY;
         clockText.text = "Day: " + Day + " " + hours + ":" + minutesText + (isMorning ? "am" : "pm");

         if (CurrentTimeOfDay > MAX_END_WORK_DAY && IsWorkDay()) {
            EndWorkDay();
         }

         if (CurrentTimeOfDay > START_OF_DAY && CurrentTimeOfDay < MIN_END_WORK_DAY && !IsWorkDay()) {
            StartWorkDay();
         }

         var prevTime = (int)(currentTime / clockMinuteRate) * clockMinuteRate;
         yield return new WaitUntil(() => currentTime - prevTime >= clockMinuteRate);
      }
   }

   private void Update() {
      currentTime += clockMinuteRate * Time.deltaTime;
   }

   public void EndWorkDay() {
      clockMinuteRateBackup = clockMinuteRate;
      clockMinuteRate = 30;
      isRestTime = true;
      OnEndWorkDay.Invoke();
      Debug.Log("Foo");
   }

   public void IncreaseTimeSpeed() {
      clockMinuteRate += 5;
   }

   public void DecreaseTimeSpeed() {
      clockMinuteRate -= 5;
      clockMinuteRate = Mathf.Max(clockMinuteRate, 0);
   }

   public void StartWorkDay() {
      clockMinuteRate = clockMinuteRateBackup;
      isRestTime = false;
      OnStartWorkDay.Invoke();
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
