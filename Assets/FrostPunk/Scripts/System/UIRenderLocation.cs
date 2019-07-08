using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRenderLocation : Singleton<UIRenderLocation> {
   [SerializeField]
   private GenericUIRenderer genericUI;
   public GameObject GetUIPrefab() { return genericUI.gameObject; }
}
