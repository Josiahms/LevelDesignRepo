using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCamera : MonoBehaviour {

   [SerializeField]
   private int screenEdgeSize = 10;
   [SerializeField]
   private float cameraMoveSpeed = 10;

   void Update()
   {
      var mousePosition = Input.mousePosition;

      if (mousePosition.x < screenEdgeSize) {
         Camera.main.transform.position -= new Vector3(cameraMoveSpeed * Time.deltaTime, 0);
      }

      if (mousePosition.y < screenEdgeSize) {
         Camera.main.transform.position -= new Vector3(0, 0, cameraMoveSpeed * Time.deltaTime);
      }

      if (mousePosition.x > Screen.width - screenEdgeSize) {
         Camera.main.transform.position += new Vector3(cameraMoveSpeed * Time.deltaTime, 0);
      }

      if (mousePosition.y > Screen.height - screenEdgeSize) {
         Camera.main.transform.position += new Vector3(0, 0, cameraMoveSpeed * Time.deltaTime);
      } 
   }
}
