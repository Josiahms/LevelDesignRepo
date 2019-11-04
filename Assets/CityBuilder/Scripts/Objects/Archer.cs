using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Archer : MonoBehaviour {

   [SerializeField]
   private float aimDelay = 1;
   [SerializeField]
   private Transform arrowSpawn;

   private Animator anim;
   private Enemy target;

   private void Start() {
      anim = GetComponent<Animator>();
      StartCoroutine(Arrow());
   }

   private void Update() {
      if (target == null) {
         var enemy = FindObjectOfType<Enemy>();
         if (enemy != null) {
            target = enemy;
         }
      } else {
         transform.LookAt(target.transform);
      }
   }



   private IEnumerator Arrow() {
      while (true) {
         yield return new WaitForSeconds(1.033f + aimDelay);
         if (target != null) {
            anim.SetTrigger("Fire");
            yield return new WaitForSeconds(0.1f);
            Projectile.Instantiate(arrowSpawn.position, target.transform.position + Vector3.up, Team.Player);
            yield return new WaitForSeconds(0.6f);
         }
      }
   }
}
