using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Assignable))]
[RequireComponent(typeof(Placeable))]
public class House : MonoBehaviour, IPlaceable, ISaveable {

   [SerializeField]
   private Transform spawnpoint;
   [SerializeField]
   private List<Worker> workers = new List<Worker>();

   public void OnPlace() {
      for (int i = 0; i < GetComponent<Assignable>().GetMaxAssignees(); i++) {
         workers.Add(Worker.Instantiate(this, spawnpoint.position + Vector3.Scale(Random.insideUnitSphere, new Vector3(3f, 0, 3f)), spawnpoint.transform.rotation));
      }
   }

   public void RemoveWorker(Worker worker) {
      workers.Remove(worker);
   }

   public void OnRemove() {
      workers.ForEach(x => {
         if (x != null) {
            Destroy(x.gameObject);
         }
      });
   }

   public void OnLoad(object saveData) {
      // Ignored
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      object result = null;
      if (data.TryGetValue("workers", out result)) {
         workers = new List<Worker>();
         foreach (var item in (IEnumerable)result) {
            workers.Add(SaveManager.GetInstance().FindLoadedInstanceBySaveIndex((int)item).GetComponent<Worker>());
         }
      }
   }
}