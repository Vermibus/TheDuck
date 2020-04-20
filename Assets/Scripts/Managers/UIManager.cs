using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

public class UIManager : MonoBehaviour {

    public static UIManager instance;

    public Text playerVelocityText;
    public Text dampenerInfo;

    void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
        } else {
            instance = this;
        }
    }

    public void SetPlayerVelocity(float3 velocity) {
        playerVelocityText.text = string.Format("Velocity: {0}\nX: {1}\nY: {2}\nZ: {3}", 
            Vector3.Magnitude(velocity),
            velocity.x,
            velocity.y,
            velocity.z
        );
    }

    public void SetDampenerInfo(bool dampener) {
        dampenerInfo.enabled = dampener;
    }
}
