using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotate : MonoBehaviour
{
    public float startTime;
    public float directionDegrees;

    private float timer = 0;



    private void Update() {
        timer += Time.deltaTime;

        if (timer > startTime) transform.Rotate(Vector3.forward * directionDegrees * Time.deltaTime);
    }
}
