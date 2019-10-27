using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Assignable))]
[RequireComponent(typeof(Builder))]
public class TownCenter : MonoBehaviour {

   [SerializeField]
   private Transform target;

   private void Start() {
      StartCoroutine(Arrow());
   }

   private void Update() {
      if (target == null) {
         var enemy = FindObjectOfType<Enemy>();
         if (enemy != null) {
            target = enemy.transform;
         }
      }
   }

   private IEnumerator Arrow() {
      while (true) {
         yield return new WaitForSeconds(5);
         if (target != null) {
            Projectile.Instantiate(transform.position, target.position);
         }
      }
   }

}
