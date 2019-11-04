using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : MonoBehaviour {

   [SerializeField]
   private float deadZone = 0.5f;

   private Animator animator;
   [SerializeField]
   private Vector3? destination;
   public Vector3 OneSecondDeltaPosition { get; private set; }

   private void Awake() {
      animator = GetComponent<Animator>();
      StartCoroutine(DeltaMonitor());
   }

   public bool Arrived() {
      return destination != null && (transform.position - destination.Value).magnitude <= deadZone;
   }

   public void SetDestination(Vector3? destination) {
      this.destination = destination;
   }

   private void Update() {
      if (destination == null) {
         animator.SetFloat("Turn", 0);
         animator.SetFloat("Forward", 0);
         return;
      }

      // 1 is normal speed;
      var speed = DayCycleManager.GetInstance().ClockMinuteRate / 5;
      if (speed <= 2) {
         AnimatedWalk(speed);
      } else {
         animator.SetFloat("Turn", 0);
         animator.SetFloat("Forward", 0);
         TeleportWalk(speed);
      }
   }

   private void TeleportWalk(float speed) {
      transform.LookAt(new Vector3(destination.Value.x, transform.position.y, destination.Value.z));
      var distance = destination - transform.position;
      transform.position += Vector3.ClampMagnitude(distance.Value, speed * Time.deltaTime * 10);
   }

   private void AnimatedWalk(float speed) {
      var forward2D = new Vector2(transform.forward.x, transform.forward.z);
      var direction = (destination.Value - transform.position).normalized;
      var direction2D = new Vector2(direction.x, direction.z);
      var angleBetween = Vector2.Angle(forward2D, direction2D);
      var between = Quaternion.AngleAxis(angleBetween / 2, transform.up) * transform.forward;
      var angleBetween2 = Vector2.Angle(direction2D, new Vector2(between.x, between.z));
      var isRightTurn = angleBetween2 < angleBetween;

      animator.speed = speed;
      animator.SetFloat("Turn", Mathf.Clamp(angleBetween / 15, 0, 1) * (isRightTurn ? 1 : -1));
      animator.SetFloat("Forward", Mathf.Clamp((destination.Value - transform.position).magnitude * 10, 0, 1));
      if ((destination.Value - transform.position).magnitude < deadZone || angleBetween > 15) {
         animator.SetFloat("Forward", 0);
      }
   }

   private IEnumerator DeltaMonitor() {
      var prevPosition = transform.position;
      while (true) {
         OneSecondDeltaPosition = (transform.position - prevPosition) * 4;
         prevPosition = transform.position;
         yield return new WaitForSeconds(0.25f);
      }
   }
}
