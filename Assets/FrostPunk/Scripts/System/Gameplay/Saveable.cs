using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface ISaveable {
   void OnLoad(object data);
   void OnLoadDependencies(object data);
}

public class Saveable : MonoBehaviour {

   [SerializeField]
   private string prefabPath;
   public string GetPrefabPath() { return prefabPath; }

   [SerializeField]
   private bool savePosition = true;
   public bool SavePosition { get { return savePosition; } }

   private static int sharedIndex;
   private int savedIndex;

   public int GetSavedIndex() {
      return savedIndex;
   }

   private void Awake() {
      savedIndex = Interlocked.Increment(ref sharedIndex);
      var saveExists = File.Exists(Application.persistentDataPath + "/game.fun");
      if (!SceneManager.GetActiveScene().isLoaded && saveExists && GetComponent<ISingleton>() == null) {
         Destroy(gameObject);
      } else {
         SaveManager.GetInstance().RegisterEntity(this);
      }
   }

   private void OnDestroy() {
      if (SaveManager.GetInstance() != null) {
         SaveManager.GetInstance().RemoveEntity(this);
      }
   }

}