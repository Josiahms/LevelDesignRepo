using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Destructable))]
[RequireComponent(typeof(Walker))]
public class Enemy : MonoBehaviour {

   [SerializeField]
   private float damage = 5;

   private Destructable destructableSelf;
   private Destructable target;
   private Coroutine attackCoroutine;

   public static Enemy Instantiate(Vector3 position) {
      return Instantiate(ResourceLoader.GetInstance().Enemy, position, new Quaternion());
   }

   private void Start() {
      destructableSelf = GetComponent<Destructable>();
   }

   private void Update() {
      if (target == null) {
         target = FindObjectsOfType<Destructable>().Where(x => x.enabled && x.GetTeam() != destructableSelf.GetTeam()).FirstOrDefault();
         if (target != null) {
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, target.transform.position - transform.position, out hitInfo)) {
               GetComponent<Walker>().SetDestination(hitInfo.point);
            } else {
               GetComponent<Walker>().SetDestination(target.transform.position);
            }
         } else {
            GetComponent<Walker>().SetDestination(null);
         }
      } else {
         if (GetComponent<Walker>().Arrived() && attackCoroutine == null) {
            attackCoroutine = StartCoroutine(Attack());
         }
      }
   }

   private void OnDestroy() {
      if (destructableSelf.Health <= 0) {
         EnemySpawner.GetInstance().RemoveEnemy();
      }
   }

   private IEnumerator Attack() {
      yield return new WaitForSeconds(1); // TODO: Scale with game speed
      if (GetComponent<Walker>().Arrived()) {
         target.OffsetHealth(-damage);
      }
      attackCoroutine = null;
   }
}
