using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class DeathEntry : MonoBehaviour
{
    public string deathText = "";
    private float timer = 0;
    public TextMeshProUGUI text;

    private void Start() {
        text = GetComponent<TextMeshProUGUI>();
        text.text = deathText;
        Destroy(gameObject, 3);

    }
    private void Update() {
        timer += Time.deltaTime;
        if (timer > 2) {
            text.color = new Color(1, 1, 1, 1 - (timer - 2));
        }
    }
}
