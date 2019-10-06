using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuildDrawerButton : MonoBehaviour {

   [SerializeField]
   private GameObject buildDrawer;

   private void Awake() {
      GetComponent<Button>().onClick.AddListener(() => {
         if (Builder.GetInstance().GetBuilding() != null) {
            Builder.GetInstance().ClearBuilding();
         } else {
            buildDrawer.SetActive(!buildDrawer.activeSelf);
         }
      });
   }
}
