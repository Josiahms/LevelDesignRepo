using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class SnapToCircleGrid : MonoBehaviour, ISaveable  {

   private Vector3? center;
   private float prevDistance;
   private Dictionary<MeshFilter, Vector3[]> sharedMeshStore = new Dictionary<MeshFilter, Vector3[]>();
   private List<JobHandleMesh> handles = new List<JobHandleMesh>();
   private int minNumber;

   private void Start() {
      prevDistance = float.MaxValue;
   }

   public int GetMinNumber() {
      return minNumber;
   }

   public Vector3? GetCenter() {
      return center;
   }

   public void SetCenter(int minNumber, Vector3? center) {
      this.minNumber = minNumber;
      this.center = center;
      Update();
   }

   private void Update() {

      if (!center.HasValue) {
         return;
      }

      transform.position = ToGrid(transform.position);
      transform.LookAt(new Vector3(center.Value.x, transform.position.y, center.Value.z));

      var currentDistance = (transform.position - center.Value).magnitude;
      if (Mathf.Abs(currentDistance - prevDistance) > 0.5f) {
         DeformMeshAndContinue(transform, transform.position);
         prevDistance = currentDistance;
      }

      foreach(var handle in handles) {
         handle.handle.Complete();
         handle.meshFilter.mesh.vertices = handle.job.displacedVertices.ToArray();
         handle.job.originalVerticies.Dispose();
         handle.job.displacedVertices.Dispose();
      }
      handles.Clear();
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
         var job = new DeformJob(center.Value, sharedMeshStore[meshFilter], meshTransform, rootPosition);
         handles.Add(new JobHandleMesh(job.Schedule(), job, meshFilter));
      }

      for (int i = 0; i < meshTransform.childCount; i++) {
         DeformMeshAndContinue(meshTransform.GetChild(i), rootPosition);
      }
   }

   private Vector3 ToGrid(Vector3 input) {
      const float INPUT_OFFSET = 1.7f;  // Aprox half the distance between radii to offset grid snapping
      const float BUILDING_WIDTH = 7.5f;
      const float TWO_PI = Mathf.PI * 2;
      const int LCD_BUILDINGS = 4; // There are 4, 8, 12... buildings in each circle.  4 is the LCD.

      var center = Vector3.Scale(this.center.Value, new Vector3(1, 0, 1));
      var distance = Vector3.Scale(input, new Vector3(1, 0, 1)) - center;
      var radiusAtMouse = distance.magnitude + INPUT_OFFSET;

      // Circumference = 2 * PI * r
      // Circumference / BuildingWidth = # of buildings that can fit at that radius (not necessarily a whole number)
      var numBuildingsAtRadius = TWO_PI * radiusAtMouse / BUILDING_WIDTH;
      var numBuildingsSnapped = SnapFloat(numBuildingsAtRadius, minNumber, LCD_BUILDINGS);
      var radius = numBuildingsSnapped * BUILDING_WIDTH / TWO_PI;
      var currentAngle = Vector3.SignedAngle(new Vector3(1, 0, 0), distance, Vector3.down);
      var angleIncrement = 360f / numBuildingsSnapped;
      var snappedAngle = Mathf.Floor((currentAngle + angleIncrement / 2f) / angleIncrement) * angleIncrement;
      var snappedAngleVect = new Vector3(Mathf.Cos(Mathf.Deg2Rad * snappedAngle), 0, Mathf.Sin(Mathf.Deg2Rad * snappedAngle));
      var result = center + radius * snappedAngleVect;

      return new Vector3(result.x, input.y, result.z);
   }

   private int SnapFloat(float num, int min, int lowestCommonDenominator) {
      var intValue = (int)Mathf.Ceil(num) / lowestCommonDenominator * lowestCommonDenominator;
      return Mathf.Max(min, intValue);
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("center", center.HasValue ? new float[] { center.Value.x, center.Value.y, center.Value.z } : null);
      data.Add("minNumber", minNumber);
      return data;
   }

   public void OnLoad(object data) {
      var savedData = (Dictionary<string, object>)data;
      var centerData = savedData["center"];
      if (centerData != null) {
         var centerArray = (float[])savedData["center"];
         center = new Vector3(centerArray[0], centerArray[1], centerArray[2]);
      }
      minNumber = (int)savedData["minNumber"];
   }

   public void OnLoadDependencies(object data) {
      // Ignored
   }
}

public struct JobHandleMesh {
   public JobHandle handle;
   public DeformJob job;
   public MeshFilter meshFilter;

   public JobHandleMesh(JobHandle handle, DeformJob job, MeshFilter meshFilter) {
      this.handle = handle;
      this.job = job;
      this.meshFilter = meshFilter;
   }
}

public struct DeformJob : IJob {

   public NativeArray<Vector3> displacedVertices;
   public NativeArray<Vector3> originalVerticies;

   private Vector3 center;
   private Matrix4x4 localToWorldMatrix;
   private Matrix4x4 worldToLocalMatrix;
   private Vector3 rootPosition;

   public DeformJob(Vector3 center, Vector3[] originalVerticies, Transform parent, Vector3 rootPosition) {
      this.center = center;
      this.originalVerticies = new NativeArray<Vector3>(originalVerticies, Allocator.TempJob);
      displacedVertices = new NativeArray<Vector3>(originalVerticies.Length, Allocator.TempJob);
      localToWorldMatrix = parent.localToWorldMatrix;
      worldToLocalMatrix = parent.worldToLocalMatrix;
      this.rootPosition = rootPosition;
   }

   public void Execute() {
      for (int i = 0; i < originalVerticies.Length; i++) {
         var absolutePosition = localToWorldMatrix.MultiplyPoint(originalVerticies[i]);
         var relativePosition = worldToLocalMatrix.MultiplyPoint(GetNewPosition(absolutePosition, rootPosition, center));
         displacedVertices[i] = relativePosition;
      }
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

}