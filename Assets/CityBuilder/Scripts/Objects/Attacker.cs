using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Destructable))]
[RequireComponent(typeof(Walker))]
public class Attacker : MonoBehaviour {
   [SerializeField]
   private float range = 2;
   [SerializeField]
   private float minRange = 1.3f;
   [SerializeField]
   private float damage = 5;

   private Destructable destructableSelf;
   private Destructable target;
   private Coroutine attackCoroutine;

   // TODO: This spawns an attacker, not an enemy.  Change this to be generic and move enemy logic out.
   public static Attacker Instantiate(Vector3 position) {
      return Instantiate(ResourceLoader.GetInstance().Enemy, position, new Quaternion());
   }

   private void Start() {
      destructableSelf = GetComponent<Destructable>();
   }

   private void Update() {
      // No need to switch targets if we are already very close to the existing target
      if ((target == null || (target.transform.position - transform.position).magnitude > 3)) {
         target = FindObjectsOfType<Destructable>()
            .OrderBy(x => Vector3.Magnitude(x.transform.position - transform.position))
            .Where(x => x.enabled && x.GetTeam() != destructableSelf.GetTeam()).FirstOrDefault();
      }

      if (target == null) {
         GetComponent<Walker>().SetDestination(null);
         return;
      }

      var results = Physics.RaycastAll(transform.position + Vector3.up * 0.5f, target.transform.position - transform.position);
      var targetPoints = results.Where(x => x.transform.gameObject == target.gameObject);
      if (targetPoints.Count() > 0) {
            var basePoint = targetPoints.First().point - Vector3.up * 0.5f;
            var distance = basePoint - transform.position;
            var destination = basePoint - Vector3.ClampMagnitude(distance, range);
            GetComponent<Walker>().SetDestination(target.transform.position, (basePoint - target.transform.position).magnitude + range);
      } else {
         GetComponent<Walker>().SetDestination(target.transform.position, range);
      }

      if (GetComponent<Walker>().Arrived() && attackCoroutine == null) {
         attackCoroutine = StartCoroutine(Attack());
      }
   }

   private void OnDestroy() {
      if (destructableSelf.Health <= 0) {
         EnemySpawner.GetInstance().RemoveEnemy();
      }
   }

   private IEnumerator Attack() {
      while (GetComponent<Walker>().Arrived() && target != null) {
         GetComponent<Animator>().SetTrigger("Attack");
         yield return new WaitForSeconds(0.65f); // TODO: trigger off of animation
         if (target != null) {
            target.OffsetHealth(-damage);
         }
         yield return new WaitForSeconds(1); // TODO: Scale with game speed
      }
      attackCoroutine = null;
   }
}
