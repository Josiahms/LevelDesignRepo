using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Worker : MonoBehaviour, ISaveable
{   
   [SerializeField]
   private float deadZone = 0.2f;

   private Animator animator;
   private Vector3? currentDestination;
   private Vector3? destination;
   private House house;

   public static Worker Instantiate(House house, Vector3 position, Quaternion rotation) {
      var result = Instantiate(ResourceLoader.GetInstance().WorkerPrefab, position + Vector3.Scale(Random.insideUnitSphere, new Vector3(3f, 0, 3f)), rotation);
      result.house = house;
      ResourceManager.GetInstance().AddToWorkforce(result);
      return result;
   }

   private void Awake() {
      animator = GetComponent<Animator>();
   }

   public void SetDestination(Vector3? destination) {
      this.destination = destination;
   }

   private void Update() {
      if (DayCycleManager.GetInstance().IsNight()) {
         currentDestination = house.transform.position;
      } else {
         currentDestination = destination;
      }

      if (currentDestination == null) {
         animator.SetFloat("Turn", 0);
         animator.SetFloat("Forward", 0);
         return;
      }

      var forward2D = new Vector2(transform.forward.x, transform.forward.z);
      var direction = (currentDestination.Value - transform.position).normalized;
      var direction2D = new Vector2(direction.x, direction.z);
      var angleBetween = Vector2.Angle(forward2D, direction2D);
      var between = Quaternion.AngleAxis(angleBetween / 2, transform.up) * transform.forward;
      var angleBetween2 = Vector2.Angle(direction2D, new Vector2(between.x, between.z));
      var isRightTurn = angleBetween2 < angleBetween;

      animator.SetFloat("Turn", Mathf.Clamp(angleBetween / 15, 0, 1) * (isRightTurn ? 1 : -1));
      animator.SetFloat("Forward", Mathf.Clamp((currentDestination.Value - transform.position).magnitude * 10, 0, 1));
      if ((currentDestination.Value - transform.position).magnitude < deadZone || angleBetween > 15) {
         animator.SetFloat("Forward", 0);
      }

   }

   private Vector3 Convert(Vector2 input) {
      return new Vector3(input.x, 0, input.y);
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("destination", destination.HasValue ? new float[] { destination.Value.x, destination.Value.y, destination.Value.z } : null);
      data.Add("house", house.GetComponent<Saveable>().GetSavedIndex());
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("destination", out result)) {
         var coords = (float[])result;
         if (coords != null) {
            destination = new Vector3(coords[0], coords[1], coords[2]);
         }
      }
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("house", out result)) {
         house = SaveManager.GetInstance().FindLoadedInstanceBySaveIndex((int)result).GetComponent<House>();
      }
   }
}
