using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManagerHUD : MonoBehaviour
{
    public NetworkManager manager;
    private int offsetX;
    private int offsetY;

    void Awake()
    {
        manager = GetComponent<NetworkManager>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        int xPos = 10 + offsetX;
        int yPos = 40 + offsetY;
        int space = 24;

        bool noConnection = (manager.client == null || manager.client.connection == null ||
                                 manager.client.connection.connectionId == -1);

        if (!manager.IsClientConnected() && !NetworkServer.active && manager.matchMaker == null)
        {
            if (noConnection)
            {
                if (UnityEngine.Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    if (GUI.Button(new Rect(xPos, yPos, 200, 20), "LAN Host(H)"))
                    {
                        manager.StartHost();
                        // GameObject.Find("Team1").gameObject.GetComponent<Team>().robotsToSpawn = 3;
                    }
                    yPos += space;
                }

                if (GUI.Button(new Rect(xPos, yPos, 105, 20), "LAN Client(C)"))
                {
                    manager.StartClient();
                }

                manager.networkAddress = GUI.TextField(new Rect(xPos + 100, yPos, 95, 20), manager.networkAddress);
                yPos += space;

                if (UnityEngine.Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    // cant be a server in webgl build
                    GUI.Box(new Rect(xPos, yPos, 200, 25), "(  WebGL cannot be server  )");
                    yPos += space;
                }
                else
                {
                    if (GUI.Button(new Rect(xPos, yPos, 200, 20), "LAN Server Only(S)"))
                    {
                        manager.StartServer();
                    }
                    yPos += space;
                }
            }
            else
            {
                GUI.Label(new Rect(xPos, yPos, 200, 20), "Connecting to " + manager.networkAddress + ":" + manager.networkPort + "..");
                yPos += space;


                if (GUI.Button(new Rect(xPos, yPos, 200, 20), "Cancel Connection Attempt"))
                {
                    manager.StopClient();
                }
            }
        }
        else
        {
            if (NetworkServer.active)
            {
                string serverMsg = "Server: port=" + manager.networkPort;
                if (manager.useWebSockets)
                {
                    serverMsg += " (Using WebSockets)";
                }
                GUI.Label(new Rect(xPos, yPos, 300, 20), serverMsg);
                yPos += space;
            }
            if (manager.IsClientConnected())
            {
                GUI.Label(new Rect(xPos, yPos, 300, 20), "Client: address=" + manager.networkAddress + " port=" + manager.networkPort);
                yPos += space;
            }
        }

        if (manager.IsClientConnected() && !ClientScene.ready)
        {
            if (GUI.Button(new Rect(xPos, yPos, 200, 20), "Client Ready"))
            {
                ClientScene.Ready(manager.client.connection);

                if (ClientScene.localPlayers.Count == 0)
                {
                    ClientScene.AddPlayer(0);
                }
            }
            yPos += space;
        }

        if (NetworkServer.active || manager.IsClientConnected())
        {
            if (GUI.Button(new Rect(xPos, yPos, 200, 20), "Stop (X)"))
            {
                manager.StopHost();
            }
            yPos += space;
        }
    }
}
