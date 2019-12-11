using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SnapToCircleGrid))]
[RequireComponent(typeof(Assignable))]
[RequireComponent(typeof(Selectable))]
public class BuildSite : MonoBehaviour, ISaveable {

   [SerializeField]
   private float buildSpeed = 5f;

   private const float DELTA_Y = 6f;

   private Placeable pendingInstance;
   private float percentComplete;

   public static BuildSite Instantiate(Placeable pendingInstance) {
      var instance = Instantiate(ResourceLoader.GetInstance().BuildSite, pendingInstance.transform.position, new Quaternion());
      pendingInstance.transform.position += new Vector3(0, -DELTA_Y, 0);
      instance.GetComponent<SnapToCircleGrid>().SetCenter(pendingInstance.GetComponent<SnapToCircleGrid>().GetCenter(), pendingInstance.GetComponent<SnapToCircleGrid>().GetMinNumber());
      instance.pendingInstance = pendingInstance;
      return instance;
   }


   private void Update() {
      var deltaPercent = DayCycleManager.GetInstance().ClockMinuteRate / 5 * Time.deltaTime * buildSpeed * GetComponent<Assignable>().GetWorkersInRange();
      deltaPercent = Mathf.Min(deltaPercent, 100 - percentComplete);
      percentComplete += deltaPercent;
      pendingInstance.transform.position += new Vector3(0, (deltaPercent / 100) * DELTA_Y, 0);
      if (percentComplete >= 100) {
         pendingInstance.Place();
         Destroy(gameObject);
      }
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data["percentComplete"] = percentComplete;
      data["pendingInstance"] = pendingInstance == null ? -1 : pendingInstance.GetComponent<Saveable>().GetSavedIndex();
      return data;
   }

   public void OnLoad(object data) {
      var savedData = (Dictionary<string, object>)data;
      percentComplete = (float)savedData["percentComplete"];
      
   }

   public void OnLoadDependencies(object data) {
      var savedData = (Dictionary<string, object>)data;
      pendingInstance = SaveManager.GetInstance().FindLoadedInstanceBySaveIndex((int)savedData["pendingInstance"]).GetComponent<Placeable>();
   }
}
