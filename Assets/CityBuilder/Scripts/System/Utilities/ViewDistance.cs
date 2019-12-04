using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewDistance : MonoBehaviour {

   [SerializeField]
   private float maxDistance = 10;

   private List<MeshRenderer> renderers = new List<MeshRenderer>();

   private void Awake() {
      renderers.AddRange(GetComponentsInChildren<MeshRenderer>());
   }

   private void Update() {
      transform.GetChild(0).gameObject.SetActive(Vector3.Distance(transform.position, Camera.main.transform.position) < maxDistance);
   }



}