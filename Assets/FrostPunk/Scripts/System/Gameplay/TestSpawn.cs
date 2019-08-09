using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawn : MonoBehaviour
{
   [SerializeField]
   private GameObject testSubject;

   private void Start() {
      if (testSubject != null) {
         Instantiate(testSubject, transform);
      }
   }
}
