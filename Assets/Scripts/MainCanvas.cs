using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class MainCanvas : MonoBehaviour {

   private static Canvas instance;

   public static Canvas Get() {
      return instance;
   }

   private void Awake() {
      instance = GetComponent<Canvas>();
   }

}
