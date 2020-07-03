using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Teamable))]
[RequireComponent(typeof(Targeter))]
[RequireComponent(typeof(Destructable))]
[RequireComponent(typeof(Walker))]
public class Attacker : MonoBehaviour {

   [SerializeField]
   private float targettingRange = 50;
   [SerializeField]
   private float range = 2;
   [SerializeField]
   private float minRange = 1.3f;
   [SerializeField]
   private float damage = 5;

   private Destructable destructableSelf;
   private Coroutine attackCoroutine;

   // TODO: This spawns an attacker, not an enemy.  Change this to be generic and move enemy logic out.
   public static Attacker Instantiate(Vector3 position) {
      return Instantiate(ResourceLoader.GetInstance().Enemy, position, new Quaternion());
   }

   private void Start() {
      destructableSelf = GetComponent<Destructable>();
   }

   private void Update() {
      var targeter = GetComponent<Targeter>();
      if (targeter.target == null) {
         targeter.SetTarget(FindObjectsOfType<Waypoint>()
            .OrderBy(x => Vector3.Magnitude(x.transform.position - transform.position))
            .Where(x => targettingRange < 0 || Vector3.Magnitude(x.transform.position - transform.position) <= targettingRange)
            //.Where(x => x.enabled && x.GetTeam() != destructableSelf.GetTeam())
            .FirstOrDefault()
            .GetComponent<Targetable>());
      }

      if (targeter.target != null && targeter.target.GetComponent<Waypoint>() != null) {
         var enemyTargetingWaypoint = targeter.target
            .GetTargeters()
            .Where(x => x.GetComponent<Attacker>() != null)
            .Where(x => x.GetComponent<Targetable>() != null)
            .Where(x => GetComponent<Teamable>().IsHostileTo(x))
            .FirstOrDefault();

         if (enemyTargetingWaypoint != null) {
            targeter.SetTarget(enemyTargetingWaypoint.GetComponent<Targetable>());
         }
      }

      if (targeter.target != null) {
         var results = Physics.RaycastAll(transform.position + Vector3.up * 0.5f, targeter.target.transform.position - transform.position);
         var targetPoints = results.Where(x => x.transform.gameObject == targeter.target.gameObject);
         if (targetPoints.Count() > 0) {
               var basePoint = targetPoints.First().point - Vector3.up * 0.5f;
               var distance = basePoint - transform.position;
               var destination = basePoint - Vector3.ClampMagnitude(distance, range);
               GetComponent<Walker>().SetDestination(targeter.target.transform.position, (basePoint - targeter.target.transform.position).magnitude + range);
         } else {
            GetComponent<Walker>().SetDestination(targeter.target.transform.position, range);
         }

         if (attackCoroutine == null && GetComponent<Walker>().Arrived() && targeter.target.GetComponent<Destructable>() != null && GetComponent<Teamable>().IsHostileTo(targeter.target)) {
            attackCoroutine = StartCoroutine(Attack());
         }
      }
   }

   private void OnDestroy() {
      if (destructableSelf.Health <= 0 && EnemySpawner.GetInstance() != null) {
         EnemySpawner.GetInstance().RemoveEnemy();
      }
   }

   private IEnumerator Attack() {
      while (GetComponent<Walker>().Arrived() && GetComponent<Targeter>().target != null) {
         GetComponent<Animator>().SetTrigger("Attack");
         yield return new WaitForSeconds(0.65f); // TODO: trigger off of animation
         var target = GetComponent<Targeter>().target;
         if (target != null) {
            var destructableTarget = target.GetComponent<Destructable>();
            if (destructableTarget != null && GetComponent<Teamable>().IsHostileTo(target)) {
               destructableTarget.OffsetHealth(-damage);
            }
         }
         yield return new WaitForSeconds(1); // TODO: Scale with game speed
      }
      attackCoroutine = null;
   }
}
