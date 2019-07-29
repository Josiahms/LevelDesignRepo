using UnityEngine;

public class TopDownCamera : Singleton<TopDownCamera>, ISaveable {

   [SerializeField]
   private int screenEdgeSize = 10;
   [SerializeField]
   private float cameraMoveSpeed = 10;
   [SerializeField, Range(1, 500)]
   private float radius = 50;
   [SerializeField]
   private Transform center;

   void Update()
   {
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
