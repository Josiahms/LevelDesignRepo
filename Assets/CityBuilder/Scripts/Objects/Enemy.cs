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

   private void Start() {
      destructableSelf = GetComponent<Destructable>();
   }

   private void Update() {
      if (target == null) {
         target = FindObjectsOfType<Destructable>().Where(x => x.enabled && x.GetTeam() != destructableSelf.GetTeam()).FirstOrDefault();
      } else {
         if (IsInRange() && attackCoroutine == null) {
            attackCoroutine = StartCoroutine(Attack());
         }
         GetComponent<Walker>().SetDestination(target.transform);
      }
   }

   private IEnumerator Attack() {
      yield return new WaitForSeconds(1); // TODO: Scale with game speed
      if (IsInRange()) {
         target.OffsetHealth(-damage);
      }
      attackCoroutine = null;
   }

   private bool IsInRange() {
      if (target == null) {
         return false;
      }
      return (transform.position - target.transform.position).magnitude < 2;
   }
}
