using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuildDrawerButton : MonoBehaviour {

   [SerializeField]
   private GameObject buildDrawer;

   private void Start() {
      GetComponent<Button>().onClick.AddListener(() => {
         if (Placer.GetInstance().GetPlaceable() != null) {
            Placer.GetInstance().ClearPlaceable();
         } else {
            buildDrawer.SetActive(!buildDrawer.activeSelf);
         }
      });
   }
}
