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
   [SerializeField]
   private Button chargeUp;
   [SerializeField]
   private Button chargeForward;
   [SerializeField]
   private Button chargeDown;

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
      var workstation = selectedItem.GetComponent<Workstation>();
      var house = selectedItem.GetComponent<Housing>();
      var pile = selectedItem.GetComponent<Workstation>();
      var gridCenter = selectedItem.GetComponent<CentralNode>();
      var buildingOptions = selectedItem.GetComponent<BuildingOptions>();
      var waypoint = selectedItem.GetComponent<Waypoint>();

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
      if (workstation != null) {
         var assignable = workstation.GetComponent<Assignable>();
         workerText.text = assignable.GetAssigneeCount() + "/" + assignable.GetMaxAssignees();
         addWorkerButton.onClick.RemoveAllListeners();
         addWorkerButton.onClick.AddListener(workstation.AddWorker);
         removeWorkerButton.onClick.RemoveAllListeners();
         removeWorkerButton.onClick.AddListener(workstation.RemoveWorker);
         workerText.gameObject.SetActive(true);
         addWorkerButton.gameObject.SetActive(true);
         removeWorkerButton.gameObject.SetActive(true);
         workerLabel.gameObject.SetActive(true);
         workerLabel.text = "Workers";
      } else {
         workerText.gameObject.SetActive(false);
         addWorkerButton.gameObject.SetActive(false);
         removeWorkerButton.gameObject.SetActive(false);
         workerLabel.gameObject.SetActive(false);
      }
      /*if (waypoint != null) {
         var assignable = workstation.GetComponent<Assignable>();
         workerText.text = assignable.GetAssigneeCount() + "/" + assignable.GetMaxAssignees();
         addWorkerButton.onClick.RemoveAllListeners();
         addWorkerButton.onClick.AddListener(waypoint.AddSoldier);
         removeWorkerButton.onClick.RemoveAllListeners();
         removeWorkerButton.onClick.AddListener(waypoint.RemoveSoldier);
         workerText.gameObject.SetActive(true);
         addWorkerButton.gameObject.SetActive(true);
         removeWorkerButton.gameObject.SetActive(true);
         workerLabel.gameObject.SetActive(true);
         workerLabel.text = "Soliders";
      } else {
         workerText.gameObject.SetActive(false);
         addWorkerButton.gameObject.SetActive(false);
         removeWorkerButton.gameObject.SetActive(false);
         workerLabel.gameObject.SetActive(false);
      }*/
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
