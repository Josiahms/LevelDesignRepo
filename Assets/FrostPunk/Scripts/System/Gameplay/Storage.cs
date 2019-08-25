﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Saveable))]
[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(Placeable))]
public class Storage : MonoBehaviour, IPlaceable, ISaveable
{
   [SerializeField]
   private ResourceType type;
   [SerializeField]
   private int amount;

   [SerializeField]
   public List<GameObject> contents;
   private Resource resourceInstance;

   private void Start() {
      resourceInstance = ResourceManager.GetInstance()[type];
   }

   public void OnPlace() {
      ResourceManager.GetInstance()[type].OffsetCapacity(amount);
   }

   public void OnRemove() {
      if (GetComponent<Placeable>().IsPlaced() && ResourceManager.GetInstance() != null) {
         ResourceManager.GetInstance()[type].OffsetCapacity(-amount);
      }
   }

   private void Update() {
      var percentFull = (float)resourceInstance.Amount / resourceInstance.Capacity;
      var numEnabled = contents.Count * percentFull;
      for (int i = 0; i < contents.Count; i++) {
         contents[i].SetActive(i < numEnabled);
      }
   }
}
