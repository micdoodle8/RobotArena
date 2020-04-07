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

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader) {
        base.OnServerAddPlayer(conn, playerControllerId, extraMessageReader);
        Debug.Log("added1?");
        GetComponent<TurnManager>().UpdatePlayers();
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        base.OnServerAddPlayer(conn, playerControllerId);
        Debug.Log("added2?");
        GetComponent<TurnManager>().UpdatePlayers();
    }
}