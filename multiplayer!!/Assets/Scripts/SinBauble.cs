using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinBauble : MonoBehaviour
{
    public float amount;
    public float speed = 1;
    private void Update() {
        transform.localPosition = new Vector3(0, amount * Mathf.Sin(Time.time * speed));
    }
}
