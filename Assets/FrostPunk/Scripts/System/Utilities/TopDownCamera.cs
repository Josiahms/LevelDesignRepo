using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCamera : Singleton<TopDownCamera>, ISaveable {

   [SerializeField]
   private int screenEdgeSize = 10;
   [SerializeField]
   private float cameraMoveSpeed = 10;

   void Update()
   {
      var mousePosition = Input.mousePosition;
      var moveAmount = cameraMoveSpeed * Time.deltaTime;
      var xComponent = moveAmount * Mathf.Sin(Camera.main.transform.rotation.eulerAngles.y * 0.01745f);
      var zComponent = moveAmount * Mathf.Cos(Camera.main.transform.rotation.eulerAngles.y * 0.01745f);

      if (mousePosition.x < screenEdgeSize) {
         Camera.main.transform.position += new Vector3(xComponent, 0, -zComponent);
      }

      if (mousePosition.y < screenEdgeSize) {
         Camera.main.transform.position += new Vector3(-xComponent, 0, -zComponent);
      }

      if (mousePosition.x > Screen.width - screenEdgeSize) {
         Camera.main.transform.position += new Vector3(-xComponent, 0, zComponent);
      }

      if (mousePosition.y > Screen.height - screenEdgeSize) {
         Camera.main.transform.position += new Vector3(xComponent, 0, zComponent);
      } 
   }

   public object OnSave() {
      return null;
   }

   public void OnLoad(object data) {
      // Ignored;
   }

   public void OnLoadDependencies(object data) {
      // Ignored;
   }
}
