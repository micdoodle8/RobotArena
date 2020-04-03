using UnityEngine;
using UnityEngine.Networking;

public enum NetworkCreateMode {
    NONE,
    CREATE,
    JOIN
}

public class MyNetworkManager : NetworkManager
{
    public static NetworkCreateMode startMode = NetworkCreateMode.NONE; 
    private bool initialized = false;

    void Update() {
        if (!initialized) {
            switch (startMode) {
                case NetworkCreateMode.NONE:
                    break;
                case NetworkCreateMode.CREATE:
                    StartHost();
                    break;
                case NetworkCreateMode.JOIN:
                    StartClient();
                    break;
            }
            initialized = true;
        }
    }

    private void OnServerInitialized() {
    }

    //public GameObject sdkManager;
    // public static short controllers = 0;

    // public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    // {
    //     // Transform startPos = GetStartPosition();
    //     Team[] teams = GameObject.FindObjectsOfType<Team>();
    //     VRPlayerController[] players = GameObject.FindObjectsOfType<VRPlayerController>();
    //     Team leastTeam = null;
    //     int max = int.MaxValue;
    //     foreach (Team t in teams) {
    //         int count = 0;
    //         foreach (VRPlayerController p in players) {
    //             if (("Team" + p.teamID) == t.gameObject.name) {
    //                 count++;
    //             }
    //         }
    //         if (count < max) {
    //             max = count;
    //             leastTeam = t;
    //         }
    //     }
    //     // Debug.Log("asd " + leastTeam.transform.position);
    //     GameObject player = (GameObject)Instantiate(playerPrefab, leastTeam.transform.position, leastTeam.transform.rotation);
    //     player.GetComponent<VRPlayerController>().teamID = int.Parse(leastTeam.name.Substring(4, 1));
    //     Debug.Log(player.GetComponent<VRPlayerController>().teamID);
    //     // if (leastTeam.numRobots == 0) {
    //     //     leastTeam.robotsToSpawn = 4;
    //     // }
    //     leastTeam.OnPlayerAdded(player);
    //     NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    // }
}