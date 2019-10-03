using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FrameCounter : MonoBehaviour {

   private Text text;

   private int min;

   private void Start() {
      text = GetComponent<Text>();
      Application.targetFrameRate = -1;
   }

   private void Update() {
      text.text = ((int)(1 / Time.unscaledDeltaTime)).ToString(); ;
   }

}
