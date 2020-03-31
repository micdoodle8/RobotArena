using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum PlayerMode {
    NONE,
    PLACE_ROBOT,
    OBSERVING,
    TAKING_TURN
}

public class ConnectedPlayerManager : NetworkBehaviour
{
    public PlayerMode currentMode = PlayerMode.NONE;
    private GameObject camera;

    public GameObject teamPrefab;

    public GameObject theTeam;
    public GameObject hookedPlayerPrefab;
    private GameObject hookedPlayer;
    public GameObject bombPrefab;
    public GameObject robotPrefab;
    private List<GameObject> robots = new List<GameObject>();
    [SyncVar]
    private int numRobots = 0;

    // Start is called before the first frame update
    void Start()
    {
        camera = GameObject.Find("OVRCameraRig");
    }

    // Update is called once per frame
    void Update()
    {
        if (hasAuthority) {
            if (Input.GetKeyDown(KeyCode.J)) {
                ChangeMode(PlayerMode.TAKING_TURN);
            }
            if (Input.GetKeyDown(KeyCode.K)) {
                CmdSpawnRobot();
            }
            if (Input.GetKeyDown(KeyCode.B)) {
                if (isClient) {
                    CmdSpawnBomb();
                }
            }
        }
    }

    private void FadeCallback(int param) {
        switch (param) {
            case 0:
                break;
            case 1:
                camera.transform.position = theTeam.transform.position + theTeam.transform.rotation * new Vector3(0.0F, 0.0F, -3.0F) + new Vector3(0.0F, 6.0F, 0.0F);
                camera.transform.rotation = theTeam.transform.rotation;
                GameObject.Find("FadeScreen").GetComponent<Fader>().FadeBackIn(2.0F, FadeCallback, 0);
                if (isServer) {
                    // theTeam.GetComponent<Team>().SpawnRobots(connectionToClient);
                }
                break;
            case 2:
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject player in players) {
                    if (player.GetComponent<VRPlayerController>().teamContainer == theTeam) {
                        player.GetComponent<VRPlayerController>().SetPlayerControlled();
                        break;
                    }
                }
                GameObject.Find("FadeScreen").GetComponent<Fader>().FadeBackIn(2.0F, FadeCallback, 0);
                break;
        }
    }

    public void ChangeMode(PlayerMode mode) {
        if (currentMode != mode) {
            if (mode == PlayerMode.PLACE_ROBOT) {
                GameObject.Find("FadeScreen").GetComponent<Fader>().FadeToBlack(2.0F, FadeCallback, 1);
            } else if (mode == PlayerMode.TAKING_TURN) {
                GameObject.Find("FadeScreen").GetComponent<Fader>().FadeToBlack(2.0F, FadeCallback, 2);
            }
        }
    }

    [Command]
    void CmdSpawnRobot() {
        Debug.Log("ConnectedPlayerManager::SpawnRobot -- Spawning robot ");
        if (hookedPlayer != null) {
            GameObject robot = Instantiate(robotPrefab, hookedPlayer.transform.position - new Vector3(0.0F, 4.83F, 0.0F), theTeam.transform.rotation);
            robots.Add(robot);
            numRobots++;
            robot.GetComponent<VRPlayerController>().teamContainer = theTeam;
            NetworkServer.SpawnWithClientAuthority(robot, connectionToClient);
            RpcSetControlState(robot, false);
            Destroy(hookedPlayer);
            if (numRobots < 3) {
                CmdSpawnHook();
            }
        }
    }

    [ClientRpc]
    void RpcSetControlState(GameObject robot, bool controlled) {
        if (hasAuthority) {
            if (controlled) {
                robot.GetComponent<VRPlayerController>().SetPlayerControlled();
            } else {
                robot.GetComponent<VRPlayerController>().SetPlayerNotControlled();
            }
        }
    }

    [Command]
    void CmdSpawnHook() {
        hookedPlayer = Instantiate(hookedPlayerPrefab, theTeam.transform.position + new Vector3(0.0F, 5.3F, 0.0F), theTeam.transform.rotation);
        NetworkServer.SpawnWithClientAuthority(hookedPlayer, connectionToClient);
    }

    [Command]
    void CmdSpawnBomb() {
        GameObject bomb = Instantiate(bombPrefab, new Vector3(0.0F, 10.0F, 0.0F), Quaternion.identity);
        NetworkServer.Spawn(bomb);
    }

    [Command]
    void CmdSpawnTeam() {
        Debug.Log("ConnectedPlayerManager::CmdSpawnTeam -- Received spawn team cmd");
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Spawn");
        if (spawnPoints.Length > 0) {
            Transform spawnPoint = spawnPoints[0].transform;
            theTeam = Instantiate(teamPrefab, spawnPoint.position, spawnPoint.rotation);
            NetworkServer.SpawnWithClientAuthority(theTeam, connectionToClient);
            // ChangeMode(PlayerMode.PLACE_ROBOT);
            if (isServer) {
                RpcOnTeamCreate(theTeam);
            }
            Destroy(spawnPoints[0]);
        }
    }

    [ClientRpc]
    private void RpcOnTeamCreate(GameObject createdTeam) {
        if (hasAuthority) {
            ChangeMode(PlayerMode.PLACE_ROBOT);
            CmdSpawnHook();
            theTeam = createdTeam;
        }
    }

    void OnGUI() {
        if (hasAuthority && theTeam == null) {
            int xPos = 10;
            int yPos = 150;
            int space = 24;
            if (GUI.Button(new Rect(xPos, yPos, 200, 20), "Create Team")) {
                Debug.Log("ConnectedPlayerManager::OnGUI -- Creating team " + hasAuthority);
                CmdSpawnTeam();
            }
        }
    }
}
