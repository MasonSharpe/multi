using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System.Net;
using Unity.Netcode.Transports.UTP;



public class NetworkManagerUI : MonoBehaviour
{
    public Button server;
    public Button host;
    public Button client;

    public TextMeshProUGUI hostText;
    public TMP_InputField ipInput;

    public UnityTransport transport;


    private void Awake()
    {
        hostText.enabled = false;
        host.onClick.AddListener(() =>
        {
            string IP = GetIP();
            transport.SetConnectionData("127.0.0.1", 7777);
            NetworkManager.Singleton.StartHost();

            hostText.text = "Host IP: " + IP;
            hostText.enabled = true;
        });
        client.onClick.AddListener(() =>
        {
            
             transport.SetConnectionData("127.0.0.1", 7777);// "127.0.0.1"     ipInput.text

            NetworkManager.Singleton.StartClient();

        });
    }
    private string GetIP() {
        var strHostName = Dns.GetHostName();
        var ipEntry = Dns.GetHostEntry(strHostName);
        var addr = ipEntry.AddressList;
        return addr[0].ToString();
    }
}
