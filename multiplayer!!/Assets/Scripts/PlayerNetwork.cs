using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System;
using Cinemachine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Collections;
using System.Linq;

public class PlayerNetwork : NetworkBehaviour
{
    


    private NetworkVariable<Color> color1 = new(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<Color> color2 = new(new Color(0.92f, 0.58f, 0.58f), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<FixedString32Bytes> username = new("Unnamed Rocket", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> placement = new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> points = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> kills = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> killer = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public PlayerUI UI;
    public TextMeshProUGUI visibleUsername;
    public CinemachineVirtualCamera cam;
    public CinemachineConfiner2D confiner;
    public SpriteRenderer sprite1;
    public SpriteRenderer sprite2;
    public SpriteRenderer sprite3;
    public TextMeshProUGUI countdown;
    public PlayerMovement movement;
    public TextMeshProUGUI objectiveText;
    public SceneManagement sceneManagement;
    public TextMeshProUGUI leaderboardEntry;
    public List<PlayerNetwork> players;
    public AudioClip[] clips;
    public AudioSource source;

    public float timeInRound = 0;
    public bool spectating = false;
    public int specIndex = 0;
    public bool roundInProgress = false;
    public float invincibilityTimer = 0;

    /*SONGS
     * Lobby song: 25
     * Short songs: 3!, 17, 33, 31, 21!
     * Medium songs: 
     * Long songs: 4!, 10, 20, 22!, 23, 24, 29, 30, 32, 34
     */

    /*LEVElS
     * Short: 4/5
     * small platforms, lava closing in all sides       extreme: same but one tiny platform       lava rising       big arena, lava blocks everywhere
     * Long: 9/10
     * hole in the wall,  default obby      extreme default       see-saw       lava rising chill       building climbing       tight jumps     catch up to a running goal
     * moving platforms everywhre       
     */

    public override void OnNetworkSpawn()
    {
        if (IsOwner) {
            UI.gameObject.SetActive(true);
            cam.Priority = 11;
            confiner.m_BoundingShape2D = SceneManagement.instance.bounds;
            string text = NetworkManagerUI.instance.usernameInput.text;
            username.Value = text == "" ? "Unnamed Rocket " + OwnerClientId : text;
            sprite1.sortingOrder = 5;
            sprite2.sortingOrder = 6;
            sprite3.sortingOrder = 7;
            visibleUsername.color = Color.yellow;
            source.PlayOneShot(clips[9]);
            
            if (SceneManager.GetActiveScene().name != "Lobby Scene") {
                StartSpectating();
                UI.spectateText.text = "you joined late! just chill here                       until the round ends, thanks <3";
            }
        }


        base.OnNetworkSpawn();
    }

    private void Update()
    {
        sprite1.color = color1.Value;
        sprite2.color = color2.Value;
        visibleUsername.text = username.Value.ToString();
        timeInRound += Time.deltaTime;
        invincibilityTimer -= Time.deltaTime;

        if (sceneManagement == null) {
            sceneManagement = FindObjectOfType<SceneManagement>(); 
            if (sceneManagement != null) confiner.m_BoundingShape2D = sceneManagement.bounds;
        }


        if (!IsOwner)
        {
            sprite1.color = new Color(color1.Value.r, color1.Value.g, color1.Value.b, 0.7f);
            sprite2.color = new Color(color2.Value.r, color2.Value.g, color2.Value.b, 0.7f);
            sprite3.color = new Color(1, 1, 1, 0.7f);
            return;
        }

        if (!spectating) {
            cam.Priority = 11;
            string time = timeInRound < 5 ? "" : (timeInRound - 5).ToString("F2");
            if (sceneManagement != null) objectiveText.text = sceneManagement.objective + "\r\n" + time;

        } else if (players[specIndex].placement.Value != -1 && roundInProgress) {
            CycleSpectator();
        }

        if (timeInRound > 2 && SceneManager.GetActiveScene().name != "Lobby Scene") {
            countdown.enabled = true;
            string text = Mathf.Floor(4 - (timeInRound - 2)).ToString();
            if (!text.Equals(countdown.text) && timeInRound < 5.5f) source.PlayOneShot(clips[2]);
            countdown.text = text;
            if (timeInRound > 5) {
                countdown.enabled = false;
            }
        }

        if (SceneManager.GetActiveScene().name == "Lobby Scene") {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                color1.Value = new Color(UnityEngine.Random.Range(0f, 1), UnityEngine.Random.Range(0f, 1), UnityEngine.Random.Range(0f, 1));
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                color2.Value = new Color(UnityEngine.Random.Range(0f, 1), UnityEngine.Random.Range(0f, 1), UnityEngine.Random.Range(0f, 1));
            }
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            if (spectating) CycleSpectator();
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            if (IsServer) {

                NetworkManager.Singleton.SceneManager.LoadScene("Level1", LoadSceneMode.Single);
            }
        }

    }

    
    public void ReachFinish() {
        if (IsOwner && !spectating && invincibilityTimer < 0) {
            source.PlayOneShot(clips[9]);
            placement.Value = 0;
            int allVictorious = 0;
            foreach (PlayerNetwork player in players) {

                if (player.placement.Value == placement.Value && player != this) {
                    placement.Value++;
                }

                if (player.placement.Value != -1) {
                    allVictorious++;
                }
            }
            StartSpectating();

            if (allVictorious >= players.Count - 1) {
                sceneManagement.EndRaceServerRpc();
            }
        }



    }

    [ClientRpc]
    public void HandleDeathClientRpc() {
        if (!IsOwner) return;
            foreach (PlayerNetwork player in players) {
            if ((ulong)player.killer.Value == OwnerClientId && player.OwnerClientId != OwnerClientId) {
                kills.Value++;
                points.Value++;
                sceneManagement.ResetKillerServerRpc((int)player.OwnerClientId);
                print("Asdsad");
                source.PlayOneShot(clips[7], 0.5f);
            }

        }

        
    }
    [ClientRpc]
    public void ResetKillerClientRpc(int id) {
        if (!IsOwner) return;
        if (id == (int)OwnerClientId) killer.Value = -1;
    }
    [ClientRpc]
    public void PlayPingClientRpc(int id)
    {
        if (!IsOwner) return;
        if (id == (int)OwnerClientId) source.PlayOneShot(clips[5]);
    }

    [ClientRpc]
    public void EndRaceClientRpc() {
        if (!IsOwner || !roundInProgress) return;
        roundInProgress = false;
        if (placement.Value == -1) {

            placement.Value = players.Count - 1;
            foreach (PlayerNetwork player in players) {
                if (player.placement.Value == placement.Value && player != this) {
                    placement.Value--;
                }
            }
            movement.rb.velocity = Vector2.zero;
            movement.rocketHorizontalVelocity = 0;
            StartSpectating();
        }

        int reward = placement.Value switch {
            0 => 6,
            1 => 3,
            2 => 2,
            _ => 1
        };
        points.Value += reward;


        if (IsServer) {
            foreach (PlayerNetwork player in players) player.Invoke(nameof(EndRaceShowClientRpc), 0.1f);
        }
    }

    [ClientRpc]
    public void EndRaceShowClientRpc() {
        players.Sort((a, b) => b.points.Value.CompareTo(a.points.Value));
        for (int i = 0; i < players.Count; i++) {
            TextMeshProUGUI entry = Instantiate(leaderboardEntry, UI.leaderboard);
            string suffix = i switch {
                0 => "st",
                1 => "nd",
                2 => "rd",
                _ => "th"
            };
            entry.text = (i + 1) + suffix + ": " + players[i].username.Value + " - " + players[i].points.Value + " Points (" + players[i].kills.Value + " kills)";
            if (players[i].OwnerClientId == OwnerClientId) entry.color = Color.yellow;       
        }
        UI.leaderboard.transform.parent.gameObject.SetActive(true);

        Invoke(nameof(GoToNextRace), 5f);
    }



    public void GoToNextRace() {
        if (IsServer && IsOwner) NetworkManager.Singleton.SceneManager.LoadScene("Level1", LoadSceneMode.Single);

    }

    [ClientRpc]
    public void StartRaceClientRpc() {
        transform.position = Vector3.zero;
        countdown.text = "";
        timeInRound = 0;
        objectiveText.gameObject.SetActive(true);
        movement.rb.velocity = Vector3.zero;
        movement.rocketHorizontalVelocity = 0;
        movement.timeInRound = 0;
        invincibilityTimer = 5;
        roundInProgress = true;
        if (IsOwner) {
            placement.Value = -1;
            killer.Value = -1;
            movement.rocketTimer.Value = 0;
        }
        spectating = false;
        cam.Priority = 10;

        foreach (Transform child in UI.leaderboard.GetComponentsInChildren<Transform>()) {
            if (child != UI.leaderboard) Destroy(child.gameObject);
        }

        UI.leaderboard.transform.parent.gameObject.SetActive(false);
        UI.spectateText.transform.parent.gameObject.SetActive(false);

        players = FindObjectsOfType<PlayerNetwork>().ToList();
        source.PlayOneShot(clips[8]);
    }



    public void Die() {
        if (IsOwner && invincibilityTimer < 0 && roundInProgress) {
            movement.rb.velocity = Vector3.zero;
            placement.Value = players.Count - 1;
            invincibilityTimer = 9999;
            source.PlayOneShot(clips[3]);

            int allVictorious = 0;
            foreach (PlayerNetwork player in players) {

                if (player.placement.Value == placement.Value && player != this) {
                    placement.Value--;
                }

                if (player.placement.Value != -1) {
                    allVictorious++;
                }
            }
            StartSpectating();
            sceneManagement.HandleDeathServerRpc();

            if (allVictorious >= players.Count - 1) {
                sceneManagement.Invoke(nameof(sceneManagement.EndRaceServerRpc), 0.1f);
            }
        }
    }

    private void CycleSpectator() {
        int prevIndex = specIndex;
        int i = 0;

        specIndex = (int)Mathf.Repeat(specIndex + 1, players.Count);
        while (players[specIndex].placement.Value != -1 && i < players.Count) {
            specIndex = (int)Mathf.Repeat(specIndex + 1, players.Count);

            i++;
        }
        if (i < players.Count) {

            players[prevIndex].cam.Priority = 10;
            players[specIndex].cam.Priority = 11;
            UI.spectateText.text = "Spectating: " + players[specIndex].username.Value + "\r\n(Q to switch)";
        }

    }

    private void StartSpectating() {
        spectating = true;
        UI.spectateText.transform.parent.gameObject.SetActive(true);
        CycleSpectator();
    }
}
