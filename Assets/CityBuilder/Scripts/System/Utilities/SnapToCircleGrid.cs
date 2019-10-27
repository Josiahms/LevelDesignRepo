using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapToCircleGrid : MonoBehaviour, ISaveable  {

   private Vector3? center;
   private float prevDistance;
   private Dictionary<MeshFilter, Vector3[]> sharedMeshStore = new Dictionary<MeshFilter, Vector3[]>();

   private void Start() {
      prevDistance = float.MaxValue;
   }

   public void SetCenter(Vector3 center) {
      this.center = center;
      Update();
   }

   private void Update() {

      if (!center.HasValue) {
         return;
      }

      transform.position = ToGrid(transform.position);
      transform.LookAt(center.Value);

      var currentDistance = (transform.position - center.Value).magnitude;
      if (Mathf.Abs(currentDistance - prevDistance) > 0.5f) {
         DeformMeshAndContinue(transform, transform.position);
         prevDistance = currentDistance;
      }
   }

   private void DeformMeshAndContinue(Transform meshTransform, Vector3 rootPosition) {
      if (meshTransform == null) {
         return;
      }

      var meshFilter = meshTransform.GetComponent<MeshFilter>();
      if (meshFilter != null) {
         if (!sharedMeshStore.ContainsKey(meshFilter)) {
            sharedMeshStore.Add(meshFilter, meshFilter.sharedMesh.vertices);
         }
         meshFilter.mesh.vertices = DeformMesh(sharedMeshStore[meshFilter], meshTransform, rootPosition);
      }

      for (int i = 0; i < meshTransform.childCount; i++) {
         DeformMeshAndContinue(meshTransform.GetChild(i), rootPosition);
      }
   }

   private Vector3[] DeformMesh(Vector3[] originalVerticies, Transform meshTransform, Vector3 rootPosition) {
      Vector3[] displacedVertices = new Vector3[originalVerticies.Length];
      for (int i = 0; i < originalVerticies.Length; i++) {
         var absolutePosition = meshTransform.localToWorldMatrix.MultiplyPoint(originalVerticies[i]);
         var relativePosition = meshTransform.worldToLocalMatrix.MultiplyPoint(GetNewPosition(absolutePosition, rootPosition, center.Value));
         displacedVertices[i] = relativePosition;
      }
      return displacedVertices;
   }

   private Vector3 GetNewPosition(Vector3 point, Vector3 meshCenter, Vector3 pivot) {
      var originalY = point.y;

      point = Vector3.Scale(point, new Vector3(1, 0, 1));
      meshCenter = Vector3.Scale(meshCenter, new Vector3(1, 0, 1));
      pivot = Vector3.Scale(pivot, new Vector3(1, 0, 1));

      var circleRadius = meshCenter - pivot;

      var angle = Vector3.SignedAngle(circleRadius, point - pivot, Vector3.down);
      var horizontalOffset = (point - pivot).magnitude * Mathf.Sin(Mathf.Deg2Rad * angle);
      var radiusAtPoint = (point - pivot).magnitude * Mathf.Cos(Mathf.Deg2Rad * angle);

      var arcLengthRatio = horizontalOffset / circleRadius.magnitude;
      var newPosition = new Vector3(Mathf.Cos(arcLengthRatio), 0, Mathf.Sin(arcLengthRatio)) * radiusAtPoint;

      var result = Quaternion.FromToRotation(Vector3.right, circleRadius) * newPosition + pivot;
      return new Vector3(result.x, originalY, result.z);
   }

   private Vector3 ToGrid(Vector3 input) {
      const float BUILDING_WIDTH = 7.5f;
      const float TWO_PI = Mathf.PI * 2;
      const int MIN_NUMBER = 8;
      const int HALF_MIN_NUMBER = MIN_NUMBER / 2;

      var center = Vector3.Scale(this.center.Value, new Vector3(1, 0, 1));
      var distanceVect = Vector3.Scale(input, new Vector3(1, 0, 1)) - center;
      var numBuildingsInCircle = (int)Mathf.Max(Mathf.Ceil((distanceVect.magnitude + 1.909f) / (BUILDING_WIDTH / TWO_PI)), MIN_NUMBER);
      var numBuildingsSnapped = (numBuildingsInCircle / HALF_MIN_NUMBER * HALF_MIN_NUMBER);
      var radius = numBuildingsSnapped * BUILDING_WIDTH / TWO_PI;

      var currentAngle = Vector3.SignedAngle(new Vector3(1, 0, 0), distanceVect, Vector3.down);
      var angleIncrement = 360f / numBuildingsSnapped;
      var snappedAngle = Mathf.Floor((currentAngle + angleIncrement / 2f) / angleIncrement) * angleIncrement;

      var snappedAngleVect = new Vector3(Mathf.Cos(Mathf.Deg2Rad * snappedAngle), 0, Mathf.Sin(Mathf.Deg2Rad * snappedAngle));

      var result = center + radius * snappedAngleVect;

      return new Vector3(result.x, input.y, result.z);
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("center", center.HasValue ? new float[] { center.Value.x, center.Value.y, center.Value.z } : null);
      return data;
   }

   public void OnLoad(object data) {
      var savedData = (Dictionary<string, object>)data;
      var centerData = savedData["center"];
      if (centerData != null) {
         var centerArray = (float[])savedData["center"];
         center = new Vector3(centerArray[0], centerArray[1], centerArray[2]);
      }
   }

   public void OnLoadDependencies(object data) {
      // Ignored
   }
}