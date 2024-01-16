using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public List<PlayerNetwork> players = new();

    private void Awake() {
        DontDestroyOnLoad(this);
        instance = this;
    }
}
