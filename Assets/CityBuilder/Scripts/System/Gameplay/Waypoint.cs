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

      public void MoveNext() {
         if (waypoint != null && waypoint.connectedWaypoints[0] != null) {
            SetWaypoint(waypoint.connectedWaypoints[0]);
         }
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
   private List<Waypoint> connectedWaypoints;
   public List<Waypoint> GetConnectedWaypoints() { return connectedWaypoints;  }
   [SerializeField]
   private Renderer flag;

   private List<Attacker> attackers = new List<Attacker>();

   public List<Attacker> GetAttackers() {
      return new List<Attacker>(attackers);
   }

   public List<Attacker> GetAttackersInRange() {
      // TODO: This really needs to change.
      return attackers.Where(x => Vector3.Magnitude(x.transform.position - transform.position) < 5).ToList();
   }

   public void Charge(Waypoint target) {
      if (connectedWaypoints.Contains(target)) {
         GetAttackersInRange()
            .Where(x => x.GetComponent<Destructable>().GetTeam() == Team.Player) // TODO: Probably not a safe assumption to use player
            .ToList()
            .ForEach(x => x.SetWaypoint(target));
      }
   }

   private void Update() {
      var attackersInRange = GetAttackersInRange().ToList();
      if (attackersInRange.Count > 0) {
         var team = attackersInRange[0].GetComponent<Destructable>().GetTeam();
         if (attackersInRange.All(x => x.GetComponent<Destructable>().GetTeam() == team)) {
            flag.material.color = team == Team.Player ? Color.blue : Color.red;
         } else {
            flag.material.color = Color.yellow;
         }
      } else {
         flag.material.color = Color.white;
      }
   }

}
