using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System.Net;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class NetworkManagerUI : NetworkBehaviour
{
    public static NetworkManagerUI instance;

    public Button server;
    public Button host;
    public Button client;

    public TextMeshProUGUI hostText;
    public TMP_InputField ipInput;
    public TMP_InputField usernameInput;

    public UnityTransport transport;


    private void Awake()
    {
        instance = this;
        hostText.enabled = false;
        host.onClick.AddListener(() =>
        {
            string IP = GetIP();
            transport.SetConnectionData(IP, 7777);
            NetworkManager.Singleton.StartHost();

            hostText.text = "Host IP: " + IP;
            hostText.enabled = true;


        });
        client.onClick.AddListener(() =>
        {
            
            transport.SetConnectionData(ipInput.text, 7777);// "127.0.0.1"     ipInput.text

            NetworkManager.Singleton.StartClient();
            //if (!NetworkManager.Singleton.IsConnectedClient)
           // {
               // NetworkManager.Singleton.Shutdown();
            //}

        });
    }
    private string GetIP() {
        var strHostName = Dns.GetHostName();
        var ipEntry = Dns.GetHostEntry(strHostName);
        var addr = ipEntry.AddressList;
        return addr[0].ToString();
    }
    public void Quit()
    {
        Application.Quit();
    }
}
