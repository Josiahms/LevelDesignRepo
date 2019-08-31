using UnityEngine;
using System.Collections.Generic;

public enum CameraZoom { Close, Medium, Far }

public class TopDownCamera : Singleton<TopDownCamera>, ISaveable {
   [SerializeField]
   private int screenEdgeSize = 10;
   [SerializeField]
   private float cameraMoveSpeed = 10;
   [SerializeField, Range(1, 500)]
   private float radius = 50;
   [SerializeField]
   private Transform center;

   private Vector3? target = null;
   private CameraZoom zoomLevel = CameraZoom.Far;

   public void SetTarget(Vector3 target, CameraZoom zoomLevel) {
      this.target = target;
      this.zoomLevel = zoomLevel;
   }

   private void Update() {

      if (target != null) {
         MoveToTarget();
      } else {
         MoveWithMouse();
      }
   }

   private void MoveWithMouse() {
      var mousePosition = Input.mousePosition;
      var moveAmount = cameraMoveSpeed * Time.deltaTime;
      var rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
      var distance = rotation * (Camera.main.transform.position - center.transform.position);

      var deltaPosition = Vector3.zero;

      if (mousePosition.x < screenEdgeSize) {
         deltaPosition += new Vector3(-moveAmount, 0);
      }

      if (mousePosition.y < screenEdgeSize) {
         deltaPosition += new Vector3(0, 0, -moveAmount);
      }

      if (mousePosition.x > Screen.width - screenEdgeSize) {
         deltaPosition += new Vector3(moveAmount, 0);
      }

      if (mousePosition.y > Screen.height - screenEdgeSize) {
         deltaPosition += new Vector3(0, 0, moveAmount);
      }

      if (Mathf.Abs(distance.x) > radius && distance.x * deltaPosition.z < 0) {
         deltaPosition.z = 0;
      }

      if (Mathf.Abs(distance.z) > radius && distance.z * deltaPosition.x > 0) {
         deltaPosition.x = 0;
      }

      Camera.main.transform.position += rotation * deltaPosition;
   }


   private void MoveToTarget() {
      Vector3 destination;
      switch (zoomLevel) {
         case CameraZoom.Close:
            Camera.main.orthographicSize += (15 - Camera.main.orthographicSize) / 10;
            break;
         case CameraZoom.Medium:
            Camera.main.orthographicSize += (20 - Camera.main.orthographicSize) / 10;
            break;
         case CameraZoom.Far:
            Camera.main.orthographicSize += (35 - Camera.main.orthographicSize) / 10;
            break;
         default:
            Camera.main.orthographicSize += (35 - Camera.main.orthographicSize) / 10;
            break;
      }
      destination = target.Value - Camera.main.transform.forward * 100;
      Camera.main.transform.position += (destination - Camera.main.transform.position) / 10;
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("target", target == null ? null : new float[] { target.Value.x, target.Value.y, target.Value.z });
      data.Add("zoomLevel", zoomLevel);
      return data;
   }

   public void OnLoad(object data) {
      var savedData = (Dictionary<string, object>)data;
      target = savedData["target"] != null ? new Vector3?(new Vector3(((float[])savedData["target"])[0], ((float[])savedData["target"])[1], ((float[])savedData["target"])[2])) : null;
      zoomLevel = (CameraZoom)savedData["zoomLevel"];
   }

   public void OnLoadDependencies(object data) {
      // Ignored;
   }
}
