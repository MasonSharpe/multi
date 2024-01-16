using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartWall : MonoBehaviour
{
    private float timer = 0;

    private void Update() {
        timer += Time.deltaTime;
        
        if (timer > 5) {
            Destroy(gameObject);
        }
    }
}
