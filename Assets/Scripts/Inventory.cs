using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Inventory {

   [SerializeField]
   private int maxCapacity;
   [SerializeField]
   private List<Slotable> items;

   private Slotable[] transient;
   private bool begun = false;

   public void Init() {
      maxCapacity = Mathf.Max(items.Count, maxCapacity);
      while (items.Count < maxCapacity) {
         items.Add(null);
      }
   }

   public bool Begin() {
      if (begun) {
         return false;
      }
      transient = new Slotable[items.Count];
      items.CopyTo(transient);
      begun = true;
      return true;
   }

   public bool Commit() {
      if (!begun) {
         return false;
      }
      items.Clear();
      items.AddRange(transient);
      begun = false;
      return true;
   }

   public void Rollback() {
      transient = null;
      begun = false;
   }

   public bool ContainsAll(IEnumerable<ItemSlotPair> items) {
      return items.All(x => {
         if (x.slotIndex == -1) {
            return transient.Contains(x.item);
         } else {
            return transient[x.slotIndex] == x.item;
         }
      });
   }

   public bool AddAll(List<ItemSlotPair> items) {
      return items.All(x => {
         if (x.slotIndex == -1) {
            return Add(x.item);
         } else {
            return AddAt(x.slotIndex, x.item);
         }
      });
   }

   public bool RemoveAll(List<ItemSlotPair> items) {
      return items.All(x => {
         if (x.slotIndex == -1) {
            return Remove(x.item);
         } else {
            return ReplaceAt(x.slotIndex, null);
         }
      });
   }

   public Slotable GetAt(int index) {
      try {
         return items[index];
      } catch (Exception e) {
         return null;
      }
   }

   public bool AddAt(int index, Slotable item) {
      if (index < 0 || index >= transient.Length || transient[index] != null) {
         return false;
      }
      return ReplaceAt(index, item);
   }

   public int GetFirstOpenIndex() {
      return items.IndexOf(null);
   }

   public bool ReplaceAt(int index, Slotable item) {
      if (index >= 0 && index < transient.Length) {
         transient[index] = item;
         return true;
      }
      return false;
   }

   public bool Remove(Slotable item) {
      for (int i = 0; i < transient.Length; i++) {
         if (transient[i] == item) {
            transient[i] = null;
            return true;
         }
      }
      return false;
   }

   public bool Add(Slotable item) {
      for (int i = 0; i < transient.Length; i++) {
         if (transient[i] == null) {
            transient[i] = item;
            return true;
         }
      }
      return false;
   }
}
