using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class ThirdPersonController : MonoBehaviour
{
   [SerializeField]
   private float movementSpeed = 1;
   [SerializeField]
   private float lookSensitivity = 3;
   [SerializeField]
   private Transform cameraPivot;
   [SerializeField]
   private float groundCheckDistance = 1;
   [SerializeField]
   private float jumpForce = 2;
   [SerializeField]
   private Transform muzzle;
   [SerializeField]
   private Transform grip;
   [SerializeField]
   private Transform gun;
   [SerializeField]
   private Transform trigger;
   [SerializeField]
   private Transform rightHandAnchor;
   [SerializeField]
   private Transform headAnchor;
   [SerializeField]
   private LineRenderer bulletPath;

   private float internalMovementSpeed = 13;
   private float sprintMultiplier = 1.56f;
   private float adsMultiplier = 0.5f;
   private float backwardsMultiplier = 0.6f;
   private float horizontalMultiplier = 0.8f;

   private Animator animator;
   private Rigidbody rb;
   private bool grounded;
   private Vector3 realworldTarget;

   private Vector3 triggerPos;
   private Quaternion triggerRot;
   private Vector3 gripPos;
   private Quaternion gripRot;
   private float sprint;
   private AudioSource audioSource;


   private void Awake() {
      animator = GetComponent<Animator>();
      rb = GetComponent<Rigidbody>();
      audioSource = GetComponentInChildren<AudioSource>();
   }

   public void SetMovementParams(Vector2 direction, Vector2 rotation, bool crouch, float sprint, float ads) {
      this.sprint = sprint;
      float animRegulator;
      if (ads > 0 && sprint > 0) {
         animRegulator = (sprint - ads + 1) * 0.5f;
      } else if (ads > 0) {
         animRegulator = 0.5f;
      } else if (sprint > 0) {
         animRegulator = (sprint + 1) * 0.5f;
      } else {
         animRegulator = 0.5f;
      }

      animator.SetFloat("Forward", direction.x * animRegulator);
      animator.SetFloat("Horizontal", direction.y * animRegulator);
      animator.speed = 0.75f;

      if (direction.magnitude > 1) {
         direction = direction.normalized;
      }

      if (direction.x < 0) {
         direction = Vector2.Scale(direction, new Vector2(backwardsMultiplier, horizontalMultiplier));
      } else {
         direction = Vector2.Scale(direction, new Vector2(1, horizontalMultiplier));
      }

      sprint = sprint * (sprintMultiplier - 1) + 1;
      ads = ads * (adsMultiplier - 1) + 1;

      Vector3 movement = (transform.forward * direction.x + transform.right * direction.y);
      movement *= Time.deltaTime * internalMovementSpeed * sprint * ads * movementSpeed;

      rb.MovePosition(transform.position + movement);
      rb.MoveRotation(transform.rotation * Quaternion.Euler(0, rotation.x * lookSensitivity, 0));
      cameraPivot.transform.Rotate(rotation.y * lookSensitivity, 0, 0);
   }

   private void OnAnimatorIK(int layerIndex) {
      var forward = transform.forward - transform.right * 0.05f;
      var lookat = transform.position + forward * Vector3.Dot(realworldTarget - transform.position, forward);
      lookat = new Vector3(lookat.x, realworldTarget.y + 0.05f, lookat.z);

      if (sprint < 0.9f) {
         gun.transform.SetParent(headAnchor, false);
      } else {
         gun.transform.SetParent(rightHandAnchor, false);
      }

      animator.SetLookAtWeight(1 - sprint, 1, 1, 0);
      animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1 - sprint);
      animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1 - sprint);
      animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1 - sprint);
      animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1 - sprint);
      animator.SetLookAtPosition(lookat);
      animator.SetIKPosition(AvatarIKGoal.RightHand, triggerPos);
      animator.SetIKRotation(AvatarIKGoal.RightHand, triggerRot);
      animator.SetIKPosition(AvatarIKGoal.LeftHand, gripPos);
      animator.SetIKRotation(AvatarIKGoal.LeftHand, gripRot);
   }

   private void Update() {
      triggerPos = trigger.position;
      triggerRot = trigger.rotation;
      gripPos = grip.position;
      gripRot = grip.rotation;

      if (Input.GetMouseButton(0) && bulletPath.enabled == false && sprint < 0.1f) {
         StartCoroutine(Fire());
      }
   }

   private IEnumerator Fire() {
      audioSource.Play();
      bulletPath.enabled = true;
      bulletPath.SetPosition(0, muzzle.transform.position);
      bulletPath.SetPosition(1, realworldTarget + Random.insideUnitSphere * 0.5f);
      yield return new WaitForSeconds(0.18f);
      bulletPath.enabled = false;
   }

   public bool Jump() {
      if (IsGrounded()) {
         animator.SetTrigger("Jump");
         return true;
      }
      return false;
   }

   public void OnJumpUp() {
      rb.AddForce(Vector3.up * 10 * jumpForce);
   }

   private void FixedUpdate() {
      RaycastHit hitInfo;
      if (rb.velocity.y <= 0 && Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, groundCheckDistance)) {
         if (!grounded) {
            animator.SetTrigger("Land");
         } else {
            animator.ResetTrigger("Land");
         }
         grounded = true;
      } else {
         grounded = false;
      }

      var cameraRay = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2));
      if (Physics.Raycast(cameraRay, out hitInfo)) {
         realworldTarget = hitInfo.point;
      } else {
         realworldTarget = transform.position + cameraRay.direction * 10000f;
      }

      if (Input.GetKeyDown(KeyCode.K)) {
         Time.timeScale = Time.timeScale == 1 ? 0 : 1;
      }
   }

   private bool IsGrounded() {
      return grounded;
   }
}
