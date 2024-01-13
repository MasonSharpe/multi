using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public TextMeshProUGUI rocketText;
    public Image rocketBackground;

    private void Update() {
        if (PlayerMovement.instance == null) return;
        print("dsa");
        float fuel = PlayerMovement.instance.rocketTimer;

        rocketText.text = ((int)Mathf.Clamp(fuel * 5f, 0, 100) + "%").ToString();

        if (fuel > 0) {
            rocketBackground.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            rocketText.color = new Color(1f, 1f, 1f, 1f);

        } else {
            rocketBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            rocketText.color = new Color(1f, 1f, 1f, 0.3f);
        }
    }
}
