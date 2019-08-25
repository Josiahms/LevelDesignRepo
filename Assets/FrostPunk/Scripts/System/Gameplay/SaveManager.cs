using System;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
      try {
         bf.Serialize(fs, savedEntities);
      } catch (Exception e) {
         Debug.LogException(e);
      } finally {
         fs.Close();
      }
   }

   private void Load() {
      if (!File.Exists(Application.persistentDataPath + "/game.fun")) {
         return;
      }

      FileStream fs = new FileStream(Application.persistentDataPath + "/game.fun", FileMode.Open);
      try {
         BinaryFormatter bf = new BinaryFormatter();
         savedEntities = (List<SavedGameObject>)bf.Deserialize(fs);
      } catch (Exception e) {
         Debug.LogException(e);
      } finally {
         fs.Close();
      }

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
            instance = Instantiate(prefab);
         }
         if (instance != null) {
            var loadedComponents = instance.GetComponents(typeof(ISaveable));
            for (int i = 0; i < savedEntity.components.Length && i < loadedComponents.Length; i++) {
               var savedComponent = savedEntity.components[i];
               LoadComponent(loadedComponents[i], savedComponent.data, false);
            }
            loadedEntities.Add(savedEntity.index, instance);
         }
      }

      foreach (var savedEntity in savedEntities) {
         GameObject loadedInstance;
         if (loadedEntities.TryGetValue(savedEntity.index, out loadedInstance)) {
            var loadedComponents = loadedInstance.GetComponents(typeof(ISaveable));
            for (int i = 0; i < savedEntity.components.Length && i < loadedComponents.Length; i++) {
               var savedComponent = savedEntity.components[i];
               LoadComponent(loadedComponents[i], savedComponent.data, true);
               foreach (var afterLoadCallback in loadedComponents[i].GetComponents<IAfterLoadCallback>()) {
                  afterLoadCallback.AfterLoad();
               }
            }

            if (savedEntity.parentIndex.HasValue) {
               loadedInstance.transform.SetParent(FindLoadedInstanceBySaveIndex(savedEntity.parentIndex.Value).transform, false);
            }

            SetPosition(loadedInstance, savedEntity);
         }
      }
   }

   private void SetPosition(GameObject instance, SavedGameObject savedEntity) {
      var rectTransform = instance.GetComponent<RectTransform>();
      if (savedEntity.position != null) {
         if (rectTransform != null) {
            Vector2 pivot = new Vector2(savedEntity.position[0], savedEntity.position[1]);
            Vector2 anchoredPosition = new Vector2(savedEntity.position[2], savedEntity.position[3]); ;
            Vector2 anchoredMax = new Vector2(savedEntity.position[4], savedEntity.position[5]); ;
            Vector2 anchoredMin = new Vector2(savedEntity.position[6], savedEntity.position[7]); ;
            Vector2 sizeDelta = new Vector2(savedEntity.position[8], savedEntity.position[9]);

            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.anchorMax = anchoredMax;
            rectTransform.anchorMin = anchoredMin;
            rectTransform.sizeDelta = sizeDelta;

            instance.transform.rotation = new Quaternion();
            instance.transform.localScale = Vector3.one;

         } else {
            instance.transform.position = new Vector3(savedEntity.position[0], savedEntity.position[1], savedEntity.position[2]);
            instance.transform.rotation = new Quaternion(savedEntity.rotation[0], savedEntity.rotation[1], savedEntity.rotation[2], savedEntity.rotation[3]);
         }
      }
   }

   private void LoadComponent(Component component, Dictionary<string ,object> data, bool forDependencies) {
      if (component.name == "QuestManager") {
         Debug.Log("Loading quest manager component");
      }
      var allFields = component.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
      foreach (var field in allFields) {
         var currentFieldValue = field.GetValue(component);
         object result;
         if (data.TryGetValue(field.Name, out result)) {
            var savedData = GetSaveData(field.FieldType, result, forDependencies);
            if (!IsNullOrEmpty(savedData)) {
               if (component.name == "QuestManager") {
                  Debug.Log("Setting field " + field.Name + " to " + savedData);
               }
               field.SetValue(component, savedData);
            }
         }
         if (forDependencies && component.GetComponent<Saveable>() != null) {
            component.GetComponent<Saveable>().IsLoaded = true;
         }
      }
   }

   private bool IsNullOrEmpty(object thing) {
      if (thing == null) {
         return true;
      }

      if (thing as IEnumerable != null) {
         return !(thing as IEnumerable).GetEnumerator().MoveNext();
      }

      return false;
   }

   public object GetSaveData(Type fieldType, object value, bool forDependencies) {
      if (fieldType.IsPrimitive || fieldType.IsEnum) {
         return value;
      }

      if (fieldType.GetCustomAttributes(false).Any(x => x.GetType() == typeof(SerializableAttribute))) {
         if (!fieldType.IsGenericType) {
            return value;
         }
      }

      if (fieldType.GetInterfaces().Contains(typeof(IDictionary))) {
         return GetDictionary(fieldType, (IDictionary)value, forDependencies);
      }

      if (fieldType.IsSubclassOf(typeof(Component))) {
         return GetSavedComponent(fieldType, value, forDependencies);
      }

      if (typeof(IEnumerable).IsAssignableFrom(fieldType)) {
         return GetList(fieldType, (IEnumerable)value, forDependencies);
      }

      return null;
   }

   private object GetDictionary(Type fieldType, IDictionary dictionary, bool forDependencies) {
      var keyType = fieldType.GetGenericArguments().ElementAt(0);
      var valueType = fieldType.GetGenericArguments().ElementAt(1);
      var keysEnumerator = dictionary.Keys.GetEnumerator();
      var valuesEnumerator = dictionary.Values.GetEnumerator();

      var result = (IDictionary)typeof(Dictionary<,>)
         .MakeGenericType(new Type[] { keyType, valueType })
         .GetConstructor(new Type[] { })
         .Invoke(new object[] { });

      while (keysEnumerator.MoveNext() && valuesEnumerator.MoveNext()) {
         var keyToAdd = GetSaveData(keyType, keysEnumerator.Current, forDependencies);
         var valueToAdd = GetSaveData(valueType, valuesEnumerator.Current, forDependencies);
         if (keyToAdd != null && valueToAdd != null) {
            result.Add(
               keyToAdd,
               valueToAdd
            );
         }
      }

      return result;
   }

   private object GetSavedComponent(Type fieldType, object value, bool forDependencies) {
      if (value != null && forDependencies) {
         var loadedInstance = FindLoadedInstanceBySaveIndex((int)value);
         if (loadedInstance != null) {
            return loadedInstance.GetComponent(fieldType);
         }
      }
      return null;
   }

   private object GetList(Type fieldType, IEnumerable value, bool forDependencies) {
      if (fieldType.IsGenericType) {
         var result = new List<object>();
         var containedType = fieldType.GetGenericArguments().First();
         foreach (var item in value) {
            var nestedValue = GetSaveData(containedType, item, forDependencies);
            if (nestedValue != null) {
               result.Add(nestedValue);
            }
         }

         var enumeratorResult = typeof(Enumerable).GetMethod("Cast", new[] { typeof(IEnumerable) })
            .MakeGenericMethod(containedType)
            .Invoke(null, new object[] { result });

         var listResult = (IList)typeof(Enumerable).GetMethod("ToList")
            .MakeGenericMethod(containedType)
            .Invoke(null, new object[] { enumeratorResult });

         return listResult;

      }
      return value;
   }

   private void OnApplicationQuit() {
      Save();
   }

   private void OnDestroy() {
      SceneManager.sceneLoaded -= OnSceneLoad;
   }
}