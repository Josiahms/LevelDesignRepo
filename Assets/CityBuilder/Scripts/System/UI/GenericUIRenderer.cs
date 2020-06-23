using UnityEngine;
using UnityEngine.UI;

public class GenericUIRenderer : MonoBehaviour {

   [SerializeField]
   private Text titleText;
   [SerializeField]
   private Text descriptionText;
   [SerializeField]
   private Text quantityLabel;
   [SerializeField]
   private Text quantityText;
   [SerializeField]
   private Text workerLabel;
   [SerializeField]
   private Text workerText;
   [SerializeField]
   private Button addWorkerButton;
   [SerializeField]
   private Button removeWorkerButton;
   [SerializeField]
   private Button upgradeButton;
   [SerializeField]
   private Button deleteButton;
   [SerializeField]
   private Button buildButton;

   private void Start() {
      DoUpdate();
   }

   private void Update() {
      DoUpdate();
   }

   private void DoUpdate() {
      var selectedItem = SelectionManager.GetInstance().GetFirstSelected();
      if (selectedItem == null) {
         transform.GetChild(0).gameObject.SetActive(false);
         return;
      } else {
         transform.GetChild(0).gameObject.SetActive(true);
      }

      var selectable = selectedItem.GetComponent<Selectable>();
      var placeable = selectedItem.GetComponent<Placeable>();
      var upgradeable = selectedItem.GetComponent<Upgradeable>();
      var assignable = selectedItem.GetComponent<Assignable>();
      var house = selectedItem.GetComponent<Housing>();
      var pile = selectedItem.GetComponent<Workstation>();
      var gridCenter = selectedItem.GetComponent<CentralNode>();
      var buildingOptions = selectable.GetComponent<BuildingOptions>();

      if (selectable != null) {
         titleText.text = selectable.GetItemName();
         descriptionText.text = selectable.Description;
      }
      if (placeable != null) {
         deleteButton.gameObject.SetActive(placeable.Deleteable);
      } else {
         deleteButton.gameObject.SetActive(false);
      }
      if (upgradeable != null && upgradeable.GetNextUpgrade() != null) {
         upgradeButton.gameObject.SetActive(true);
      } else {
         upgradeButton.gameObject.SetActive(false);
      }
      if (assignable != null) {
         workerText.text = assignable.GetWorkerCount() + "/" + assignable.GetMaxAssignees();
         addWorkerButton.onClick.RemoveAllListeners();
         addWorkerButton.onClick.AddListener(assignable.AddWorker);
         removeWorkerButton.onClick.RemoveAllListeners();
         removeWorkerButton.onClick.AddListener(assignable.RemoveWorker);
         workerText.gameObject.SetActive(true);
         addWorkerButton.gameObject.SetActive(true);
         removeWorkerButton.gameObject.SetActive(true);
         workerLabel.gameObject.SetActive(true);
      } else {
         workerText.gameObject.SetActive(false);
         addWorkerButton.gameObject.SetActive(false);
         removeWorkerButton.gameObject.SetActive(false);
         workerLabel.gameObject.SetActive(false);
      }
      if (house != null) {
         quantityText.text = house.Capacity.ToString();
         quantityLabel.text = "Inhabitants";
         quantityLabel.gameObject.SetActive(true);
         quantityText.gameObject.SetActive(true);
      } else {
         quantityLabel.gameObject.SetActive(false);
         quantityText.gameObject.SetActive(false);
      }
      if (pile != null && pile.GetQuantity() != -1) {
         quantityText.text = pile.GetQuantity().ToString();
         quantityLabel.text = "Quantity";
         quantityLabel.gameObject.SetActive(true);
         quantityText.gameObject.SetActive(true);
      } else {
         quantityLabel.gameObject.SetActive(false);
         quantityText.gameObject.SetActive(false);
      }
      if (buildingOptions != null) {
         buildButton.gameObject.SetActive(true);
      } else {
         buildButton.gameObject.SetActive(false);
      }
   }
}
