using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabManager : MonoBehaviour {

    private Tab selectedTab;

    private void Start() {
        var tabs = GetComponentsInChildren<Tab>(true);
        foreach (var tab in tabs) {
            tab.gameObject.SetActive(true); // So that I can disable them in the editor
            tab.SetManager(this);
        }

        if (tabs.Length > 0) {
            Select(tabs[0]);
        }
    }

    public void Select(Tab tab) {
        if (selectedTab != null) {
            selectedTab.Lowlight();
        }

        selectedTab = tab;
        tab.Highlight();
    }

}
