using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class EndDayButton : MonoBehaviour
{
   private Button btn;


   private void Start() {
      btn = GetComponent<Button>();
      btn.onClick.AddListener(DayCycleManager.GetInstance().EndWorkDay);
   }

   private void Update() {
      btn.interactable = DayCycleManager.GetInstance().CurrentTimeOfDay > DayCycleManager.MIN_END_WORK_DAY
         && DayCycleManager.GetInstance().IsWorkDay();
   }

}
