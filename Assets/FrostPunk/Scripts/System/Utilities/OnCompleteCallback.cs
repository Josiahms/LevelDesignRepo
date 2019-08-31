using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IOnCompleteCallback {

   void SetOnCompleteAction(UnityAction onComplete);

}
