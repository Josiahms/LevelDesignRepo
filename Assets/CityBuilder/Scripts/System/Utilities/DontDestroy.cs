using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
   private static List<Object> objects = new List<Object>();

   private void Awake() {
      DontDestroyOnLoad(gameObject);
      objects.Add(gameObject);
   }

   public static void DestroyAll() {
      objects.ForEach(x => Destroy(x));
      objects.Clear();
   }
}
