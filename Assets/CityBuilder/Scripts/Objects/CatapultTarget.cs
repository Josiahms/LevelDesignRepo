using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultTarget : MonoBehaviour {

   private int numEnemies;

   public void OnTriggerStay(Collider other) {
      var attacker = other.gameObject.GetComponent<Destructable>();
      // TODO: This should be written to allow for enemy catapults too
      if (attacker != null && attacker.GetTeam() == Team.Enemy) {
         numEnemies++;
      }
   }

   private void FixedUpdate() {
      numEnemies = 0;
   }

   public bool HasTargets() {
      return numEnemies > 0;
   }

}
