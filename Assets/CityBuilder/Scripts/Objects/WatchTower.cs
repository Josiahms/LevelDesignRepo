using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Placeable))]
public class WatchTower : MonoBehaviour, IUpgradeable {

   [SerializeField]
   private Archer archerPrefab;

   private void Start() {
      var placeable = GetComponent<Placeable>();
      if (placeable.IsPlaced()) {
         Instantiate(archerPrefab, GetComponentInChildren<ArcherSpawn>().transform);
      } else {
         GetComponent<Placeable>().OnPlaceEvent.AddListener((x) => Instantiate(archerPrefab, GetComponentInChildren<ArcherSpawn>().transform));
      }
   }

   public void OnUpgrade() {
      GetComponentInChildren<Archer>().transform.SetParent(GetComponent<Upgradeable>().GetNextUpgrade().GetComponentInChildren<ArcherSpawn>().transform, false);
      GetComponentInChildren<FillerBar>().transform.SetParent(GetComponent<Upgradeable>().GetNextUpgrade().GetComponentInChildren<HealthBarSpawn>().transform, false);
      GetComponent<Destructable>().OffsetMaxHealth(50);
      GetComponent<Destructable>().OffsetHealth(50);
   }

}
