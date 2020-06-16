﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Archer : MonoBehaviour {

   [SerializeField]
   private float aimDelay = 1;
   [SerializeField]
   private Transform arrowSpawn;

   private Animator anim;
   private Attacker target;

   private void Start() {
      anim = GetComponent<Animator>();
      StartCoroutine(Arrow());
   }

   private void Update() {
      if (target == null) {
         // TODO: The archer should know it's team, the concept of teams needs to be refactored.
         var enemy = FindObjectsOfType<Attacker>().Where(x => x.GetComponent<Destructable>().GetTeam() != transform.parent.GetComponent<Destructable>().GetTeam()).FirstOrDefault();
         if (enemy != null) {
            target = enemy;
         }
      } else {
         transform.LookAt(target.transform);
      }
      anim.speed = DayCycleManager.GetInstance().ClockMinuteRate / 5;
   }

   private IEnumerator Arrow() {
      while (true) {
         var dcm = DayCycleManager.GetInstance();
         var curTime = dcm.CurrentTime;
         yield return new WaitUntil(() => dcm.CurrentTime > curTime + (1.033f + aimDelay));
         if (target != null) {
            anim.SetTrigger("Fire");
            curTime = dcm.CurrentTime;
            yield return new WaitUntil(() => dcm.CurrentTime > curTime + 0.5f);
            Projectile.Instantiate(arrowSpawn.position, target.transform.position + Vector3.up, target.GetComponent<Walker>().OneSecondDeltaPosition, Team.Player);
            curTime = dcm.CurrentTime;
            yield return new WaitUntil(() => dcm.CurrentTime > curTime + 3);
         }
      }
   }
}
