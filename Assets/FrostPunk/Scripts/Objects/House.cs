using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Assignable))]
[RequireComponent(typeof(Placeable))]
public class House : MonoBehaviour, IPlaceable {

   [SerializeField]
   private Transform spawnpoint;

   private void Start() {
      if (GetComponent<Placeable>().IsPlaced()) {
         OnPlace();
      }
   }

   public bool OnPlace() {
      for (int i = 0; i < GetComponent<Assignable>().GetMaxAssignees(); i++) {
         Worker.Instantiate(this, spawnpoint.position + Vector3.Scale(Random.insideUnitSphere, new Vector3(3f, 0, 3f)), spawnpoint.transform.rotation);
      }
      return true;
   }

   public bool OnRemove() {
      // TODO: How do we remove workers from the world?
      //ResourceManager.GetInstance().OffsetMaxPopulation(-capacity);
      return true;
   }

   public object OnSave() {
      return null;
   }
}