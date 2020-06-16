using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Destructable))]
[RequireComponent(typeof(Walker))]
public class Attacker : MonoBehaviour {

   [SerializeField]
   private float damage = 5;

   private Destructable destructableSelf;
   private Destructable target;
   private Coroutine attackCoroutine;

   public static Attacker Instantiate(Vector3 position) {
      return Instantiate(ResourceLoader.GetInstance().Enemy, position, new Quaternion());
   }

   private void Start() {
      destructableSelf = GetComponent<Destructable>();
   }

   private void Update() {
      // No need to switch targets if we are already very close to the existing target
      if (target == null || (target.transform.position - transform.position).magnitude > 3) {
         target = FindObjectsOfType<Destructable>()
            .OrderBy(x => Vector3.Magnitude(x.transform.position - transform.position))
            .Where(x => x.enabled && x.GetTeam() != destructableSelf.GetTeam()).FirstOrDefault();
         if (target == null) {
            GetComponent<Walker>().SetDestination(null);
            return;
         }
      }

      RaycastHit hitInfo;
      if (Physics.Raycast(transform.position + Vector3.up * 0.5f, target.transform.position - transform.position, out hitInfo)) {
         if (hitInfo.collider.gameObject == target.gameObject) {
            var destination = hitInfo.point - Vector3.up * 0.5f - (target.transform.position - transform.position).normalized * (GetComponent<CapsuleCollider>().radius);
            if ((transform.position - destination).magnitude < 0.2f) {
               // Close enough
               GetComponent<Walker>().SetDestination(null);
               transform.LookAt(target.transform);
            } else {
               GetComponent<Walker>().SetDestination(destination);
            }
         } else {
            // Somethings in the way, but we'll just walk through it.
            GetComponent<Walker>().SetDestination(target.transform.position);
         }
      } else {
         GetComponent<Walker>().SetDestination(target.transform.position);
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
