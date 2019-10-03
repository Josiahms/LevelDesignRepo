using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Worker : MonoBehaviour, ISaveable
{   
   [SerializeField]
   private float deadZone = 0.2f;

   private Animator animator;
   private Transform currentDestination;
   private Assignable assignedLocation;

   public House House { get; private set; }

   public static Worker Instantiate(House house, Vector3 position, Quaternion rotation) {
      var result = Instantiate(ResourceLoader.GetInstance().WorkerPrefab, position + Vector3.Scale(Random.insideUnitSphere, new Vector3(3f, 0, 3f)), rotation);
      result.House = house;
      PopulationManager.GetInstance().AddToWorkforce(result);
      return result;
   }

   private void Awake() {
      animator = GetComponent<Animator>();
   }

   private void OnDestroy() {
      if (House != null) {
         House.RemoveWorker(this);
      }
      if (ResourceManager.GetInstance() != null) {
         PopulationManager.GetInstance().RemoveFromWorkforce(this);
      }
      if (assignedLocation != null) {
         assignedLocation.RemoveWorker(this);
      }
   }

   public void SetDestination(Assignable destination) {
      this.assignedLocation = destination;
   }

   public bool IsAssigned() {
      return assignedLocation != null;
   }

   private void Update() {
      if (DayCycleManager.GetInstance().IsRestTime()) {
         currentDestination = House.transform;
      } else {
         currentDestination = assignedLocation == null ? null : assignedLocation.GetSpotForWorker(this);
      }

      if (currentDestination == null) {
         animator.SetFloat("Turn", 0);
         animator.SetFloat("Forward", 0);
         return;
      }

      // 1 is normal speed;
      var speed = DayCycleManager.GetInstance().ClockMinuteRate / 5;
      if (speed <= 2) {
         AnimatedWalk(speed);
      } else {
         animator.SetFloat("Turn", 0);
         animator.SetFloat("Forward", 0);
         TeleportWalk(speed);
      }

   }

   private void TeleportWalk(float speed) {
      transform.LookAt(currentDestination);
      var distance = currentDestination.position - transform.position;
      transform.position += Vector3.ClampMagnitude(distance, speed * Time.deltaTime * 10);
   }

   private void AnimatedWalk(float speed) {
      var forward2D = new Vector2(transform.forward.x, transform.forward.z);
      var direction = (currentDestination.position - transform.position).normalized;
      var direction2D = new Vector2(direction.x, direction.z);
      var angleBetween = Vector2.Angle(forward2D, direction2D);
      var between = Quaternion.AngleAxis(angleBetween / 2, transform.up) * transform.forward;
      var angleBetween2 = Vector2.Angle(direction2D, new Vector2(between.x, between.z));
      var isRightTurn = angleBetween2 < angleBetween;

      animator.speed = speed;
      animator.SetFloat("Turn", Mathf.Clamp(angleBetween / 15, 0, 1) * (isRightTurn ? 1 : -1));
      animator.SetFloat("Forward", Mathf.Clamp((currentDestination.position - transform.position).magnitude * 10, 0, 1));
      if ((currentDestination.position - transform.position).magnitude < deadZone || angleBetween > 15) {
         animator.SetFloat("Forward", 0);
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("destination", assignedLocation != null ? assignedLocation.GetComponent<Saveable>().GetSavedIndex() : -1);
      data.Add("house", House.GetComponent<Saveable>().GetSavedIndex());
      return data;
   }

   public void OnLoad(object savedData) {
      // Ignored
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("house", out result)) {
         House = SaveManager.GetInstance().FindLoadedInstanceBySaveIndex((int)result).GetComponent<House>();
      }
      if (data.TryGetValue("destination", out result)) {
         assignedLocation = (int)result == -1 ? null : SaveManager.GetInstance().FindLoadedInstanceBySaveIndex((int)result).GetComponent<Assignable>();
      }
   }
}
