using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
   private void Update() {
      transform.LookAt(Camera.main.transform, Camera.main.transform.up);
      transform.Rotate(transform.up, 180, Space.World);
   }
}
