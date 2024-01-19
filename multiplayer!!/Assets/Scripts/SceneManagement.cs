using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : NetworkBehaviour {
    public static SceneManagement instance;
    public PolygonCollider2D bounds;
    private List<PlayerNetwork> players = new();
    public string objective;
    public AudioSource source;
    private bool songPlayed = false;

    private void Awake() {
        instance = this;
    }
    private void Update() {
        if (!songPlayed && players.Count != 0 && players[0].timeInRound > 5 && SceneManager.GetActiveScene().name != "Lobby Scene") {
            source.Play();
            songPlayed = true;
        }
    }
    private void Start() {
        if (!IsServer) return;

        players = FindObjectsOfType<PlayerNetwork>().ToList();
        foreach (PlayerNetwork player in players) {
            player.StartRaceClientRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void EndRaceServerRpc() {
        if (!IsServer) return;

        foreach (PlayerNetwork player in players) {
            player.EndRaceClientRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void HandleDeathServerRpc() {
        if (!IsServer) return;
        foreach (PlayerNetwork player in players) {
            player.HandleDeathClientRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void ResetKillerServerRpc(int id) {
        if (!IsServer) return;
        foreach (PlayerNetwork player in players) {
            player.ResetKillerClientRpc(id);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void PlayPingServerRpc(int id)
    {
        if (!IsServer) return;
        if (players.Count == 0) players = FindObjectsOfType<PlayerNetwork>().ToList();
        foreach (PlayerNetwork player in players)
        {
            player.PlayPingClientRpc(id);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void HandleWinServerRpc(int id) {
        if (!IsServer) return;
        foreach (PlayerNetwork player in players) {
            player.HandleWinClientRpc(id);
        }
    }
}
