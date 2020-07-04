using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

   private float damage = 5;
   [SerializeField]
   private float velocity = 15;
   [SerializeField]
   private float arc = 3;

   private Vector3 initialPosition;
   private Vector3 target;
   private float startTime;
   private Team team;

   public static Projectile Instantiate(Projectile projectile, Vector3 spawnPoint, Vector3 target, Vector3 oneSecondDeltaPosition, Team team, float damage) {
      var instance = Instantiate(projectile, spawnPoint, new Quaternion());
      instance.startTime = DayCycleManager.GetInstance().CurrentTime;
      instance.initialPosition = spawnPoint;
      instance.target = target + oneSecondDeltaPosition * (target - spawnPoint).magnitude / instance.velocity / DayCycleManager.GetInstance().ClockMinuteRate;
      instance.team = team;
      instance.transform.LookAt(target);
      instance.damage = damage;
      Destroy(instance.gameObject, 1.2f);

      return instance;
   }

   private void Update() {
      var distance = (target - initialPosition).magnitude;
      var t = Mathf.Min((DayCycleManager.GetInstance().CurrentTime - startTime) * velocity / distance, 1);
      var newPosition = Vector3.Lerp(initialPosition, target, t) + Vector3.up * Mathf.Lerp(0, arc * distance / 30, -4 * t * (t - 1)); // y = -4x(x - 1)
      var delta = newPosition - transform.position;

      transform.position = newPosition;
      transform.LookAt(newPosition + delta);
   }

   public void OnTriggerEnter(Collider other) {
      var destructable = other.GetComponent<Destructable>();
      if (destructable != null && destructable.GetTeam() != team) {
         destructable.OffsetHealth(-damage);
         Destroy(gameObject);
      }
   }
}