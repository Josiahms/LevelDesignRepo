using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDeformer : Singleton<MeshDeformer>, ISaveable {

   [SerializeField]
   private List<Transform> rootObjects;

   private Dictionary<Transform, Vector3[]> originalVerticiesStore;
   private Dictionary<Transform, float> prevLocationStore = new Dictionary<Transform, float>();

   private void Start() {
      originalVerticiesStore = new Dictionary<Transform, Vector3[]>();
      foreach (var rootObject in rootObjects) {
         InitializeAndContinue(rootObject);
      }
   }

   public void AddMesh(Transform mesh) {
      rootObjects.Add(mesh);
      InitializeAndContinue(mesh);
   }

   public void RemoveMesh(Transform mesh) {
      rootObjects.Remove(mesh);
      originalVerticiesStore.Remove(mesh);
      prevLocationStore.Remove(mesh);
   }

   private void Update() {
      if (rootObjects.Count == 0) {
         return;
      }

      foreach (var rootObject in rootObjects) {

         if (!prevLocationStore.ContainsKey(rootObject)) {
            prevLocationStore.Add(rootObject, 0);
         }

         var prevDistance = prevLocationStore[rootObject];
         var currentDistance = (transform.position - rootObject.position).magnitude;
         if (Mathf.Abs(currentDistance - prevDistance) > 1) {
            DeformMeshAndContinue(rootObject, rootObject.position);
            prevLocationStore[rootObject] = currentDistance;
         }
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

   private void DeformMeshAndContinue(Transform meshTransform, Vector3 rootPosition) {
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
            DeformMesh(originalVerticies, meshTransform, rootPosition, GetMesh(meshFilter));
         }
      }

      for (int i = 0; i < meshTransform.childCount; i++) {
         DeformMeshAndContinue(meshTransform.GetChild(i), rootPosition);
      }
   }

   private void DeformMesh(Vector3[] originalVerticies, Transform meshTransform, Vector3 rootPosition, Mesh mesh) {
      if (originalVerticies == null || mesh == null || originalVerticies.Length != mesh.vertices.Length) {
         Start();
         return;
      }

      Vector3[] displacedVertices = new Vector3[mesh.vertices.Length];
      for (int i = 0; i < mesh.vertices.Length; i++) {
         var absolutePosition = meshTransform.localToWorldMatrix.MultiplyPoint(originalVerticies[i]);
         var relativePosition = meshTransform.worldToLocalMatrix.MultiplyPoint(GetNewPosition(absolutePosition, rootPosition, transform.position));
         displacedVertices[i] = relativePosition;
      }
      mesh.vertices = displacedVertices;
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

   private Mesh GetMesh(MeshFilter meshFilter) {
      if (Application.isPlaying) {
         return meshFilter.mesh;
      } else {
         return meshFilter.sharedMesh;
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("rootObjects", rootObjects.Select(x => x.GetComponent<Saveable>().GetSavedIndex()).ToArray());
      return data;
   }

   public void OnLoad(object data) {
      // Ignored
   }

   public void OnLoadDependencies(object data) {
      var savedData = (Dictionary<string, object>)data;
      rootObjects = ((int[])savedData["rootObjects"]).Select(x => SaveManager.GetInstance().FindLoadedInstanceBySaveIndex(x).GetComponent<Transform>()).ToList();
   }
}