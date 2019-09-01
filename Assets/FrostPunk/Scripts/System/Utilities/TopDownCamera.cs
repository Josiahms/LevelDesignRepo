using UnityEngine;
using System.Collections.Generic;

public enum CameraZoom { Close, Medium, Far, Free }

public class TopDownCamera : Singleton<TopDownCamera>, ISaveable {

   private enum Mode { Keyboard, Mouse }
   [SerializeField]
   private Mode mode;
   [SerializeField]
   private int screenEdgeSize = 10;
   [SerializeField]
   private float cameraMoveSpeed = 10;
   [SerializeField]
   private float zoomSpeed = 35;
   [SerializeField, Range(1, 500)]
   private float radius = 50;
   [SerializeField]
   private Transform center;

   private Vector3? target = null;
   private CameraZoom zoomLevel = CameraZoom.Free;

   public void SetTarget(Vector3 target, CameraZoom zoomLevel) {
      this.target = target;
      this.zoomLevel = zoomLevel;
   }

   private void Update() {

      if (target != null) {
         MoveToTarget();
      } else {
         if (mode == Mode.Mouse) {
            MoveWithMouse();
         } else {
            MoveWithKeyboard();
         }
         Zoom();
      }
   }

   private void Zoom() {
      if (zoomLevel == CameraZoom.Free) {
         var delta = Input.GetAxis("Mouse ScrollWheel");
         var distance = Camera.main.transform.position.y - center.position.y;
         if (delta < 0 && distance < 30) {
            Camera.main.transform.position += Camera.main.transform.forward * delta * Time.deltaTime * zoomSpeed;
         } else if (delta > 0 && distance > -30) {
            Camera.main.transform.position += Camera.main.transform.forward * delta * Time.deltaTime * zoomSpeed;
         }
      }
   }

   private Vector3 ClampAndRotate(Vector3 delta) {
      var rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
      RaycastHit hit;
      if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2)), out hit)) {

         var distance = rotation * new Vector3(hit.point.x, center.transform.position.y, hit.point.z) - center.transform.position;
         if (Mathf.Abs(distance.x) > radius && distance.x * delta.z < 0) {
            delta.z = 0;
         }

         if (Mathf.Abs(distance.z) > radius && distance.z * delta.x > 0) {
            delta.x = 0;
         }
      }
      return rotation * delta;
   }

   private void MoveWithKeyboard() {
      Camera.main.transform.position += ClampAndRotate(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")))  * cameraMoveSpeed * Time.deltaTime;
   }

   private void MoveWithMouse() {
      var mousePosition = Input.mousePosition;
      var moveAmount = cameraMoveSpeed * Time.deltaTime;
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

      Camera.main.transform.position += ClampAndRotate(deltaPosition);
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
