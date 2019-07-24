using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DeleteButton : MonoBehaviour {

   private Button btn;

   private void Awake() {
      btn = GetComponent<Button>();
      btn.onClick.AddListener(Delete);
   }

   private void Delete() {
      var selected = Selectable.GetSelected();
      if (selected != null) {
         selected.GetComponent<Placeable>().Remove();
      }
   }

}
