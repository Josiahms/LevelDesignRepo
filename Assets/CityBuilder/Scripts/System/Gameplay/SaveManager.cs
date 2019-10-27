using System;
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
      components = saveableGameObject.gameObject.GetComponents(typeof(ISaveable)).Select(x => new SavedComponent(x)).ToArray();
   }
}
// TODO: Save enabled state
[Serializable]
public struct SavedComponent {
   public Type type;
   public object data;
   public bool isEnabled;

   public SavedComponent(Component component) {
      type = component.GetType();
      data = (component as ISaveable).OnSave();
      if (component as Behaviour != null) {
         isEnabled = (component as Behaviour).enabled;
      } else {
         isEnabled = false;
      }
   }
}

public class SaveManager : Singleton<SaveManager> {

   private List<Saveable> entitiesToSave = new List<Saveable>();
   private List<SavedGameObject> savedEntities = new List<SavedGameObject>();
   private Dictionary<int, GameObject> loadedEntities = new Dictionary<int, GameObject>();
   private bool loadSuccess = true;

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
      if (loadSuccess == true) {
         savedEntities.Clear();
         foreach (var saveable in entitiesToSave) {
            savedEntities.Add(new SavedGameObject(saveable));
         }
         BinaryFormatter bf = new BinaryFormatter();
         FileStream fs = new FileStream(Application.persistentDataPath + "/game.fun", FileMode.Create);
         try {
            bf.Serialize(fs, savedEntities);
         } catch (Exception e) {
            Debug.LogException(e);
         } finally {
            fs.Close();
         }
      } else {
         Debug.LogWarning("Did not save because last load was unsuccessful");
      }
   }

   private void Load() {      
      if (!File.Exists(Application.persistentDataPath + "/game.fun")) {
         return;
      }

      loadSuccess = false;
      BinaryFormatter bf = new BinaryFormatter();
      FileStream fs = new FileStream(Application.persistentDataPath + "/game.fun", FileMode.Open);
      try {
         savedEntities = (List<SavedGameObject>)bf.Deserialize(fs);
      } catch (Exception e) {
         Debug.LogException(e);
      } finally {
         fs.Close();
      }

      try {
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
                  throw new Exception("Saved entity " + savedEntity.prefabPath + " has no prefab and is not a singleton");
               }
            } else {
               var prefab = (GameObject)Resources.Load(savedEntity.prefabPath);

               instance = Instantiate(prefab, position, rotation);
            }
            if (instance != null) {
               var loadedComponents = instance.GetComponents(typeof(ISaveable));
               for (int i = 0; i < savedEntity.components.Length && i < loadedComponents.Length; i++) {
                  var savedComponent = savedEntity.components[i];
                  (loadedComponents[i] as ISaveable).OnLoad(savedComponent.data);
                  if (loadedComponents[i] as Behaviour != null) {
                     (loadedComponents[i] as Behaviour).enabled = savedComponent.isEnabled;
                  }
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
         loadSuccess = true;
      } catch (Exception e) {
         Debug.LogException(e);
      }
   }

   private void OnApplicationQuit() {
      Save();
   }

   private void OnDestroy() {
      SceneManager.sceneLoaded -= OnSceneLoad;
   }

}
