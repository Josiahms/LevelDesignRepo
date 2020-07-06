using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Church : MonoBehaviour {

   [SerializeField]
   private float range;
   [SerializeField]
   private float period = 10;
   [SerializeField]
   private float healAmount = 5;

   [SerializeField]
   private ParticleSystem ps;

   private void Start() {
      StartCoroutine(HealCycle());
   }

   private IEnumerator HealCycle() {
      while(true) {
         yield return new WaitForSeconds(period);
         // TODO: Make this more efficient?
         var thingsToHeal = FindObjectsOfType<Destructable>()
            .Where(x => Vector3.Magnitude(x.transform.position - transform.position) < range)
            .Where(x => x.GetComponent<Destructable>() != null && x.GetComponent<Destructable>().GetTeam() == GetComponent<Destructable>().GetTeam())
            .Where(x => x.gameObject != gameObject);

         foreach (var thingToHeal in thingsToHeal) {
            thingToHeal.OffsetHealth(healAmount);
         }
      }
   }

}
