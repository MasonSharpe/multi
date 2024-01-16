using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathWall : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision) {
        PlayerNetwork player = collision.transform.parent.GetComponent<PlayerNetwork>();
        player.Invoke(nameof(player.Die), Random.Range(0f, 0.5f));
    }
}
