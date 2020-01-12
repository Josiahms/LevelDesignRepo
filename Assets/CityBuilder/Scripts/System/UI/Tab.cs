using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tab : MonoBehaviour {

    [SerializeField]
    private Button header;
    [SerializeField]
    private GameObject content;
    [SerializeField]
    private Text text;

    public void SetManager(TabManager manager) {
        header.onClick.AddListener(() => manager.Select(this));
        Lowlight();
    }

    public void Highlight() {
        header.targetGraphic.color = Color.white;
        content.SetActive(true);
        text.color = Color.black;
    }

    public void Lowlight() {
        header.targetGraphic.color = Color.gray;
        content.SetActive(false);
        text.color = Color.white;
    }


}
