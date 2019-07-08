﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : Singleton<Tutorial>, ISaveable {

   [SerializeField]
   private House firstHouse;
   [SerializeField]
   private Transform fastWoodPile;
   [SerializeField]
   private RectTransform woodAmount;
   [SerializeField]
   private RectTransform buildMenu;
   [SerializeField]
   private RectTransform foodAmount;
   [SerializeField]
   private RectTransform starvingAmount;
   [SerializeField]
   private bool skip;

   private enum CameraZoom { Close, Medium, Far }
   private Vector3 initialCameraPosition;
   private int currentStep;
   private Transform target;
   private CameraZoom zoomLevel;
   private bool resetCamera;

   private new void Awake() {
      base.Awake();
      initialCameraPosition = Camera.main.transform.position;
   }

   private void Start() {
      if (currentStep != 0) {
         return;
      }
      if (skip) {
         Skip();
      } else {
         StepOne();
      }
   }

   private void StepOne() {
      currentStep = 1;
      DayCycleManager.GetInstance().AdjustGameplaySpeed(0, false);
      FocusCameraOn(firstHouse.transform, CameraZoom.Close);
      TutorialPopup.Instantiate("Hello there!  Welcome to the village.  It's your duty to manage the workers here to ensure they have " +
         "the resources they need to survive.  If you are successful with this we can expand!", StepTwo);
   }

   private void StepTwo() {
      currentStep = 2;
      FocusCameraOn(fastWoodPile, CameraZoom.Medium);
      TutorialPopup.Instantiate("First things first, lets collect some wood.  Click on the stack of wood and assign a few workers to it.  " +
         "They'll start collecting wood immediately.  Gather 5 wood before moving on.", StepThree);
   }

   private void StepThree() {
      currentStep = 3;
   }

   private void StepFour() {
      currentStep = 4;
      TutorialPopup.Instantiate(woodAmount, "Nice work!  You got enough wood to build a farm.", StepFive);
   }

   private void StepFive() {
      currentStep = 5;
      TutorialPopup.Instantiate(buildMenu, "Open the build menu to build a farm.  You can hover over any item to see its cost.  Build the farm, assign up to 2 workers to it, and collect 5 food to continue.", StepSix, true);
   }

   private void StepSix() {
      currentStep = 6;
   }

   private void StepSeven() {
      currentStep = 7;
      var popup = TutorialPopup.Instantiate("Wow... That's super slow.  We'll work on making that faster as we advance, for now, here is 20 more wood, enough for 2 more farms.", StepEight);
      ResourceManager.GetInstance().AddResource(ResourceType.Wood, 20);
      FloatingText.Instantiate(popup.transform, 20, ResourceType.Wood.ToString(), true, true, 1.5f);
   }

   private void StepEight() {
      currentStep = 8;
   }

   private void StepNine() {
      currentStep = 9;
      TutorialPopup.Instantiate(foodAmount, "Great!  You collected enough food for your 5 workers.  " +
         "Each day at 8:00am and 8:00pm the workers will each eat 1 food.", StepTen);
   }

   private void StepTen() {
      currentStep = 10;
      TutorialPopup.Instantiate(starvingAmount, "If there isn't enough food for a worker at either meal time, he will become starving.  " +
         "A starving worker will consume 2 food at the next meal time.  If there is not enough food for all the starving workers at either meal time, the village will fail!", StepEleven);
   }

   private void StepEleven() {
      currentStep = 11;
      resetCamera = true;
      var popup = TutorialPopup.Instantiate("That's it!  Start sending some workers out to collect resources, monitor your food value, and work towards sustainability!  Here is enough wood for a second house to get you started.", StepTwelve);
      ResourceManager.GetInstance().AddResource(ResourceType.Wood, 35);
      FloatingText.Instantiate(popup.transform, 35, ResourceType.Wood.ToString(), true, true, 1.5f);
   }

   private void StepTwelve() {
      currentStep = 12;
      DayCycleManager.GetInstance().AdjustGameplaySpeed(1);
   }

   private void Skip() {
      currentStep = 12;
      ResourceManager.GetInstance().AddResource(ResourceType.Wood, 45);
      resetCamera = true;
   }

   private void MoveCameraToTarget() {
      Vector3 destination;
      if (resetCamera) {
         destination = initialCameraPosition;
      } else if (target == null) {
         destination = Camera.main.transform.position;
      } else {
         int distance;
         switch (zoomLevel) {
            case CameraZoom.Close:
               distance = 20;
               break;
            case CameraZoom.Medium:
               distance = 30;
               break;
            case CameraZoom.Far:
               distance = 40;
               break;
            default:
               distance = 20;
               break;
         }
         destination = target.position - Camera.main.transform.forward * distance;
      }
      Camera.main.transform.position += (destination - Camera.main.transform.position) / 10;
   }

   private void FocusCameraOn(Transform target, CameraZoom zoomLevel) {
      this.target = target;
      this.zoomLevel = zoomLevel;
   }

   private void Update() {
      if (currentStep == 3 && ResourceManager.GetInstance().WoodAmount >= 10) {
         StepFour();
      }

      if (currentStep == 6 && ResourceManager.GetInstance().FoodAmount == 1) {
         StepSeven();
      }

      if (currentStep == 8 && ResourceManager.GetInstance().FoodAmount >= 5) {
         StepNine();
      }
      MoveCameraToTarget();
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("currentStep", currentStep);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("currentStep", out result)) {
         currentStep = (int)result;
      }
   }

}