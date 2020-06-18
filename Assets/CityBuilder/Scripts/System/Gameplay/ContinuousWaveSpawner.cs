using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** This consideres the delay to be before the current wave.  Meaning the first wave, wave 1, is already **/
/** started when the game starts and it's delay is in progress.  Once the wave spawns, this moves to wave 2. **/
public class ContinuousWaveSpawner : MonoBehaviour, ISaveable {

   [SerializeField]
   private int initialNumEnemies = 3;
   [SerializeField, Range(0, 1)]
   private float difficultyMultiplier = 0.25f;
   [SerializeField]
   private float initialWaveDelay = 5;  // The number of minutes before wave 1
   [SerializeField]
   private float minWaveDelay = 1; // The least amount of minutes between waves
   [SerializeField, Range(0, 0.5f)]
   private float waveShorteningRate = 0.05f;

   [SerializeField]
   private int currentWave = 1;
   [SerializeField]
   private float timeSinceLastWave;

   private int GetNumEnemiesToSpawn() {
      // 0 - difficulty stays the same, 1 - difficulty doubles each wave
      return (int)(initialNumEnemies + Mathf.Pow(1 + difficultyMultiplier, currentWave));
   }

   private float GetMinutesBetweenCurrentWave() {
      // 0: time between waves stays the same,  0.5: time between waves is cut in half each wave
      return Mathf.Pow(1 - waveShorteningRate, currentWave) * (initialWaveDelay - minWaveDelay) + minWaveDelay;
   }

   private void Update() {
      timeSinceLastWave += Time.deltaTime;
      if (timeSinceLastWave > GetMinutesBetweenCurrentWave() * 60) {
         SpawnEnemies(GetNumEnemiesToSpawn());
         timeSinceLastWave = 0;
         currentWave++;
      }
   }

   private void SpawnEnemies(int numEnemies) {
      for (int i = 0; i < numEnemies; i++) {
         var randomOffset = Random.insideUnitCircle * Mathf.Sqrt(numEnemies + 1);
         Attacker.Instantiate(transform.position + new Vector3(randomOffset.x, 0, randomOffset.y));
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("currentWave", currentWave);
      data.Add("timeSinceLastWave", timeSinceLastWave);
      return data;
   }

   public void OnLoad(object data) {
      var savedData = (Dictionary<string, object>)data;
      currentWave = (int)savedData["currentWave"];
      timeSinceLastWave = (float)savedData["timeSinceLastWave"];
   }

   public void OnLoadDependencies(object data) {
      
   }
}