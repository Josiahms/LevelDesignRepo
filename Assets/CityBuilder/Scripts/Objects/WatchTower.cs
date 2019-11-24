using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Placeable))]
public class WatchTower : MonoBehaviour {

   [SerializeField]
   private Archer archerPrefab;
   [SerializeField]
   private Transform archerLocation;

   private void Start() {
      var placeable = GetComponent<Placeable>();
      if (placeable.IsPlaced()) {
         Instantiate(archerPrefab, archerLocation);
      } else {
         GetComponent<Placeable>().OnPlaceEvent.AddListener((x) => Instantiate(archerPrefab, archerLocation));
      }
   }

}
