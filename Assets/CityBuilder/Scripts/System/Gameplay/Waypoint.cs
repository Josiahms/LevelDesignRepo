using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Waypoint : MonoBehaviour {
    [SerializeField]
    private Waypoint up;
    public Waypoint Up { get { return up; }}
    [SerializeField]
    private Waypoint forward;
    public Waypoint Forward { get { return forward; }}
    [SerializeField]
    private Waypoint down;
    public Waypoint Down { get { return down; }}

    private List<Attacker> attackers;

    public void ChargeUp() {
        if (up != null) {
            
        }
    }

    public void ChargeForward() {

    }

    public void ChargeDown() {

    }

    public void AddSoldier() {
        /*var solider = FindObjectsOfType<Attacker>()
            .Where(x => x.GetTeam() == Team.Player)
            .FirstOrDefault();
        GetComponent<Assignable>().AddAssignee(soldier);*/
    }

    public void RemoveSoldier() {

    }
} 