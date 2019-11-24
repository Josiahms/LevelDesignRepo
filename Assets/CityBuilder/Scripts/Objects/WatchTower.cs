using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchTower : MonoBehaviour {

   [SerializeField]
   private Archer archerPrefab;
   [SerializeField]
   private Transform archerLocation;

   private void Start() {
      Instantiate(archerPrefab, archerLocation);
   }

}
