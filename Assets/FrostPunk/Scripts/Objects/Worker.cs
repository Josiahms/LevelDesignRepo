using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Worker : MonoBehaviour, ISaveable
{   
   [SerializeField]
   private float deadZone = 0.2f;

   private Animator animator;
   private Assignable currentDestination;
   private Assignable destination;
   private House house;

   public House House { get { return house;  } }

   public static Worker Instantiate(House house, Vector3 position, Quaternion rotation) {
      var result = Instantiate(ResourceLoader.GetInstance().WorkerPrefab, position + Vector3.Scale(Random.insideUnitSphere, new Vector3(3f, 0, 3f)), rotation);
      result.house = house;
      PopulationManager.GetInstance().AddToWorkforce(result);
      return result;
   }

   private void Awake() {
      animator = GetComponent<Animator>();
   }

   private void OnDestroy() {
      if (house != null) {
         house.RemoveWorker(this);
      }
      if (ResourceManager.GetInstance() != null) {
         PopulationManager.GetInstance().RemoveFromWorkforce(this);
      }
      if (destination != null) {
         destination.RemoveWorker(this);
      }
   }

   public void SetDestination(Assignable destination) {
      this.destination = destination;
   }

   public bool IsAssigned() {
      return destination != null;
   }

   private void Update() {
      if (DayCycleManager.GetInstance().IsRestTime()) {
         currentDestination = house.GetComponent<Assignable>();
      } else {
         currentDestination = destination;
      }

      if (currentDestination == null) {
         animator.SetFloat("Turn", 0);
         animator.SetFloat("Forward", 0);
         return;
      }

      var forward2D = new Vector2(transform.forward.x, transform.forward.z);
      var direction = (currentDestination.transform.position - transform.position).normalized;
      var direction2D = new Vector2(direction.x, direction.z);
      var angleBetween = Vector2.Angle(forward2D, direction2D);
      var between = Quaternion.AngleAxis(angleBetween / 2, transform.up) * transform.forward;
      var angleBetween2 = Vector2.Angle(direction2D, new Vector2(between.x, between.z));
      var isRightTurn = angleBetween2 < angleBetween;

      animator.SetFloat("Turn", Mathf.Clamp(angleBetween / 15, 0, 1) * (isRightTurn ? 1 : -1));
      animator.SetFloat("Forward", Mathf.Clamp((currentDestination.transform.position - transform.position).magnitude * 10, 0, 1));
      if ((currentDestination.transform.position - transform.position).magnitude < deadZone || angleBetween > 15) {
         animator.SetFloat("Forward", 0);
      }

   }

   public void OnLoad(object savedData) {
      // Ignored
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("house", out result)) {
         house = SaveManager.GetInstance().FindLoadedInstanceBySaveIndex((int)result).GetComponent<House>();
      }
      if (data.TryGetValue("destination", out result)) {
         if (result != null) {
            destination = SaveManager.GetInstance().FindLoadedInstanceBySaveIndex((int)result).GetComponent<Assignable>();
         }
      }
   }
}
