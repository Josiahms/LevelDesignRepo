using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPlayer : Singleton<Tutorial>, ISaveable {

   private List<string> tutorialMessages = new List<string> {
      "Hello there!  Welcome to the village.  It's your duty to manage the workers here to ensure they have the resources they need to survive.  If you are successful with this we can expand!",
      ""
   };

   public object OnSave() {
      return null;
   }

   public void OnLoad(object data) {
      // Ignored
   }

   public void OnLoadDependencies(object data) {
      // Ignored
   }
}
