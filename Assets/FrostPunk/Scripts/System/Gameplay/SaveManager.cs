using System;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public struct SavedGameObject {
   public int index;
   public string prefabPath;
   public float[] position;
   public float[] rotation;
   public SavedComponent[] components;

   public SavedGameObject(Saveable saveableGameObject) {
      index = saveableGameObject.GetSavedIndex();
      if (saveableGameObject.SavePosition) {
         position = new float[] { saveableGameObject.transform.position.x, saveableGameObject.transform.position.y, saveableGameObject.transform.position.z };
         rotation = new float[] { saveableGameObject.transform.rotation.x, saveableGameObject.transform.rotation.y, saveableGameObject.transform.rotation.z, saveableGameObject.transform.rotation.w };
      } else {
         position = null;
         rotation = null;
      }
      prefabPath = saveableGameObject.GetPrefabPath();
      components = saveableGameObject.gameObject.GetComponents<ISaveable>().Select(x => new SavedComponent(x)).ToArray();
   }
}

[Serializable]
public class SavedComponent {
   public Type type;
   public Dictionary<string, object> data;

   public SavedComponent(object itemToSave) {
      type = itemToSave.GetType();
      var allFields = itemToSave.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
      data = allFields.ToDictionary(x => x.Name, x => {
         var value = GetSaveData(x.GetValue(itemToSave));
         return value;
      });
   }


   public object GetSaveData(object fieldValue) {
      if (fieldValue == null) {
         return null;
      }

      var fieldType = fieldValue.GetType();

      if (fieldType.IsPrimitive || fieldType.IsEnum) {
         return fieldValue;
      }

      if (fieldType.GetCustomAttributes(false).Any(x => x.GetType() == typeof(SerializableAttribute))) {
         if (!fieldType.IsGenericType) {
            return fieldValue;
         }
      }

      if (fieldType.GetInterfaces().Contains(typeof(IDictionary))) {
         return HandleDictionary((IDictionary)fieldValue);
      }

      if (typeof(Component).IsAssignableFrom(fieldType)) {
         return HandleReferenceToOther((Component)fieldValue);
      }

      if (fieldType == typeof(GameObject)) {
         return null;
      }

      if (typeof(IEnumerable).IsAssignableFrom(fieldType)) {
         return HandleList((IEnumerable)fieldValue);
      }

      return new SavedComponent(fieldValue);
   }

   private List<object> HandleList(IEnumerable list) {
      List<object> result = new List<object>();
      foreach (var item in list) {
         result.Add(GetSaveData(item));
      }
      return result;
   }

   private object HandleReferenceToOther(Component component) {
      if (component.GetComponent<Saveable>() != null) {
         return component.GetComponent<Saveable>().GetSavedIndex();
      }
      return null;
   }

   private Dictionary<object, object> HandleDictionary(IDictionary dictionary) {
      var result = new Dictionary<object, object>();
      var keys = dictionary.Keys.GetEnumerator();
      var values = dictionary.Values.GetEnumerator();

      while (keys.MoveNext() && values.MoveNext()) {
         result.Add(GetSaveData(keys.Current), GetSaveData(values.Current));
      }

      return result;
   }

}

public class SaveManager : Singleton<SaveManager> {

   private List<Saveable> entitiesToSave = new List<Saveable>();
   private List<SavedGameObject> savedEntities = new List<SavedGameObject>();
   private Dictionary<int, GameObject> loadedEntities = new Dictionary<int, GameObject>();

   private new void Awake() {
      base.Awake();
      SceneManager.sceneLoaded += OnSceneLoad;
   }

   private void OnSceneLoad(Scene scene, LoadSceneMode mode) {
      if (scene.buildIndex == 0) {
         Load();
      }
   }

   public void RegisterEntity(Saveable entity, bool partOfOriginalScene = false) {
      entitiesToSave.Add(entity);
   }

   public void RemoveEntity(Saveable entity) {
      entitiesToSave.Remove(entity);
   }

   private void Update() {
      if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R)) {
         Reset();
         return;
      }

      if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R)) {
         DontDestroy.DestroyAll();
         SceneManager.LoadScene(0);
         return;
      }

      if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S)) {
         Save();
         return;
      }
   }

   private void Reset() {
      if (File.Exists(Application.persistentDataPath + "/game.fun")) {
         File.Delete(Application.persistentDataPath + "/game.fun");
      }
      DontDestroy.DestroyAll();
      SceneManager.LoadScene(0);
   }

   public GameObject FindLoadedInstanceBySaveIndex(int saveIndex) {
      GameObject instance;
      if (loadedEntities.TryGetValue(saveIndex, out instance)) {
         return instance;
      } else {
         Debug.LogWarning("Could not find saved entity with index: " + saveIndex);
         return null;
      }
   }

   public void Save() {
      savedEntities.Clear();
      foreach (var saveable in entitiesToSave) {
         savedEntities.Add(new SavedGameObject(saveable));
      }
      BinaryFormatter bf = new BinaryFormatter();
      FileStream fs = new FileStream(Application.persistentDataPath + "/game.fun", FileMode.Create);
      bf.Serialize(fs, savedEntities);
      fs.Close();
   }

   private void Load() {
      if (!File.Exists(Application.persistentDataPath + "/game.fun")) {
         return;
      }

      FileStream fs = new FileStream(Application.persistentDataPath + "/game.fun", FileMode.Open);
      BinaryFormatter bf = new BinaryFormatter();
      savedEntities = (List<SavedGameObject>)bf.Deserialize(fs);
      fs.Close();

      foreach (var savedEntity in savedEntities) {
         GameObject instance = null;
         var position = savedEntity.position == null
            ? Vector3.zero
            : new Vector3(savedEntity.position[0], savedEntity.position[1], savedEntity.position[2]);
         var rotation = savedEntity.rotation == null
            ? new Quaternion()
            : new Quaternion(savedEntity.rotation[0], savedEntity.rotation[1], savedEntity.rotation[2], savedEntity.rotation[3]);
         if (String.IsNullOrEmpty(savedEntity.prefabPath)) {
            if (savedEntity.components.Any(x => x.type.GetInterfaces().Contains(typeof(ISingleton)))) {
               var singleton = savedEntity.components.First(x => x.type.GetInterfaces().Contains(typeof(ISingleton)));
               instance = (GameObject)singleton.type.BaseType.GetMethod("GetGameObject").Invoke(null, null);
               if (savedEntity.position != null) {
                  instance.transform.position = position;
                  instance.transform.rotation = rotation;
               }
            } else {
               Debug.LogError("Saved entity is not a singleton, nor does it have a prefab to load.");
            }
         } else {
            var prefab = (GameObject)Resources.Load(savedEntity.prefabPath);
            
            instance = Instantiate(prefab, position, rotation);
         }
         if (instance != null) {
            var loadedComponents = instance.GetComponents<ISaveable>();
            for (int i = 0; i < savedEntity.components.Length && i < loadedComponents.Length; i++) {
               var savedComponent = savedEntity.components[i];
               loadedComponents[i].OnLoad(savedComponent.data);
            }
            loadedEntities.Add(savedEntity.index, instance);
         }
      }

      foreach (var savedEntity in savedEntities) {
         GameObject loadedInstance;
         if (loadedEntities.TryGetValue(savedEntity.index, out loadedInstance)) {
            var loadedComponents = loadedInstance.GetComponents<ISaveable>();
            for (int i = 0; i < savedEntity.components.Length && i < loadedComponents.Length; i++) {
               var savedComponent = savedEntity.components[i];
               loadedComponents[i].OnLoadDependencies(savedComponent.data);
            }
         }
      }
   }

   private void OnApplicationQuit() {
      Save();
   }

   private void OnDestroy() {
      SceneManager.sceneLoaded -= OnSceneLoad;
   }

}
