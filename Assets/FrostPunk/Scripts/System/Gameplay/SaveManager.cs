﻿using System;
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
   public SavedComponent[] components;

   public SavedGameObject(Saveable saveableGameObject) {
      index = saveableGameObject.GetSavedIndex();
      position = new float[] { saveableGameObject.transform.position.x, saveableGameObject.transform.position.y, saveableGameObject.transform.position.z };
      prefabPath = saveableGameObject.GetPrefabPath();
      components = saveableGameObject.gameObject.GetComponents<ISaveable>().Select(x => new SavedComponent(x)).ToArray();
   }
}

[Serializable]
public struct SavedComponent {
   public Type type;
   public object data;
   public SavedComponent(ISaveable saveable) {
      type = saveable.GetType();
      data = saveable.OnSave();
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
         if (String.IsNullOrEmpty(savedEntity.prefabPath)) {
            if (savedEntity.components.Any(x => x.type.GetInterfaces().Contains(typeof(ISingleton)))) {
               var singleton = savedEntity.components.First(x => x.type.GetInterfaces().Contains(typeof(ISingleton)));
               instance = (GameObject)singleton.type.BaseType.GetMethod("GetGameObject").Invoke(null, null);
            } else {
               Debug.LogError("Saved entity is not a singleton, nor does it have a prefab to load.");
            }
         } else {
            var prefab = (GameObject)Resources.Load(savedEntity.prefabPath);
            instance = Instantiate(prefab, new Vector3(savedEntity.position[0], savedEntity.position[1], savedEntity.position[2]), new Quaternion());
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