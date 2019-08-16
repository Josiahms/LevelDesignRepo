using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
   [SerializeField]
   private Text description;
   [SerializeField]
   private Text progress;

   private QuestObjective quest;

   public static QuestUI Instantiate(Transform parent, QuestObjective quest) {
      var instance = Instantiate(ResourceLoader.GetInstance().QuestUI, parent).GetComponent<QuestUI>();
      instance.quest = quest;
      instance.UpdateUI();
      return instance;
   }

   public void UpdateUI() {
      description.text = quest.Text;
      progress.text = "(" + quest.Amount + "/" + quest.Goal + ")";
   }
}
