using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SavedGameObject {
   public int index;
   public int? parentIndex;
   public string prefabPath;
   public float[] position;
   public float[] rotation;
   public SavedComponent[] components;

   public SavedGameObject(Saveable saveableGameObject) {
      index = saveableGameObject.GetSavedIndex();
      if (saveableGameObject.SavePosition) {
         if (saveableGameObject.GetComponent<RectTransform>()) {
            var rectTransform = saveableGameObject.GetComponent<RectTransform>();
            position = new float[] {
               rectTransform.pivot.x, rectTransform.pivot.y,
               rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y,
               rectTransform.anchorMax.x, rectTransform.anchorMax.y,
               rectTransform.anchorMin.x, rectTransform.anchorMin.y,
               rectTransform.sizeDelta.x, rectTransform.sizeDelta.y};
            rotation = null; ;
         } else {
            position = new float[] { saveableGameObject.transform.position.x, saveableGameObject.transform.position.y, saveableGameObject.transform.position.z };
            rotation = new float[] { saveableGameObject.transform.rotation.x, saveableGameObject.transform.rotation.y, saveableGameObject.transform.rotation.z, saveableGameObject.transform.rotation.w };
         }
      } else {
         position = null;
         rotation = null;
      }
      prefabPath = saveableGameObject.GetPrefabPath();
      components = saveableGameObject.gameObject.GetComponents<ISaveable>().Select(x => new SavedComponent(x)).ToArray();
      var parent = saveableGameObject.transform.parent;
      if (parent != null && parent.GetComponent<Saveable>() != null) {
         parentIndex = parent.GetComponent<Saveable>().GetSavedIndex();
      } else {
         parentIndex = null;
      }
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
