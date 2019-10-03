using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Just here to help check if a class derives from singleton while also being generic
public interface ISingleton { }

public class Singleton<T> : MonoBehaviour, ISingleton {

   private static GameObject instance;

   protected void Awake() {
      if (instance != null) {
         if (!Application.isEditor) {
            Destroy(gameObject);
         }
      } else {
         instance = gameObject;
      }
   }

   public static GameObject GetGameObject() {
      return instance;
   }

   public static T GetInstance() {
      return instance == null ? default(T) : instance.GetComponent<T>();
   }
}
