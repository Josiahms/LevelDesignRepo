using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLoader : Singleton<ResourceLoader> {

   [SerializeField]
   private FilledCircle timerCirclePrefab;
   public FilledCircle TimerCirclePreab { get { return timerCirclePrefab; } }
  
   [SerializeField]
   private FloatingText floatingTextPrefab;
   public FloatingText FloatingTextPrefab { get { return floatingTextPrefab;  } }

   [SerializeField]
   private BuildingCostOverlay buildingCostOverlay;
   public BuildingCostOverlay BuildingCostOverlay { get { return buildingCostOverlay; } }

   [SerializeField]
   private TutorialPopup tutorialPopup;
   public TutorialPopup TutorialPopup { get { return tutorialPopup; } }

   [SerializeField]
   private TutorialPopup tutorialPopupDown;
   public TutorialPopup TutorialPopupDown { get { return tutorialPopupDown; } }
}
