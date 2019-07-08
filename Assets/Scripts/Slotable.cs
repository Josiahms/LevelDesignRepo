using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Slotable : MonoBehaviour {

   [SerializeField]
   private new string name;
   [SerializeField]
   private Sprite sprite;

   public string GetName() { return name; }

   public Sprite GetSprite() { return sprite; }

   public abstract void OnClick(Slot slot);

   public abstract void OnDoubleClick(Slot slot);

   public abstract void OnRightClick(Slot slot);

}
