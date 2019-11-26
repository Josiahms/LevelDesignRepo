using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkerSpawner : Singleton<WorkerSpawner>
{
   [SerializeField]
   [Range(1, 24)]
   private int spawnHour = 11;
   [SerializeField]
   private Transform spawnpoint;
   [SerializeField]
   private Text populationText;

   private int prevHour;

   private void Start() {
      prevHour = spawnHour;
   }

   private void Update() {
      var dayCycleManager = DayCycleManager.GetInstance();
      if (dayCycleManager.Day > 2 && dayCycleManager.GetCurrentHour() == spawnHour && prevHour != spawnHour) {
         var randomPoint = Random.insideUnitSphere;
         FloatingText.Instantiate(populationText.transform, 1, "Worker", false, true, 1);
         Worker.Instantiate(
            PopulationManager.GetInstance().GetAvailableHome(),
            spawnpoint.position + new Vector3(randomPoint.x, 0, randomPoint.z),
            spawnpoint.rotation);
      }
      prevHour = dayCycleManager.GetCurrentHour();

   }
}
