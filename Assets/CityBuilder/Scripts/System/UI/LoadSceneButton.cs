using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LoadSceneButton : MonoBehaviour {

   [SerializeField]
   private int buildIndex;
   [SerializeField]
   private UnityEvent beforeChange;

   private void Awake() {
      GetComponent<Button>().onClick.AddListener(beforeChange.Invoke);
      GetComponent<Button>().onClick.AddListener(() => { SceneManager.LoadScene(buildIndex, LoadSceneMode.Single); });
   }

}
