using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

   [SerializeField]
   private int garrisonSize; // How many enemies will remain at a waypoint to defend
   [SerializeField]
   private int minAttackPartySize; // The minimum number of enemies to charge the next node

   private List<Waypoint> waypoints;

   private void Start() {
      waypoints = new List<Waypoint>(FindObjectsOfType<Waypoint>());
   }

   private void Update() {

      foreach(var waypoint in waypoints) {
         var attackers = waypoint.GetAttackers().Where(x => x.GetComponent<Destructable>().GetTeam() == Team.Enemy).ToList();
         var attackersInRange = waypoint.GetAttackersInRange().Where(x => x.GetComponent<Destructable>().GetTeam() == Team.Enemy).ToList();

         if (attackersInRange.Count() > garrisonSize + minAttackPartySize) {

            var nextWaypoint = waypoint.GetConnectedWaypoints()
               .OrderBy(x => x.GetAttackers().Where(y => y.GetComponent<Destructable>().GetTeam() == Team.Enemy).Count())
               .FirstOrDefault();

            if (nextWaypoint != null) {
               for (int i = 0; i < attackersInRange.Count() - garrisonSize; i++) {
                  attackersInRange[i].SetWaypoint(nextWaypoint);
               }
            }
         }
      }
   }

}
