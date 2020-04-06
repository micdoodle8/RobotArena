using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Leap.Unity;
using Leap;

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
    public GameObject rocketPrefab;
    private GameObject[] hooks = new GameObject[2];
    private GameObject[] closestPlayers = new GameObject[2];
    private GameObject grabbedPlayer = null;
    public GameObject moveCylinderPrefab;
    private GameObject spawnedCylinder;
    private GameObject spawnedRocket;
    private bool lastSpawnedRocketActive = false;
    private List<GameObject> robots = new List<GameObject>();
    [SyncVar]
    private int numRobots = 0;
    private GameObject palmGui;
    private GameObject actingPlayer = null;
    private ActingHandler actingHandler;
    private bool indexFingerExtended = false;

    // Start is called before the first frame update
    void Start()
    {
        camera = GameObject.Find("Leap Rig");
        scene = GameObject.Find("Scene");
        palmGui = GameObject.Find("PalmGui");
        actingHandler = GetComponent<ActingHandler>();
        if (hasAuthority) {
            CmdSpawnTeam();
            if (isServer) {
                MoveToObserverPosition();
            }
        }
    }

    private void MoveToObserverPosition() {
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
                    CmdSpawnBomb(Camera.main.transform.forward);
                }
            }
            if (currentMode == PlayerMode.CHOOSING_MOVE) {
                closestPlayers = getClosestPlayers();
                float offsetY = 5.0F;
                int numToUpdate = 2;
                if (closestPlayers[0] == closestPlayers[1] && closestPlayers[0] != null) {
                    numToUpdate = 1;
                    if (hooks[1] != null) {
                        Destroy(hooks[1]);
                        hooks[1] = null;
                    }
                }
                for (int i = 0; i < numToUpdate; ++i) {
                    if (closestPlayers[i] != null) {
                        if (hooks[i] == null) {
                            hooks[i] = Instantiate(hookPrefab, closestPlayers[i].transform.position + new Vector3(0.0F, offsetY, 0.0F), Quaternion.identity);
                        } else {
                            hooks[i].transform.position = closestPlayers[i].transform.position + new Vector3(0.0F, offsetY, 0.0F);
                        }
                    } else {
                        if (hooks[i] != null) {
                            Destroy(hooks[i]);
                            hooks[i] = null;
                        }
                    }
                }
            } else if (currentMode == PlayerMode.MOVING) {
                if (grabbedPlayer != null) {
                    GameObject hook = hooks[0] != null ? hooks[0] : hooks[1];
                    Vector3 diff = hook.transform.position - spawnedCylinder.transform.position;
                    diff.y = 0.0F;
                    Vector3 target;
                    if (diff.magnitude >= 3.5F) {
                        target = spawnedCylinder.transform.position + diff.normalized * 3.5F;
                    } else {
                        target = hook.transform.position;
                    }
                    target.y = grabbedPlayer.transform.position.y;
                    grabbedPlayer.transform.position = target;
                }
            } else if (currentMode == PlayerMode.ACTING) {
                if (spawnedRocket != null) {
                    GameObject handModels = GameObject.Find("Hand Models");
                    for (int i = 0; i < handModels.transform.childCount; ++i) {
                        GameObject hand = handModels.transform.GetChild(i).gameObject;
                        if (hand.active) {
                            RiggedHand rh = hand.GetComponent<RiggedHand>();
                            if (rh != null && rh.Handedness == Chirality.Right) {
                                Vector3 fingerDirection = rh.GetLeapHand().Fingers[1].Bone(Bone.BoneType.TYPE_DISTAL).Direction.ToVector3();
                                spawnedRocket.GetComponent<RocketScript>().Steer(fingerDirection, indexFingerExtended);
                                // Debug.Log(Camera.main.transform.InverseTransformDirection(fingerDirection));
                            }
                        }
                    }
                } else if (spawnedRocket == null && lastSpawnedRocketActive) {
                    GameObject.Find("FadeScreen").GetComponent<Fader>().FadeToBlack(1.0F, FadeCallback, 4);
                }
            }
        }
        lastSpawnedRocketActive = this.spawnedRocket != null;
    }

    private void FadeCallback(int param) {
        switch (param) {
            case 0:
                break;
            case 1:
                GameObject.FindObjectOfType<HandSwitcher>().SwitchHands();
                MoveToObserverPosition();
                if (isServer) {
                    // theTeam.GetComponent<Team>().SpawnRobots(connectionToClient);
                }
                break;
            case 2:
                GameObject.FindObjectOfType<HandSwitcher>().SwitchHands();
                actingPlayer.GetComponent<VRPlayerController>().SetPlayerControlled();
                GameObject.Find("FadeScreen").GetComponent<Fader>().FadeBackIn(1.0F, FadeCallback, 0);

                GameObject.Find("PalmPivot").GetComponent<FacingCamera>().Disable(false);
                PalmGuiEnableDisable[] palmGuis = Resources.FindObjectsOfTypeAll<PalmGuiEnableDisable>();
                foreach (PalmGuiEnableDisable gui in palmGuis) {
                    gui.gameObject.SetActive(gui.gameObject.name.Equals("PalmGuiAct"));
                }
                break;
            case 3:
                actingPlayer.GetComponent<VRPlayerController>().SetPlayerNotControlled();
                GameObject.Find("FadeScreen").GetComponent<Fader>().FadeBackIn(1.0F, FadeCallback, 0);
                GameObject.Find("PalmPivot").GetComponent<FacingCamera>().Disable(true);
                CmdSpawnRocket();
                GameObject.FindObjectOfType<HandSwitcher>().SwitchHands();
                camera.transform.position = actingPlayer.transform.position + new Vector3(0.0F, 16.0F, 0.0F);
                camera.transform.rotation = theTeam.transform.rotation;
                break;
            case 4:
                // GameObject.FindObjectOfType<HandSwitcher>().SwitchHands();
                MoveToObserverPosition();
                ChangeMode(PlayerMode.OBSERVING);
                break;
        }
    }

    public void SpawnRobot() {
        if (hasAuthority) {
            CmdSpawnRobot();
        }
    }

    [ClientRpc]
    private void RpcChangedMode(PlayerMode mode) {
        if (!isServer && hasAuthority) {
            ChangeMode(mode);
        }
    } 

    public void ChangeMode(PlayerMode mode) {
        if (isServer) {
            RpcChangedMode(mode);
        }
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
            } else if (currentMode == PlayerMode.CHOOSING_ACT) {
                GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject obj in objects) {
                    VRPlayerController controller = obj.GetComponent<VRPlayerController>();
                    if (controller != null) {
                        if (controller.teamContainer == theTeam) { // On same team
                            controller.transform.Find("PlayerButton").GetChild(0).gameObject.SetActive(false);
                        }
                    }
                }
            }

            if (mode == PlayerMode.PLACE_ROBOT) {
                GameObject.Find("FadeScreen").GetComponent<Fader>().FadeToBlack(1.0F, FadeCallback, 1);
            } else if (mode == PlayerMode.OBSERVING) {
                camera.transform.localScale = new Vector3(25.0F, 25.0F, 25.0F);
                GameObject.Find("PalmPivot").GetComponent<FacingCamera>().Disable(false);
                PalmGuiEnableDisable[] palmGuis = Resources.FindObjectsOfTypeAll<PalmGuiEnableDisable>();
                foreach (PalmGuiEnableDisable gui in palmGuis) {
                    gui.gameObject.SetActive(gui.gameObject.name.Equals("PalmGui"));
                }
            } else if (mode == PlayerMode.ACTING) {
                GameObject.Find("FadeScreen").GetComponent<Fader>().FadeToBlack(1.0F, FadeCallback, 2);
            } else if (mode == PlayerMode.CHOOSING_ACT) {
                GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject obj in objects) {
                    VRPlayerController controller = obj.GetComponent<VRPlayerController>();
                    if (controller != null) {
                        if (controller.teamContainer == theTeam) { // On same team
                            controller.transform.Find("PlayerButton").GetChild(0).gameObject.SetActive(true);
                        }
                    }
                }
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
    void CmdSpawnBomb(Vector3 forward) {
        GameObject bomb = Instantiate(bombPrefab, actingPlayer.transform.position + new Vector3(0.0F, 2.0F, 0.0F) + forward / 2.0F, Quaternion.identity, scene.transform);
        // bomb.GetComponent<Rigidbody>().velocity = (actingPlayer.transform.forward + Vector3.up) * 25.0F;
        NetworkServer.SpawnWithClientAuthority(bomb, connectionToClient);
    }

    [Command]
    void CmdSpawnRocket() {
        GameObject rocket = Instantiate(rocketPrefab, actingPlayer.transform.position + new Vector3(0.0F, 2.0F, 0.0F), theTeam.transform.rotation);
        rocket.transform.Rotate(new Vector3(-45.0F, 0.0F, 0.0F), Space.Self);
        // rocket.GetComponent<Rigidbody>().isKinematic = true;
        // rocket.GetComponent<Rigidbody>().useGravity = false;
        NetworkServer.SpawnWithClientAuthority(rocket, connectionToClient);
        spawnedRocket = rocket;
        RpcOnRocketSpawn(spawnedRocket);
    }

    [ClientRpc]
    private void RpcOnRocketSpawn(GameObject rocket) {
        if (hasAuthority) {
            spawnedRocket = rocket;
        }
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
            MoveToObserverPosition();
            CmdSpawnHook();
        }
    }

    [Command]
    private void CmdSetActingPlayer(GameObject actingPlayer) {
        this.actingPlayer = actingPlayer;
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
        FinishTurn();
    }

    public void OnPlayerActChose(GameObject robot) {
        if (hasAuthority) {
            ChangeMode(PlayerMode.ACTING);
            Debug.Log(robot);
            actingPlayer = robot;
            if (!isServer) {
                CmdSetActingPlayer(actingPlayer);
            }
        }
    }

    public void StartMoveRobot(GameObject theHook) {
        Debug.Log(theHook);
        ChangeMode(PlayerMode.MOVING);
        for (int i = 0; i < 2; ++i) {
            if (hooks[i] != theHook && hooks[i] != null) {
                Debug.Log("Destr " +hooks[i]);
                Destroy(hooks[i]);
            } else if (hooks[i] == theHook) {
                Debug.Log("Keep 2 " +hooks[i] + closestPlayers[i] + " " + closestPlayers[0] + " " + closestPlayers[1]);
                grabbedPlayer = closestPlayers[i];
            }
        }
        spawnedCylinder = Instantiate(moveCylinderPrefab, grabbedPlayer.transform.position, Quaternion.identity);
    }

    public void FinishMoveRobot(GameObject theHook) {
        ChangeMode(PlayerMode.OBSERVING);
        grabbedPlayer = null;
        for (int i = 0; i < 2; ++i) {
            hooks[i] = null;
            closestPlayers[i] = null;
        }
        Destroy(spawnedCylinder);
        FinishTurn();
    }

    public void DoAction(int actionNum) {
        if (hasAuthority) {
            Debug.Log("Doing Action " + actionNum);
            switch (actionNum) {
                case 1:
                    Debug.LogError("sending cmd");
                    CmdSpawnBomb(Camera.main.transform.forward);
                    break;
                case 2:
                    GameObject.Find("FadeScreen").GetComponent<Fader>().FadeToBlack(1.0F, FadeCallback, 3);
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 7:
                    break;
                case 8:
                    break;
                case 9:
                    break;
            }
        }
    }

    public void FingerExtended(bool state) {
        indexFingerExtended = state;
    }

    private GameObject[] getClosestPlayers() {
        GameObject closestPlayerLeft = null;
        GameObject closestPlayerRight = null;
        float distLeft = 100.0F;
        float distRight = 100.0F;
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");
        GameObject palmObjLeft = GameObject.Find("ForwardTransformPalmLeft");
        GameObject palmObjRight = GameObject.Find("ForwardTransformPalmRight");
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

    public void FinishTurn() {

    }
}
