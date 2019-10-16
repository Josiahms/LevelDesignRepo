using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class Deformer : MonoBehaviour
{
   [SerializeField]
   private Transform pivot;
   [SerializeField]
   private Transform center;

   [SerializeField]
   private List<Transform> transforms;
   [SerializeField]
   private List<Vector3> originalPositions;

   private void Start() {
      originalPositions.Clear();
      foreach (var trans in transforms) {
         originalPositions.Add(trans.position);
      }
   }

   private void OnDisable() {
      for (int i = 0; i < transforms.Count; i++) {
         transforms[i].position = originalPositions[i];
         transforms[i].rotation = new Quaternion();
      }
      originalPositions.Clear();
   }

   private void Update() {
      if (originalPositions.Count < transforms.Count) {
         Start();
         return;
      }

      for (int i = 0; i < transforms.Count; i++) {
         Transform(transforms[i], originalPositions[i]);
      }
   }

   private void Transform(Transform trans, Vector3 originalPosition) {
      trans.position = GetNewPosition(originalPosition, center.position, pivot.position);
      trans.LookAt(pivot);
   }

   private Vector3 GetNewPosition(Vector3 originalPosition, Vector3 meshCenter, Vector3 pivotPoint) {
      var circleRadius = meshCenter - pivotPoint;

      var angle = Vector3.SignedAngle(circleRadius, originalPosition - pivotPoint, Vector3.up);
      var horizontalOffset = (originalPosition - pivotPoint).magnitude * Mathf.Sin(Mathf.Deg2Rad * angle);
      var radiusAtPoint = (originalPosition - pivotPoint).magnitude * Mathf.Cos(Mathf.Deg2Rad * angle);

      var arcLengthRatio = horizontalOffset / circleRadius.magnitude;
      var newPosition = new Vector3(Mathf.Cos(arcLengthRatio), 0, Mathf.Sin(arcLengthRatio)) * radiusAtPoint;

      return Quaternion.FromToRotation(Vector3.right, circleRadius) * newPosition + pivotPoint;
   }

}