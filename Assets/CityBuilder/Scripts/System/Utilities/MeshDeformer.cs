using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDeformer : MonoBehaviour {

   [SerializeField]
   private Transform rootObject;

   private Dictionary<Transform, Vector3[]> originalVerticiesStore;
   private Vector3 prevDistanceVect;

   private void Start() {
      originalVerticiesStore = new Dictionary<Transform, Vector3[]>();
      InitializeAndContinue(rootObject);
   }

   private void Update() {
      var distanceVect = transform.position - rootObject.transform.position;
      if ((distanceVect - prevDistanceVect).magnitude > 0.1f) {
         DeformMeshAndContinue(rootObject);
         prevDistanceVect = distanceVect;
      }
   }

   private Mesh GetMesh(MeshFilter meshFilter) {
      if (Application.isPlaying) {
         return meshFilter.mesh;
      } else {
         return meshFilter.sharedMesh;
      }

   }

   private void InitializeAndContinue(Transform meshTransform) {
      if (meshTransform == null) {
         return;
      }

      var meshFilter = meshTransform.GetComponent<MeshFilter>();
      if (meshFilter != null) {
         var mesh = GetMesh(meshFilter);
         var originalVerticies = new Vector3[mesh.vertices.Length];
         for (int i = 0; i < originalVerticies.Length; i++) {
            originalVerticies[i] = mesh.vertices[i];
         }

         originalVerticiesStore.Add(meshTransform, originalVerticies);
      }

      for (int i = 0; i < meshTransform.childCount; i++) {
         InitializeAndContinue(meshTransform.GetChild(i));
      }
   }

   private void DeformMeshAndContinue(Transform meshTransform) {
      if (originalVerticiesStore == null) {
         Start();
         return;
      }

      if (meshTransform == null) {
         return;
      }

      if (originalVerticiesStore.ContainsKey(meshTransform)) {
         var originalVerticies = originalVerticiesStore[meshTransform];
         var meshFilter = meshTransform.GetComponent<MeshFilter>();
         if (meshFilter != null && originalVerticies != null && GetMesh(meshFilter).vertices.Length == originalVerticies.Length) {
            DeformMesh(originalVerticies, meshTransform, GetMesh(meshFilter));
         }
      }

      for (int i = 0; i < meshTransform.childCount; i++) {
         DeformMeshAndContinue(meshTransform.GetChild(i));
      }
   }

   private void DeformMesh(Vector3[] originalVerticies, Transform meshTransform, Mesh mesh) {
      if (originalVerticies == null || mesh == null || originalVerticies.Length != mesh.vertices.Length) {
         Start();
         return;
      }

      Vector3[] displacedVertices = new Vector3[mesh.vertices.Length];
      for (int i = 0; i < mesh.vertices.Length; i++) {
         displacedVertices[i] = CircleDeform(meshTransform, originalVerticies[i], transform.position);
      }
      mesh.vertices = displacedVertices;
   }

   private Vector3 CircleDeform(Transform meshTransform, Vector3 point, Vector3 deformationPoint) {
      var absolutePoint = meshTransform.localToWorldMatrix.MultiplyPoint(point);
      var newPoint = GetNewPosition(absolutePoint, rootObject.position, deformationPoint);
      return meshTransform.worldToLocalMatrix.MultiplyPoint(newPoint);
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