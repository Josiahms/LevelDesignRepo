﻿using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct SimulationInformation {
   public ResourceType resourceType;
   public int rateOfChange;
   public float? expirationTime;

   public SimulationInformation(ResourceType resourceType, int rateOfChange, float? expirationTime) {
      this.resourceType = resourceType;
      this.rateOfChange = rateOfChange;
      this.expirationTime = expirationTime;
   }
}

public interface ISimulatable {
   SimulationInformation GetSimulationInformation();
}

public class Simulatable : MonoBehaviour {
   private void Awake() {
      Debug.Log("Registering simulatable");
      MapManager.GetInstance().RegisterEntity(this);
   }

   public List<SimulationInformation> GetSimulationInformation() {
      return GetComponents<ISimulatable>().Select(x => x.GetSimulationInformation()).ToList();
   }

   private void OnDestroy() {
      if (SceneManager.GetActiveScene().isLoaded) {
         Debug.Log("Unregistering simulatable");
         MapManager.GetInstance().UnregisterEntity(this);
      }
   }
}