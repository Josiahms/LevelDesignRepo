using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

   [SerializeField]
   private float damage = 5;
   [SerializeField]
   private float flightTime = 0.35f;
   [SerializeField]
   private float arc = 5;

   private Vector3 initialPosition;
   private Vector3 target;
   private float startTime;
   private Team team;

   public static Projectile Instantiate(Vector3 spawnPoint, Vector3 target, Team team) {
      var instance = Instantiate(ResourceLoader.GetInstance().Arrow, spawnPoint, new Quaternion());
      instance.startTime = Time.time;
      instance.initialPosition = spawnPoint;
      instance.target = target;
      instance.team = team;
      instance.transform.LookAt(target);
      Destroy(instance.gameObject, 1.2f);

      return instance;
   }

   private void Update() {
      var totalTime = (target - initialPosition).magnitude / flightTime;
      var t = Mathf.Min((Time.time - startTime) / flightTime, 1);
      var newPosition = Vector3.Lerp(initialPosition, target, t) + Vector3.up * Mathf.Lerp(0, arc, -4 * t * (t - 1)); // y = -4x(x - 1)
      var delta = newPosition - transform.position;

      transform.position = newPosition;
      transform.LookAt(newPosition + delta);
   }

   public void OnTriggerEnter(Collider other) {
      var destructable = other.GetComponent<Destructable>();
      if (destructable != null && destructable.GetTeam() != team) {
         destructable.OffsetHealth(-damage);
      }
   }
}