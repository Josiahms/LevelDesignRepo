using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class WorkerAI : MonoBehaviour, ISaveable
{   
   [SerializeField]
   private float deadZone = 0.2f;

   private Animator animator;
   private Vector3? destination;

   private void Awake() {
      animator = GetComponent<Animator>();
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

      var forward2D = new Vector2(transform.forward.x, transform.forward.z);
      var direction = (destination.Value - transform.position).normalized;
      var direction2D = new Vector2(direction.x, direction.z);
      var angleBetween = Vector2.Angle(forward2D, direction2D);
      var between = Quaternion.AngleAxis(angleBetween / 2, transform.up) * transform.forward;
      var angleBetween2 = Vector2.Angle(direction2D, new Vector2(between.x, between.z));
      var isRightTurn = angleBetween2 < angleBetween;

      animator.SetFloat("Turn", Mathf.Clamp(angleBetween / 15, 0, 1) * (isRightTurn ? 1 : -1));
      animator.SetFloat("Forward", Mathf.Clamp((destination.Value - transform.position).magnitude * 10, 0, 1));
      if ((destination.Value - transform.position).magnitude < deadZone || angleBetween > 15) {
         animator.SetFloat("Forward", 0);
      }

   }

   private Vector3 Convert(Vector2 input) {
      return new Vector3(input.x, 0, input.y);
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("destination", destination.HasValue ? new float[] { destination.Value.x, destination.Value.y, destination.Value.z } : null);
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("destination", out result)) {
         var coords = (float[])result;
         if (coords != null) {
            destination = new Vector3(coords[0], coords[1], coords[2]);
         }
      }
   }
}
