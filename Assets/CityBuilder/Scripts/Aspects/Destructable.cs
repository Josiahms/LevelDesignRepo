using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public interface IDestructable {
   void OnDestruction();
}

public class Destructable : MonoBehaviour, ISaveable {

   [SerializeField]
   private float maxHealth = 100;
   [SerializeField]
   private Team team;
   public Team GetTeam() { return team;  }

   public float Health { get; private set; }
   private FillerBar healthBar;

   public void Start() {
      Health = maxHealth;
      healthBar = FillerBar.Instantiate(transform, Health, maxHealth, Color.green, Color.red, Color.black);
   }

   public void OffsetHealth(float amount) {
      Health += amount;
      Health = Mathf.Clamp(Health, 0, maxHealth);

      healthBar.SetPercent(Health, maxHealth);

      if (Health == 0) {
         foreach (var destructable in GetComponents<IDestructable>()) {
            destructable.OnDestruction();
         }
         Destroy(gameObject);
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("health", Health);
      data.Add("maxHealth", maxHealth);
      return data;
   }

   public void OnLoad(object data) {
      var savedData = (Dictionary<string, object>)data;
      Health = (float)savedData["health"];
      maxHealth = (float)savedData["maxHealth"];
   }

   public void OnLoadDependencies(object data) {
      // Ignored;
   }
}