using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class NewDayText : MonoBehaviour
{
   [SerializeField]
   private Gradient color;
   [SerializeField]
   private float duration = 4;

   private Text text;
   private float prevTime;
   private float currentStep;

   private void Start()
   {
      text = GetComponent<Text>();
      prevTime = DayCycleManager.GetInstance().CurrentTimeOfDay;
      currentStep = duration;
   }

   private void Update()
   {
      if (prevTime < DayCycleManager.START_OF_DAY && DayCycleManager.GetInstance().CurrentTimeOfDay > DayCycleManager.START_OF_DAY) {
         text.text = "Day " + DayCycleManager.GetInstance().Day;
         currentStep = 0;
      }
      prevTime = DayCycleManager.GetInstance().CurrentTimeOfDay;
      ShowLoop();
   }

   private void ShowLoop() {
      text.color = color.Evaluate(currentStep / duration);
      currentStep += Time.deltaTime;
   }
}
