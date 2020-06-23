using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IUpgradeable {
    void OnUpgrade();
}

[RequireComponent(typeof(Collider))]
public class Upgradeable : MonoBehaviour, ISaveable {

   private int level;
   private List<Upgrade> levels;

   private void Awake() {
      levels = GetComponentsInChildren<Upgrade>(true).ToList();
   }

   public Upgrade GetNextUpgrade() {
      return level + 1 < GetComponentsInChildren<Upgrade>(true).Count() ? GetComponentsInChildren<Upgrade>(true)[level + 1] : null;
   }

   public bool Upgrade() {
      var upgrade = GetNextUpgrade();
      if (ResourceManager.GetInstance().OffsetAll(-upgrade.wood, -upgrade.stone, -upgrade.metal, 0)) {

         levels[level + 1].gameObject.SetActive(true);
         foreach (var upgradeable in GetComponents<IUpgradeable>()) {
            upgradeable.OnUpgrade();
         }
         levels[level].gameObject.SetActive(false);
         level++;

         return true;
      }
      return false;
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("level", level);

      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      level = (int)data["level"];
      if (levels.Count > 0) {
         levels[0].gameObject.SetActive(false);
         levels[Mathf.Min(level, levels.Count - 1)].gameObject.SetActive(true);
      }
   }

   public void OnLoadDependencies(object savedData) {
      // Ignored
   }
}