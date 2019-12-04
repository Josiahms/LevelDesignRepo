using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : Singleton<EnemySpawner>, ISaveable {

   [System.Serializable]
   public struct Wave { public int day; public int numEnemies; }

   [SerializeField]
   private float spawnSize = 3;
   [SerializeField]
   private List<Wave> waves;

   private int numEnemies;

   private void Start() {
      var dayCycleManager = DayCycleManager.GetInstance();
      dayCycleManager.OnStartWorkDay.AddListener(() => {
         var matchingWaves = waves.Where(x => x.day == dayCycleManager.Day).ToList();
         if (matchingWaves.Count > 0) {
            dayCycleManager.StartAttack();
            matchingWaves.ForEach(x => SpawnEnemies(x.numEnemies));
         }
      });
   }

   private void SpawnEnemies(int num) {
      for (int i = 0; i < num; i++) {
         var randomOffset = Random.insideUnitCircle * spawnSize;
         Enemy.Instantiate(transform.position + new Vector3(randomOffset.x, 0, randomOffset.y));
         numEnemies++;
      }
   }

   public void RemoveEnemy() {
      numEnemies--;
      if (numEnemies == 0) {
         DayCycleManager.GetInstance().EndAttack();
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("numEnemies", numEnemies);
      return data;
   }

   public void OnLoad(object data) {
      var savedData = (Dictionary<string, object>)data;
      numEnemies = (int)savedData["numEnemies"];
   }

   public void OnLoadDependencies(object data) {
      
   }
}
