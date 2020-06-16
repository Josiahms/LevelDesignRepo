using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardAnimController : MonoBehaviour
{
   private Animator animator;

   private void Start() {
      animator = GetComponent<Animator>();
   }

   private void Update() {
      animator.SetFloat("Forward", Input.GetAxis("Vertical"));
      animator.SetFloat("Turn", Input.GetAxis("Horizontal"));
      if (Input.GetMouseButtonDown(0)) {
         animator.SetTrigger("Attack");
      }
   }
}
