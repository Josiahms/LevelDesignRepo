using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
[RequireComponent(typeof(Rigidbody))]
public class FloatingText : MonoBehaviour {

   public static FloatingText Instantiate(Transform parent, string text, bool up = true, float speed = 0.75f, float lifetime = 1.5f, Color? color = null) {
      var instance = Instantiate(ResourceLoader.GetInstance().FloatingTextPrefab, parent);
      var rb = instance.GetComponent<Rigidbody>();
      var txt = instance.GetComponent<Text>();
      rb.useGravity = false;
      rb.velocity = instance.transform.up * speed * (up ? 1 : -1);
      txt.text = text;
      txt.color = color.HasValue ? color.Value : new Color(1, 0.7534726f, 0);
      Destroy(instance.gameObject, lifetime);
      return instance;
   }

   public static FloatingText InstantiateNumber(Transform parent, int amount, string postfix, bool? up = null) {
      if (amount > 0) {
         return Instantiate(parent, amount + " " + postfix, up.GetValueOrDefault(true));
      } else {
         var instance = Instantiate(parent, amount + " " + postfix, up.GetValueOrDefault(false), 0.6f, 1f, new Color(1, 0.2953345f, 0));
         instance.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
         return instance;
      }
   }

   public static FloatingText Instantiate(Transform parent, int number, string postfix, bool up, bool good, float scale) {
      var instance = Instantiate(ResourceLoader.GetInstance().FloatingTextPrefab, parent);
      var rb = instance.GetComponent<Rigidbody>();
      var txt = instance.GetComponent<Text>();
      rb.useGravity = false;
      rb.velocity = instance.transform.up * 0.75f * (up ? 1 : -1) * scale * 100;
      txt.text = (number > 0 ? "+" : "") + number + " " + postfix;
      txt.color = good ? new Color(1, 0.7534726f, 0) : new Color(1, 0.2953345f, 0);
      instance.transform.localScale = Vector3.one * scale;
      Destroy(instance.gameObject, 1.5f);
      return instance;
   }

}
