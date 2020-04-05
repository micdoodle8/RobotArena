using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum PlayerMode {
    NONE,
    PLACE_ROBOT,
    OBSERVING,
    CHOOSING_MOVE,
    CHOOSING_ACT,
    MOVING,
    ACTING
}

public class ConnectedPlayerManager : NetworkBehaviour
{
    private GameObject scene;
    public PlayerMode currentMode = PlayerMode.PLACE_ROBOT;
    private GameObject camera;

    public GameObject teamPrefab;

    public GameObject theTeam;
    public GameObject hookedPlayerPrefab;
    private GameObject hookedPlayer;
    public GameObject bombPrefab;
    public GameObject robotPrefab;
    public GameObject hookPrefab;
    private GameObject[] hooks = new GameObject[2];
    private GameObject[] closestPlayers = new GameObject[2];
    private List<GameObject> robots = new List<GameObject>();
    [SyncVar]
    private int numRobots = 0;
    private GameObject palmGui;

    // Start is called before the first frame update
    void Start()
    {
        camera = GameObject.Find("Leap Rig");
        scene = GameObject.Find("Scene");
        palmGui = GameObject.Find("PalmGui");
        if (hasAuthority) {
            CmdSpawnTeam();
            if (isServer) {
                BeginPlaceMode();
            }
        }
    }

    private void BeginPlaceMode() {
        float scale = 2.0F;
        camera.transform.position = theTeam.transform.position + theTeam.transform.rotation * new Vector3(0.0F, 0.0F, -3.0F) * scale + new Vector3(0.0F, 6.0F, 0.0F) * scale;
        camera.transform.rotation = theTeam.transform.rotation;
        GameObject.Find("FadeScreen").GetComponent<Fader>().FadeBackIn(1.0F, FadeCallback, 0);
    }

    void Awake() {
    }

    // Update is called once per frame
    void Update()
    {
        if (hasAuthority) {
            if (Input.GetKeyDown(KeyCode.K)) {
                CmdSpawnRobot();
            }
            if (Input.GetKeyDown(KeyCode.B)) {
                if (isClient) {
                    CmdSpawnBomb();
                }
            }
            if (currentMode == PlayerMode.CHOOSING_MOVE) {
                GameObject[] closestPlrs = getClosestPlayers();
                float offsetY = 5.0F;
                for (int i = 0; i < 2; ++i) {
                    if (closestPlrs[i] != null) {
                        if (hooks[i] == null) {
                            hooks[i] = Instantiate(hookPrefab, closestPlrs[i].transform.position + new Vector3(0.0F, offsetY, 0.0F), Quaternion.identity);
                        } else {
                            hooks[i].transform.position = closestPlrs[i].transform.position + new Vector3(0.0F, offsetY, 0.0F);
                        }
                    } else {
                        if (hooks[i] != null) {
                            Destroy(hooks[i]);
                            hooks[i] = null;
                        }
                    }
                }
            }
        }
    }

    private void FadeCallback(int param) {
        switch (param) {
            case 0:
                break;
            case 1:
                GameObject.FindObjectOfType<HandSwitcher>().SwitchHands();
                BeginPlaceMode();
                if (isServer) {
                    // theTeam.GetComponent<Team>().SpawnRobots(connectionToClient);
                }
                break;
            case 2:
                GameObject.FindObjectOfType<HandSwitcher>().SwitchHands();
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject player in players) {
                    if (player.GetComponent<VRPlayerController>().teamContainer == theTeam) {
                        player.GetComponent<VRPlayerController>().SetPlayerControlled();
                        break;
                    }
                }
                GameObject.Find("FadeScreen").GetComponent<Fader>().FadeBackIn(1.0F, FadeCallback, 0);
                break;
        }
    }

    public void SpawnRobot() {
        if (hasAuthority) {
            CmdSpawnRobot();
        }
    }

    public void ChangeMode(PlayerMode mode) {
        if (currentMode != mode) {
            // currentMode is the 'old' mode we're switching from in this case
            // mode is the new mode
            if (currentMode == PlayerMode.OBSERVING) {
                GameObject.Find("PalmPivot").GetComponent<FacingCamera>().Disable(true);
            } else if (currentMode == PlayerMode.MOVING) {
                for (int i = 0; i < 2; ++i) {
                    if (hooks[i] != null) {
                        Destroy(hooks[i]);
                    }
                }
            }

            if (mode == PlayerMode.PLACE_ROBOT) {
                GameObject.Find("FadeScreen").GetComponent<Fader>().FadeToBlack(1.0F, FadeCallback, 1);
            } else if (mode == PlayerMode.OBSERVING) {
                GameObject.Find("PalmPivot").GetComponent<FacingCamera>().Disable(false);
                // GameObject.Find("FadeScreen").GetComponent<Fader>().FadeToBlack(1.0F, FadeCallback, 2);
            }
            currentMode = mode;
        }
    }

    [Command]
    void CmdSpawnRobot() {
        Debug.Log("ConnectedPlayerManager::SpawnRobot -- Spawning robot ");
        if (hookedPlayer != null) {
            GameObject robot = Instantiate(robotPrefab, hookedPlayer.transform.position - new Vector3(0.0F, 4.83F, 0.0F) * scene.transform.localScale.y, theTeam.transform.rotation, scene.transform);
            robots.Add(robot);
            numRobots++;
            robot.GetComponent<VRPlayerController>().teamContainer = theTeam;
            NetworkServer.SpawnWithClientAuthority(robot, connectionToClient);
            RpcSetControlState(robot, false);
            Destroy(hookedPlayer);
            if (numRobots < 3) {
                CmdSpawnHook();
            } else {
                ChangeMode(PlayerMode.OBSERVING);
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
        hookedPlayer = Instantiate(hookedPlayerPrefab, theTeam.transform.position + new Vector3(0.0F, 5.3F, 0.0F) * scene.transform.localScale.y, theTeam.transform.rotation, scene.transform);
        NetworkServer.SpawnWithClientAuthority(hookedPlayer, connectionToClient);
    }

    [Command]
    void CmdSpawnBomb() {
        GameObject bomb = Instantiate(bombPrefab, new Vector3(0.0F, 10.0F, 0.0F) * scene.transform.localScale.y, Quaternion.identity, scene.transform);
        NetworkServer.Spawn(bomb);
    }

    [Command]
    void CmdSpawnTeam() {
        Debug.Log("ConnectedPlayerManager::CmdSpawnTeam -- Received spawn team cmd");
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Spawn");
        if (spawnPoints.Length > 0) {
            Transform spawnPoint = spawnPoints[0].transform;
            theTeam = Instantiate(teamPrefab, spawnPoint.position, spawnPoint.rotation, scene.transform);
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
            // ChangeMode(PlayerMode.PLACE_ROBOT);
            theTeam = createdTeam;
            BeginPlaceMode();
            CmdSpawnHook();
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

    public void StartAct() {
        if (hasAuthority) {
            ChangeMode(PlayerMode.CHOOSING_ACT);
        }
    }

    public void StartMove() {
        if (hasAuthority) {
            ChangeMode(PlayerMode.CHOOSING_MOVE);
        }
    }

    public void StartPass() {
    }

    public void StartMoveRobot(int hand) {
        Debug.Log("Start " + hand);
        if (currentMode != PlayerMode.MOVING) {
            ChangeMode(PlayerMode.MOVING);
        }
        if (this.closestPlayers[0] == null) {
            this.closestPlayers[0] = getClosestPlayers()[0];
        }
        if (this.closestPlayers[1] == null) {
            this.closestPlayers[1] = getClosestPlayers()[1];
        }
        for (int i = 0; i < 2; ++i) {
            if (hooks[i] != null && this.closestPlayers[i] != null) {
                
            }
        }
        // this.closestPlayer.transform.position = hook.transform.position - new Vector3(0.0F, 4.83F, 0.0F);
        // hook.transform.position = new Vector3(this.closestPlayer.transform.position.x, hook.transform.position.y, this.closestPlayer.transform.position.z);
    }

    public void FinishMoveRobot(int hand) {
        Debug.Log("Finish " + hand);
        // this.closestPlayer = null;
    }

    private GameObject[] getClosestPlayers() {
        GameObject closestPlayerLeft = null;
        GameObject closestPlayerRight = null;
        float distLeft = 100.0F;
        float distRight = 100.0F;
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");
        GameObject palmObjLeft = GameObject.Find("ForwardTransformPalmLeft");
        GameObject palmObjRight = GameObject.Find("ForwardTransformPalmLeft");
        foreach (GameObject obj in objects) {
            VRPlayerController controller = obj.GetComponent<VRPlayerController>();
            if (controller != null) {
                if (controller.teamContainer == theTeam) { // On same team
                    Vector3 diff = palmObjLeft.transform.position - obj.transform.position;
                    if (diff.magnitude < distLeft) {
                        closestPlayerLeft = obj;
                        distLeft = diff.magnitude;
                    }
                    diff = palmObjRight.transform.position - obj.transform.position;
                    if (diff.magnitude < distRight) {
                        closestPlayerRight = obj;
                        distRight = diff.magnitude;
                    }
                }
            }
        }
        return new[] { closestPlayerLeft, closestPlayerRight };
    }
}
