using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceLoader : Singleton<ResourceLoader> {

   [SerializeField]
   private WorkstationStatusUI timerCirclePrefab;
   public WorkstationStatusUI TimerCirclePreab { get { return timerCirclePrefab; } }
  
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

   [SerializeField]
   private Worker workerPrefab;
   public Worker WorkerPrefab { get { return workerPrefab; } }

   [SerializeField]
   private BuildSite buildSite;
   public BuildSite BuildSite { get { return buildSite; } }
}
