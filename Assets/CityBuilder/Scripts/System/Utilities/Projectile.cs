using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour {

   [SerializeField]
   private float damage = 5;
   [SerializeField]
   private float initalVelocity = 20;


   public static Projectile Instantiate(Vector3 spawnPoint, Vector3 target) {
      var instance = Instantiate(ResourceLoader.GetInstance().Arrow, spawnPoint, new Quaternion());

      var angle = Mathf.Asin(((target - spawnPoint).magnitude * Physics.gravity.magnitude) / (instance.initalVelocity * instance.initalVelocity)) / 2;
      var velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
      velocity = Quaternion.AngleAxis(Vector3.SignedAngle(new Vector3(1, 0, 0), target - spawnPoint, Vector3.up), Vector3.up) * velocity;

      instance.GetComponent<Rigidbody>().velocity = velocity * instance.initalVelocity;
      Destroy(instance.gameObject, 10f);

      return instance;
   }

   private void FixedUpdate() {
      var rb = GetComponent<Rigidbody>();
      if (rb.velocity.magnitude > 0.4f) {
         transform.rotation = Quaternion.LookRotation(rb.velocity);
      }
   }

   private void OnTriggerEnter(Collider collider) {
      var destructable = collider.gameObject.GetComponent<Destructable>();
      if (destructable != null) {
         destructable.OffsetHealth(-damage);
         Destroy(gameObject);
      }
   }
}
