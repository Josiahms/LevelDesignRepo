using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(ThirdPersonController))]
public class ThirdPersonPlayerInput : MonoBehaviour
{
   private ThirdPersonController thirdPersonController;
   private float timeZoomed;

   private void Awake() {
      thirdPersonController = GetComponent<ThirdPersonController>();
      Cursor.visible = false;
      Cursor.lockState = CursorLockMode.Locked;
   }

   private void FixedUpdate() {
      float mouseX = CrossPlatformInputManager.GetAxis("Mouse X");
      float mouseY = CrossPlatformInputManager.GetAxis("Mouse Y");
      float keyboardForward = CrossPlatformInputManager.GetAxis("Vertical");
      float keyboardSideways = CrossPlatformInputManager.GetAxis("Horizontal");
      float sprint = CrossPlatformInputManager.GetAxis("Sprint");
      float ads = CrossPlatformInputManager.GetAxis("Fire2");

      bool crouch = Input.GetKey(KeyCode.C);

      thirdPersonController.SetMovementParams(new Vector2(keyboardForward, keyboardSideways), new Vector2(mouseX, -mouseY), crouch, sprint, ads);

      Camera.main.fieldOfView = 60 - 15 * ads;

      if (Input.GetKeyDown(KeyCode.Space)) {
         thirdPersonController.Jump();
      }
   }
}
