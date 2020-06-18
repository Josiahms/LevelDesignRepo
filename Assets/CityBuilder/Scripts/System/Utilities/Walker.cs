﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : MonoBehaviour, ISaveable {

   [SerializeField]
   private float deadZone = 0.5f;
   [SerializeField]
   private bool debug;

   private Animator animator;
   private Vector3? destination;
   private float? offset;
   public Vector3 OneSecondDeltaPosition { get; private set; }

   private void Awake() {
      animator = GetComponent<Animator>();
      StartCoroutine(DeltaMonitor());
   }

   public bool Arrived() {
      return destination != null && (transform.position - destination.Value).magnitude <= offset;
   }

   public Vector3? GetDestination() {
      return destination;
   }

   public void SetDestination(Vector3? destination, float? offset = null) {
      if (destination.HasValue) {
         this.destination = destination;
      } else {
         this.destination = transform.position;
      }
      this.offset = offset;
   }

   private void Update() {
      if (destination == null) {
         animator.SetFloat("Turn", 0);
         animator.SetFloat("Forward", 0);
         return;
      }

      if (debug) {
         Debug.DrawLine(destination.Value, destination.Value + Vector3.up);
      }

      // 1 is normal speed;
      var speed = DayCycleManager.GetInstance().ClockMinuteRate / 5;
      var inDeadZone = (destination.Value - transform.position).magnitude < deadZone;
      AnimatedWalk(speed);
      /*if (speed <= 2 && !inDeadZone) {
         
      } else {
         animator.SetFloat("Turn", 0);
         animator.SetFloat("Forward", 0);
         //TeleportWalk(speed);
      }*/
   }

   private void TeleportWalk(float speed) {
      transform.LookAt(new Vector3(destination.Value.x, transform.position.y, destination.Value.z));
      var distance = destination - transform.position;
      transform.position += Vector3.ClampMagnitude(distance.Value, speed * Time.deltaTime * 10);
   }

   private void AnimatedWalk(float speed) {
      var forward2D = new Vector2(transform.forward.x, transform.forward.z);
      var right2D = new Vector2(transform.right.x, transform.right.z);
      var direction = destination.Value - transform.position;
      var direction2D = new Vector2(direction.x, direction.z).normalized;
      var angleBetween = Vector2.Angle(forward2D, direction2D);
      var angleBetween2 = Vector2.Angle(right2D, direction2D);
      var isRightTurn = angleBetween2 < 90;

      var turn = Mathf.Clamp(angleBetween / 15, 0, 1) * (isRightTurn ? 1 : -1);
      var forward = Mathf.Clamp((destination.Value - transform.position).magnitude - offset.GetValueOrDefault(0) + 0.1f, 0, 1);

      if (angleBetween > 15) {
         forward = 0;
      }

      if (debug) {
         Debug.Log(forward + ", " + turn);
      }

      animator.speed = speed;
      animator.SetFloat("Turn", turn);
      animator.SetFloat("Forward", forward);
   }

   private IEnumerator DeltaMonitor() {
      var prevPosition = transform.position;
      while (true) {
         OneSecondDeltaPosition = (transform.position - prevPosition) * 4;
         prevPosition = transform.position;
         yield return new WaitForSeconds(0.25f);
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("destination", destination.HasValue ? new float[] { destination.Value.x, destination.Value.y, destination.Value.z } : null);
      return data;
   }

   public void OnLoad(object data) {
      var savedData = (Dictionary<string, object>)data;
      var dest = (float[])savedData["destination"];
      if (dest != null) {
         destination = new Vector3(dest[0], dest[1], dest[2]);
      }
   }

   public void OnLoadDependencies(object savedData) {
      // Not used
   }
}
