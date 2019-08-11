using System.Collections;
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
      if (skip) {
         Skip();
      } else {
         GoToStep(currentStep);
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
      ResourceManager.GetInstance()[ResourceType.Wood].OffsetValue(20);
      FloatingText.Instantiate(popup.transform, 20, ResourceType.Wood.ToString(), true, true, 1.5f);
   }

   private void StepEight() {
      currentStep = 8;
   }

   private void StepNine() {
      currentStep = 9;
      TutorialPopup.Instantiate(foodAmount, "Great!  You collected enough food for your 5 workers.  " +
         "Your workers will eat each morning before the workday starts, at 8am sharp.", StepTen);
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
      ResourceManager.GetInstance()[ResourceType.Wood].OffsetValue(35);
      FloatingText.Instantiate(popup.transform, 35, ResourceType.Wood.ToString(), true, true, 1.5f);
   }

   private void StepTwelve() {
      currentStep = 12;
      DayCycleManager.GetInstance().AdjustGameplaySpeed(1);
   }

   private void Skip() {
      currentStep = 12;
      ResourceManager.GetInstance()[ResourceType.Wood].OffsetValue(45);
      resetCamera = true;
   }

   private void MoveCameraToTarget() {
      Vector3 destination;
      if (resetCamera) {
         destination = initialCameraPosition;
         Camera.main.orthographicSize = 35;
      } else if (target == null) {
         destination = Camera.main.transform.position;
      } else {
         switch (zoomLevel) {
            case CameraZoom.Close:
               Camera.main.orthographicSize += (15 - Camera.main.orthographicSize) / 10;
               break;
            case CameraZoom.Medium:
               Camera.main.orthographicSize += (20 - Camera.main.orthographicSize) / 10;
               break;
            case CameraZoom.Far:
               Camera.main.orthographicSize += (35 - Camera.main.orthographicSize) / 10;
               break;
            default:
               Camera.main.orthographicSize += (35 - Camera.main.orthographicSize) / 10;
               break;
         }
         destination = target.position - Camera.main.transform.forward * 100;
      }
      Camera.main.transform.position += (destination - Camera.main.transform.position) / 10;
   }

   private void FocusCameraOn(Transform target, CameraZoom zoomLevel) {
      this.target = target;
      this.zoomLevel = zoomLevel;
   }

   private void Update() {
      var resourceManager = ResourceManager.GetInstance();
      if (currentStep == 3 && resourceManager[ResourceType.Wood].Amount >= 10) {
         StepFour();
      }

      if (currentStep == 6 && resourceManager[ResourceType.Food].Amount == 1) {
         StepSeven();
      }

      if (currentStep == 8 && resourceManager[ResourceType.Food].Amount >= 5) {
         StepNine();
      }
      MoveCameraToTarget();
   }

   private void GoToStep(int stepNum) {
      switch (stepNum) {
         case 0:
            StepOne();
            return;
         case 1:
            StepOne();
            return;
         case 2:
            StepTwo();
            return;
         case 3:
            StepThree();
            return;
         case 4:
            StepFour();
            return;
         case 5:
            StepFive();
            return;
         case 6:
            StepSix();
            return;
         case 7:
            StepSeven();
            return;
         case 8:
            StepEight();
            return;
         case 9:
            StepNine();
            return;
         case 10:
            StepTen();
            return;
         case 11:
            StepEleven();
            return;
         case 12:
            StepTwelve();
            return;
         default:
            Skip();
            return;
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("currentStep", currentStep);
      data.Add("firstHouse", firstHouse != null ? firstHouse.GetComponent<Saveable>().GetSavedIndex() : -1);
      data.Add("fastWoodPile", fastWoodPile != null ? fastWoodPile.GetComponent<Saveable>().GetSavedIndex() : -1);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("currentStep", out result)) {
         currentStep = (int)result;
      }
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("firstHouse", out result)) {
         if ((int)result != -1) {
            firstHouse = SaveManager.GetInstance().FindLoadedInstanceBySaveIndex((int)result).GetComponent<House>();
         }
      }
      if (data.TryGetValue("fastWoodPile", out result)) {
         if ((int)result != -1) {
            fastWoodPile = SaveManager.GetInstance().FindLoadedInstanceBySaveIndex((int)result).transform;
         }
      }
   }

}