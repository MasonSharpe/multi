using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System;

public class PlayerNetwork : NetworkBehaviour
{
    /*IDEAS
    pretty much fall guys. no collision, ghost players. Make basic obstacle cources, make a leaderboard for who finishes the fastest.
    have a pulse ability on a long cooldown that can apply a knockback force to players. some maps can having rising lava
    for dynamic obstacles, rely on client transforms. have usernames above heads, and a lobby for when you aren't connected
    also have cosmetic options in the lobby. game could reset after a certain number of rounds if i have time. rocket is a circle that spawns
    and despawns very quickly, force based on distance to center and charge. put in timer based coyote time, incentive kills
    */


    private NetworkVariable<Color> color = new(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> health = new(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> knifeTimer = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {




        base.OnNetworkSpawn();
    }
    private void Update()
    {
        GetComponentInChildren<SpriteRenderer>().color = color.Value;



        if (!IsOwner) return;


        if (Input.GetKeyDown(KeyCode.E))
        {
            color.Value = new Color(UnityEngine.Random.Range(0f, 1), UnityEngine.Random.Range(0f, 1), UnityEngine.Random.Range(0f, 1));

        }



    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner) return;

    }
}
