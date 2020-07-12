using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Walker))]
public class Worker : MonoBehaviour, ISaveable, IInstructable {

   private Vector3? previousLocation;
   private Assignable assignedLocation;
   private bool createdFromScene = true;

   public Housing House { get; set; }

   public static Worker Instantiate(Housing house, Vector3 position, Quaternion rotation) {
      var result = Instantiate(ResourceLoader.GetInstance().WorkerPrefab, position + Vector3.Scale(Random.insideUnitSphere, new Vector3(3f, 0, 3f)), rotation);
      result.House = house;
      result.createdFromScene = false;
      PopulationManager.GetInstance().AddToWorkforce(result);
      return result;
   }

   private void Start() {
      DayCycleManager.GetInstance().OnStartWorkDay.AddListener(() => {
         GetComponent<Walker>().SetDestination(previousLocation.GetValueOrDefault());
      });

      DayCycleManager.GetInstance().OnEndWorkDay.AddListener(() => {
         previousLocation = GetComponent<Walker>().GetDestination();
         GetComponent<Walker>().SetDestination(House != null ? House.transform.position : WorkerSpawner.GetInstance().transform.position);
      });

      if (createdFromScene) {
         PopulationManager.GetInstance().AddToWorkforce(this);
      }
   }

   private void OnDestroy() {
      if (House != null) {
         House.RemoveWorker(this);
      }
      if (ResourceManager.GetInstance() != null) {
         PopulationManager.GetInstance().RemoveFromWorkforce(this);
      }
      if (assignedLocation != null) {
         assignedLocation.RemoveWorker(this);
      }
   }

   public bool SetDestination(Assignable assignment) {
      if (assignment == null || assignment.AddWorker(this)) {
         if (assignedLocation != null && assignedLocation != assignment) {
            assignedLocation.RemoveWorker(this);
         }
         assignedLocation = assignment;
         SetWalkerDestination(assignment == null ? null : (Vector3?)assignment.GetSpotForWorker(this).position);
         return true;
      }
      return false;
   }

   public void SetDestination(Vector3 destination) {
      if (assignedLocation != null) {
         assignedLocation.RemoveWorker(this);
         assignedLocation = null;
      }
      SetWalkerDestination(destination);
   }

   public bool IsAssigned() {
      return assignedLocation != null;
   }

   private void SetWalkerDestination(Vector3? destination) {
      if (DayCycleManager.GetInstance().IsRestTime()) {
         previousLocation = destination;
      } else {
         GetComponent<Walker>().SetDestination(destination, 0.7f);
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data.Add("assignedLocation", assignedLocation != null ? assignedLocation.GetComponent<Saveable>().GetSavedIndex() : -1);
      data.Add("previousLocation", previousLocation.HasValue ? new float[] { previousLocation.Value.x, previousLocation.Value.y, previousLocation.Value.z } : null);
      data.Add("house", House == null ? null : (int?)House.GetComponent<Saveable>().GetSavedIndex());
      return data;
   }

   public void OnLoad(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      createdFromScene = false;
      var prevLoc = (float[])data["previousLocation"];
      if (prevLoc != null) {
         previousLocation = new Vector3(prevLoc[0], prevLoc[1], prevLoc[2]);
      }
   }

   public void OnLoadDependencies(object savedData) {
      var data = (Dictionary<string, object>)savedData;
      var houseIndex = (int?)data["house"];
      House = houseIndex.HasValue ? SaveManager.GetInstance().FindLoadedInstanceBySaveIndex(houseIndex.Value).GetComponent<Housing>() : null;
      assignedLocation = (int)data["assignedLocation"] == -1 ? null : SaveManager.GetInstance().FindLoadedInstanceBySaveIndex((int)data["assignedLocation"]).GetComponent<Assignable>();
   }

   public void OnInstruct(Vector3 location, GameObject target, List<Selectable> originalSelection) {
      if (target == null || target.GetComponent<Assignable>() == null) {
         var selectedWorkers = originalSelection.Select(x => x.GetComponent<Worker>()).Where(x => x != null).ToList();
         var i = selectedWorkers.IndexOf(this);
         var offset = new Vector3(3 * (i % 5), 0, 3 * (i / 5));
         SetDestination(location + offset);
      } else {
         SetDestination(target.GetComponent<Assignable>());
      }
      SelectionManager.GetInstance().Deselect(GetComponent<Selectable>());
   }
}
