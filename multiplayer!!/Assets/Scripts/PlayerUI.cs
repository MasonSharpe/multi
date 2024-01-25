using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerUI : MonoBehaviour
{
    public TextMeshProUGUI rocketText;
    public Image rocketBackground;
    public PlayerMovement movement;
    public PlayerNetwork player;
    public Transform leaderboard;
    public TextMeshProUGUI playerCount;
    public Transform deathFeed;
    public DeathEntry killPrefab;
    public Transform killHolder;
    public TextMeshProUGUI spectateText;
    public Image fade;
    public GameObject pauseScreen;

    private float fadeTimer = -1;

    private void Update() {
        float fuel = player.spectating ? player.players[player.specIndex].movement.rocketTimer.Value : movement.rocketTimer.Value;
        rocketText.text = ((int)Mathf.Clamp(fuel * 5f, 0, 100) + "%").ToString();

        if (fuel > 0) {
            rocketBackground.color = new Color(1f, 1f - fuel / 20f, 1f - fuel / 20f, 1f);
            rocketText.color = new Color(1f, 1f, 1f, 1f);

        } else {
            rocketBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            rocketText.color = new Color(1f, 1f, 1f, 0.3f);
        }

        if (fadeTimer != -1)
        {
            fadeTimer += Time.deltaTime;
            if (fadeTimer < 1.6f)
            {
                fade.color = new Color(0, 0, 0, fadeTimer);
            }
            else
            {
                if (fadeTimer < 2.6f)
                {
                    fade.color = new Color(0, 0, 0, 2.6f - fadeTimer);
                }
                else
                {
                    fadeTimer = -1;
                    fade.color = new Color(0, 0, 0, 0);
                }
            }
        }
    }

    public void Fade()
    {
        fadeTimer = 0;
    }

    public void Resume()
    {
        player.source.PlayOneShot(player.clips[9], 0.7f);
        pauseScreen.SetActive(false);
    }
    public void Disconnect()
    {
        player.source.PlayOneShot(player.clips[9], 0.7f);
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("Lobby Scene");
    }

    public void ToggledMusic()
    {
        player.source.PlayOneShot(player.clips[9], 0.7f);
        player.musicOn = !player.musicOn;
    }
}
