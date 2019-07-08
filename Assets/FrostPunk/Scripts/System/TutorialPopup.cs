using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TutorialPopup : MonoBehaviour {

   [SerializeField]
   private Button doneButton;
   [SerializeField]
   private Text message;

   public static TutorialPopup Instantiate(string text, UnityAction onClose = null, bool faceDown = false) {
      return Instantiate(null, text, onClose, faceDown);
   }

   public static TutorialPopup Instantiate(RectTransform target, string text, UnityAction onClose = null, bool faceDown = false) {
      var prefab = faceDown ? ResourceLoader.GetInstance().TutorialPopupDown : ResourceLoader.GetInstance().TutorialPopup;
      var instance = Instantiate(prefab, MainCanvas.Get().transform);
      instance.message.text = text;
      instance.doneButton.onClick.AddListener(() => {
         Destroy(instance.gameObject);
      });
      if (onClose != null) {
         instance.doneButton.onClick.AddListener(onClose);
      }
      if (target != null) {
         instance.GetComponent<RectTransform>().position = target.position;
      }
      return instance;
   }
}
