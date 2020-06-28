using UnityEngine;

public class FrameRateLimit : MonoBehaviour {

    [SerializeField]
    private int targetFrameRate = 30;

    private void Start() {
        Application.targetFrameRate = targetFrameRate;
    }

}