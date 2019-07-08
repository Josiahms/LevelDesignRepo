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
      ResourceManager.GetInstance().AddToWorkforce(GetComponent<Assignable>().GetMaxAssignees(), spawnpoint);
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