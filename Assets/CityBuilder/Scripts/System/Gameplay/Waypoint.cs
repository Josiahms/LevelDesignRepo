using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour {

   public class AttackerRelationship {

      public Waypoint waypoint { get { return _waypoint; } set { SetWaypoint(value); } }

      private Attacker attacker;
      private Waypoint _waypoint;

      public AttackerRelationship(Attacker attacker) {
         this.attacker = attacker;
      }

      private void SetWaypoint(Waypoint newWaypoint) {
         if (_waypoint != null) {
            _waypoint.attackers.Remove(attacker);
         }

         _waypoint = newWaypoint;

         if (newWaypoint != null) {
            newWaypoint.attackers.Add(attacker);
         }
      }

   }

   [SerializeField]
   private Waypoint nextWaypoint;

   private List<Attacker> attackers = new List<Attacker>();

   public List<Attacker> GetAttackers() {
      return new List<Attacker>(attackers);
   }

   private List<Attacker> GetAttackersInRange() {
      // TODO: This really needs to change.
      return attackers.Where(x => Vector3.Magnitude(x.transform.position - transform.position) < 5).ToList();
   }

   public void Charge() {
      GetAttackersInRange().ForEach(x => x.SetWaypoint(nextWaypoint));
   }

   private void Update() {
      var attackersInRange = GetAttackersInRange();
      // At least 4 attackers to charge and more than 8 assigned
      if (attackersInRange.Count >= 4  && attackers.Count > 8) {
         Charge();
      }

      if (attackersInRange.Count > 0) {
         var team = attackersInRange[0].GetComponent<Destructable>().GetTeam();
         if (attackersInRange.All(x => x.GetComponent<Destructable>().GetTeam() == team)) {
            GetComponent<Renderer>().material.color = team == Team.Player ? Color.blue : Color.red;
         } else {
            GetComponent<Renderer>().material.color = Color.yellow;
         }
      } else {
         GetComponent<Renderer>().material.color = Color.white;
      }
   }

}
