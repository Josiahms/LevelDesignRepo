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

   private float health;
   private FillerBar healthBar;

   public void Start() {
      health = maxHealth;
      healthBar = FillerBar.Instantiate(transform, health, maxHealth, Color.green, Color.red, Color.black);
   }

   public void OffsetHealth(float amount) {
      health += amount;
      health = Mathf.Clamp(health, 0, maxHealth);

      healthBar.SetPercent(health, maxHealth);

      if (health == 0) {
         foreach (var destructable in GetComponents<IDestructable>()) {
            destructable.OnDestruction();
         }
         Destroy(gameObject);
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("health", health);
      data.Add("maxHealth", maxHealth);
      return data;
   }

   public void OnLoad(object data) {
      var savedData = (Dictionary<string, object>)data;
      health = (float)savedData["health"];
      maxHealth = (float)savedData["maxHealth"];
   }

   public void OnLoadDependencies(object data) {
      // Ignored;
   }
}