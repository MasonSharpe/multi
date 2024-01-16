using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishWall : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision) {
        collision.transform.parent.GetComponent<PlayerNetwork>().ReachFinish();
    }
}
