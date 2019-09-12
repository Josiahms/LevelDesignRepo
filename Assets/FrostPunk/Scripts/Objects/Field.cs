using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Workstation))]
public class Field : MonoBehaviour {

   [SerializeField]
   private Transform crops;

   private void Update() {
      crops.localScale = new Vector3(crops.localScale.x, GetComponent<Workstation>().PercentComplete, crops.localScale.z);
   }

}
