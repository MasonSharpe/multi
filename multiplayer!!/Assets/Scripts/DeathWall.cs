using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathWall : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == 7)
        {
            PlayerNetwork player = collision.transform.parent.GetComponent<PlayerNetwork>();
            if (player.IsOwner && player.placement.Value == -1 && !player.desiresDeath) {
                SceneManagement.instance.HServerRpc((int)player.OwnerClientId);
            }
        }

    }
}
