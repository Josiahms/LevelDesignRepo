using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Catapult : MonoBehaviour {

   [SerializeField]
   private float spread = 2.5f;
   [SerializeField]
   private Projectile rockPrefab;
   [SerializeField]
   private List<Transform> rockSpawns;

   [SerializeField]
   private float attackRange;
   [SerializeField]
   private float fleeRange;

   [SerializeField]
   private Transform target;

   private Animator animator;

   private void Start() {
      animator = GetComponent<Animator>();
   }

   private void Update() {
      var distanceToTarget = Vector3.Magnitude(target.position - transform.position);

      animator.SetFloat("Turn", 0);
      animator.SetFloat("Forward", 0);

      var angleToTarget = Vector3.Angle(transform.forward, target.position - transform.position);
      var rightAngleToTarget = Vector3.Angle(transform.right, target.position - transform.position);

      if (distanceToTarget < fleeRange) {
         // TODO: Flee
      } else if (distanceToTarget < attackRange) {
         if (angleToTarget > 1) {
            // Target while in range
            animator.SetFloat("Turn", rightAngleToTarget < 90 ? 1 : -1);
         } else {
            animator.SetTrigger("Attack");
         }
      } else {
         if (angleToTarget > 1) {
            // Get on course
            animator.SetFloat("Turn", rightAngleToTarget < 90 ? 1 : -1);
         } else {
            // Move forward
            animator.SetFloat("Forward", 1);
            
         }
      }
   }

   public void DoneReloading() {
      foreach(var transform in rockSpawns) {
         transform.gameObject.SetActive(true);
      }
   }

   public void LaunchRocks() {
      foreach (var transform in rockSpawns) {
         transform.gameObject.SetActive(false);
         var offset = Random.insideUnitCircle * spread;
         Projectile.Instantiate(ResourceLoader.GetInstance().Rock, transform.position, target.transform.position + new Vector3(offset.x, 0, offset.y), Vector3.zero, Team.Player, 25);
      }
   }

}
