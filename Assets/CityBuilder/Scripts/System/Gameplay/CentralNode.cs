using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentralNode : MonoBehaviour {

   [SerializeField]
   private int minNumber;
   public int MinNumber { get { return minNumber; } }

   private List<Placeable> children;

}
