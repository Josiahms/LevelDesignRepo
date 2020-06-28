using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team { Player, Enemy }

public class Teamable : MonoBehaviour {

    [SerializeField]
    private Team team;

    public bool IsHostileTo(Component other) {
        var otherTeamable = other.GetComponent<Teamable>();
        if (otherTeamable != null) {
            return otherTeamable.team != team;
        }
        return false;
    }

}
