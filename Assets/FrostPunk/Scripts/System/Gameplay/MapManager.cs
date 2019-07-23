using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/**
   The Map Manager needs to be a monobehavior because it will need to be instaitated into the game space.  It will need to exist in all scenes so that the scenes can inform
   the map manager of all the things happening within the scene when the player exits, that way the map manager will be able to calculate summary level statistics and simulate
   the collection of resources as if the player were still in that scene.

   This means the map manager will need to know what all the workers are doing.  The workerAI class does not know what station the worker is currently assigned to, so that will
   either need to be added, or the map manager will need to know what the workers are working on rather than just about the workers themselves.  Additionally, there is a possibility
   of buildings that will have passive effects on resources.  So it seems better that the map manager extends some facility for any entity within the scene to register itself.  Those
   entities would then be responsible for providing the map manager with the information it needs to calculate out the result of the passing of time for that entity.

   This could work a few ways:
   1. A callback which the entity can register with the map manager which returns the change in resources for a day and if that entity is all used up, will tell the map manager to
   unregister it.  The map manager invokes this callback at the end of each day.  When the player visits the location he must wait until the end/start of the next day.  This means the
   map manager will have to invoke 1 callback per entity each day, but each entity will have more control over it's behavior, but each entity will only be able to make decisions given
   the same inputs as they'll all be invoked the same way.
   2. The entity could just inform the map manager of the rate of change for each resource and can inform the map manager of the time which the resource will expire.  This is more
   efficient as the map manager can simply add all rates together for a location which saves lots of computation, however, this implementation assumes that resource nodes will be
   at a constant rate until it expires and then it is done.
   3. Each entity could start a coroutine before the scene unloads, assuming there is a way to keep the coroutines running, then all of these coroutines (1 for each entity) could be running
   in the background adding resources to the resource manager.  The map manager would just check the change in resources at the end of each day and show the change.

**/

// Where I am at currently:
/*
   I have gone down the route of implementing storing the rate and expiration time of all simulatable entities.  One of the biggest sticking points is the fact
   that work stations are not relative to the game time rate.  This means that I need to convert too and from real-time and game-time.  If the game is tracking
   at 15 game minutes per 1 real-time second, and there is 50 wood in a pile being gathered at a rate of 1 wood every 3 real life seconds, then the expiration date
   in game time would be 50 / 1 * 3 * 15 / 1 = 2250.  (quantity / rate * period * game minutes / real seconds).  This is confusing and a waste of time since eventually
   I want the speed of workers and gathering resources to be proportional to the game time (speeding up game time speeds up resource gathering).  Note that Unity time is
   not a factor in any of this.  As far an Unity is concerned time is traveling normally, all the time scaling comes from my code.

   So for next time, I think the best path forward would be to correlate the speed of work stations to the speed of game time before continuing down the route of simulation.
*/
public class MapManager : Singleton<MapManager> {
   private List<Simulatable> registeredEntities = new List<Simulatable>();
   private SimulationExpirationMap expirationTimes = new SimulationExpirationMap();

   private float prevTime;
   private int deltaWood;
   private int deltaStone;
   private int deltaMetal;
   private int deltaFood;

   private Coroutine loop;

   private void Start() {   
      prevTime = DayCycleManager.GetInstance().CurrentTime;
   }

   private void Update() {
      if (FoodLabel.GetInstance() != null) {
         FoodLabel.GetInstance().text = deltaFood > 0 ? "+" + deltaFood : deltaFood.ToString();
      } 
   }

   public void RegisterEntity(Simulatable entity) {
      registeredEntities.Add(entity);
   }

   public void UnregisterEntity(Simulatable entity) {
      registeredEntities.Remove(entity);
   }

   public void GetSimulationInformation() {
      if (loop == null) {
         loop = StartCoroutine(SimulationLoop());
      }
      registeredEntities.ForEach(x => {
         x.GetSimulationInformation().ForEach(y => {
            if (y.expirationTime.HasValue) {
               expirationTimes.AddSimulationInformation(y.expirationTime.Value, y);
            }

            switch (y.resourceType) {
               case ResourceType.Wood:
                  deltaWood += y.rateOfChange;
                  break;
               case ResourceType.Stone:
                  deltaStone += y.rateOfChange;
                  break;
               case ResourceType.Metal:
                  deltaMetal += y.rateOfChange;
                  break;
               case ResourceType.Food:
                  deltaFood += y.rateOfChange;
                  break;
            }

         });
      });
   }

   private IEnumerator SimulationLoop() {
      Debug.Log("Here we go!  Current time: " + DayCycleManager.GetInstance().CurrentTime);
      prevTime = DayCycleManager.GetInstance().CurrentTime;
      while (true) {
         yield return new WaitForSeconds(96 / 4);  // 96 is the number of real seconds it takes to go 1 day at 15 game minutes per 1 real life second.
         Debug.Log("Ding!  It's been 1/4 of a day.");
         var currentTime = DayCycleManager.GetInstance().CurrentTime;
         var expiredSimulations = expirationTimes.PopSimulationInformationsUpToTime(currentTime);
         Debug.Log(expiredSimulations.Count + " simulations expired in that time block.  Current time: " + currentTime);
         expiredSimulations.ForEach(expiredSimulation => {
            var partialAmount = expiredSimulation.rateOfChange * (expiredSimulation.expirationTime.Value - prevTime) / DayCycleManager.MIN_IN_DAY;
            Debug.Log("Adding " + (expiredSimulation.expirationTime.Value - prevTime) / DayCycleManager.MIN_IN_DAY + " of the full amount " + expiredSimulation.rateOfChange + ", " + partialAmount);
            ResourceManager.GetInstance().AddResource(expiredSimulation.resourceType, (int)partialAmount);
            switch (expiredSimulation.resourceType) {
               case ResourceType.Wood:
                  deltaWood -= expiredSimulation.rateOfChange;
                  break;
               case ResourceType.Stone:
                  deltaStone -= expiredSimulation.rateOfChange;
                  break;
               case ResourceType.Metal:
                  deltaMetal -= expiredSimulation.rateOfChange;
                  break;
               case ResourceType.Food:
                  deltaFood -= expiredSimulation.rateOfChange;
                  break;
            }
         });
         Debug.Log("Adding " + deltaFood / 4 + " food");
         ResourceManager.GetInstance().AddResource(ResourceType.Wood, deltaWood / 4);
         ResourceManager.GetInstance().AddResource(ResourceType.Stone, deltaStone / 4);
         ResourceManager.GetInstance().AddResource(ResourceType.Metal, deltaMetal / 4);
         ResourceManager.GetInstance().AddResource(ResourceType.Food, deltaFood / 4);
         prevTime = currentTime;
      }
   }
}

public class SimulationExpirationMap {

   private SortedList<float, List<SimulationInformation>> expirationTimes = new SortedList<float, List<SimulationInformation>>();

   public void AddSimulationInformation(float expirationTime, SimulationInformation simulationInformation) {
      List<SimulationInformation> simulationInfos;
      if (expirationTimes.TryGetValue(expirationTime, out simulationInfos)) {
         simulationInfos.Add(simulationInformation);
      } else {
         expirationTimes.Add(expirationTime, new List<SimulationInformation> { simulationInformation });
      }
   }

   public List<SimulationInformation> PopSimulationInformationsUpToTime(float time) {
      if (expirationTimes.Count == 0 || expirationTimes.Keys[0] > time) {
         return new List<SimulationInformation>();
      }

      var greatestExpirationTime = expirationTimes.Keys.Last(expirationTime => expirationTime <= time);
      var highestIndex = expirationTimes.IndexOfKey(greatestExpirationTime);
      var result = new List<SimulationInformation>();
      for (int i = 0; i <= highestIndex; i++) {
         result.AddRange(expirationTimes.Values[0]);
         expirationTimes.RemoveAt(0);
      }

      return result;
   }

}