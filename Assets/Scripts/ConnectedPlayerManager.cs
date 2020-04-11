#pragma warning disable 0618
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
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
    [Header("Prefabs")]

    public GameObject teamPrefab;
    public GameObject hookedPlayerPrefab;
    public GameObject bombPrefab;
    public GameObject robotPrefab;
    public GameObject hookPrefab;
    public GameObject rocketPrefab;
    public GameObject moveCylinderPrefab;
    public GameObject laserPrefab;

    private PlayerMode currentMode = PlayerMode.PLACE_ROBOT;
    private GameObject theTeam;
    private GameObject scene;
    private GameObject leapRig;
    private GameObject hookedPlayer;
    private GameObject spawnedHook;
    private GameObject grabbedPlayer = null;
    private GameObject spawnedCylinder;
    private GameObject spawnedRocket;
    private GameObject spawnedLaser;
    private bool lastSpawnedRocketActive = false;
    private List<GameObject> robots = new List<GameObject>();
    [SyncVar]
    private int numRobots = 0;
    private GameObject palmGui;
    private GameObject actingPlayer = null;
    private bool indexFingerExtended = false;
    private bool handsOpened = false;
    private bool firingLaser = false;
    private bool gameOver = false;
    private bool isMyTurn = false;
    private bool resultWin = false;

    // Start is called before the first frame update
    void Start()
    {
        leapRig = GameObject.Find("Leap Rig");
        scene = GameObject.Find("Scene");
        palmGui = GameObject.Find("PalmGui");
        if (hasAuthority) {
            CmdSpawnTeam();
            if (isServer) {
                MoveToObserverPosition();
            }
        }
    }

    private void MoveToObserverPosition() {
        float scale = 2.0F;
        leapRig.transform.position = theTeam.transform.position + theTeam.transform.rotation * new Vector3(0.0F, 0.0F, -3.0F) * scale + new Vector3(0.0F, 6.0F, 0.0F) * scale;
        leapRig.transform.rotation = theTeam.transform.rotation;
        GameObject.Find("FadeScreen").GetComponent<VRFader>().FadeBackIn(1.0F, FadeCallback, 0);
    }

    void Update() {
        if (hasAuthority) {
            switch (currentMode) {
                case PlayerMode.CHOOSING_MOVE:
                    if (spawnedHook == null) {
                        GameObject closestPlayer = getClosestPlayer();
                        spawnedHook = Instantiate(hookPrefab, closestPlayer.transform.position + new Vector3(0.0F, 5.0F, 0.0F), Quaternion.identity);
                    }
                    break;
                case PlayerMode.MOVING:
                    if (grabbedPlayer != null) {
                        // Debug.Log("3");
                        // GameObject hook = hooks[0] != null ? hooks[0] : hooks[1];
                        Vector3 diff = spawnedHook.transform.position - spawnedCylinder.transform.position;
                        diff.y = 0.0F;
                        Vector3 target;
                        if (diff.magnitude >= 3.5F) {
                            target = spawnedCylinder.transform.position + diff.normalized * 3.5F;
                        } else {
                            target = spawnedHook.transform.position;
                        }
                        target.y = grabbedPlayer.transform.position.y;
                        grabbedPlayer.transform.position = target;
                    }
                    break;
                case PlayerMode.ACTING:
                    if (spawnedRocket != null) {
                        GameObject handModels = GameObject.Find("Hand Models");
                        for (int i = 0; i < handModels.transform.childCount; ++i) {
                            GameObject hand = handModels.transform.GetChild(i).gameObject;
                            if (hand.active) {
                                RiggedHand rh = hand.GetComponent<RiggedHand>();
                                if (rh != null && rh.Handedness == Chirality.Right) {
                                    Vector3 fingerDirection = rh.GetLeapHand().Fingers[1].Bone(Bone.BoneType.TYPE_DISTAL).Direction.ToVector3();
                                    spawnedRocket.GetComponent<ItemRocket>().Steer(fingerDirection, indexFingerExtended);
                                }
                            }
                        }
                    } else if (spawnedRocket == null && lastSpawnedRocketActive) {
                        GameObject.Find("FadeScreen").GetComponent<VRFader>().FadeToBlack(1.0F, FadeCallback, 5);
                    }
                    lastSpawnedRocketActive = (this.spawnedRocket != null);
                    break;
            }
        }
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
                actingPlayer.GetComponent<ControllableRobot>().SetPlayerControlled();
                GameObject.Find("FadeScreen").GetComponent<VRFader>().FadeBackIn(1.0F, FadeCallback, 0);

                EnablePalmGui(1);
                break;
            case 3:
                actingPlayer.GetComponent<ControllableRobot>().SetPlayerNotControlled();
                GameObject.Find("FadeScreen").GetComponent<VRFader>().FadeBackIn(1.0F, FadeCallback, 0);
                GameObject.Find("PalmPivot").GetComponent<FacingCamera>().Disable(true);
                CmdSpawnRocket();
                GameObject.FindObjectOfType<HandSwitcher>().SwitchHands();
                leapRig.transform.position = actingPlayer.transform.position + new Vector3(0.0F, 16.0F, 0.0F);
                leapRig.transform.rotation = theTeam.transform.rotation;
                break;
            case 4:
                GameObject.FindObjectOfType<HandSwitcher>().SwitchHands();
                goto case 5; // Dreaded goto, only used to prevent code duplication
            case 5:
                MoveToObserverPosition();
                ChangeMode(PlayerMode.OBSERVING);
                if (actingPlayer != null) {
                    actingPlayer.GetComponent<ControllableRobot>().SetPlayerNotControlled();
                }
                actingPlayer = null;
                break;
        }
    }

    public void OnRobotDeath(GameObject robot) {
        if (hasAuthority) {
            if (robot == actingPlayer) {
                EndTurn(true, true);
            }
            CmdCheckWinLoss();
        }
    }

    [Command]
    public void CmdCheckWinLoss() {
        int robotCount = 0;
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject obj in objects) {
            ControllableRobot controller = obj.GetComponent<ControllableRobot>();
            if (controller != null) {
                if (obj.GetComponent<Health>().GetHealth() > 0) {
                    if (controller.teamContainer == theTeam) { // On this team
                        robotCount++;
                    }
                } else {
                    Destroy(obj);
                }
            }
        }
        if (robotCount == 0) {
            TurnManager turnManager = NetworkManager.singleton.gameObject.GetComponent<TurnManager>();
            turnManager.PlayerLost(this);
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
                // for (int i = 0; i < 2; ++i) {
                //     if (hooks[i] != null) {
                //         Debug.Log("3Destroying hook " + i);
                //         Destroy(hooks[i]);
                //     }
                // }
            } else if (currentMode == PlayerMode.CHOOSING_ACT) {
                GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject obj in objects) {
                    ControllableRobot controller = obj.GetComponent<ControllableRobot>();
                    if (controller != null) {
                        if (controller.teamContainer == theTeam) { // On same team
                            controller.transform.Find("PlayerButton").gameObject.SetActive(false);
                        }
                    }
                }
            } else if (currentMode == PlayerMode.ACTING) {
                firingLaser = false;
            }

            if (mode == PlayerMode.PLACE_ROBOT) {
                GameObject.Find("FadeScreen").GetComponent<VRFader>().FadeToBlack(1.0F, FadeCallback, 1);
            } else if (mode == PlayerMode.OBSERVING) {
                leapRig.transform.localScale = new Vector3(25.0F, 25.0F, 25.0F);
                if (isMyTurn && !gameOver) {
                    EnablePalmGui(0);
                    // GameObject.Find("HintText").GetComponent<Text>().text = "Your Turn!";
                    ShowText(0);
                } else if (gameOver) {
                    ShowText(resultWin ? 2 : 1);
                }
            } else if (mode == PlayerMode.ACTING) {
                GameObject.Find("FadeScreen").GetComponent<VRFader>().FadeToBlack(1.0F, FadeCallback, 2);
            } else if (mode == PlayerMode.CHOOSING_ACT) {
                GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject obj in objects) {
                    ControllableRobot controller = obj.GetComponent<ControllableRobot>();
                    if (controller != null) {
                        if (controller.teamContainer == theTeam) { // On same team
                            controller.transform.Find("PlayerButton").gameObject.SetActive(true);
                        }
                    }
                }
            }
            currentMode = mode;
        }
    }

    [Command]
    void CmdSpawnRobot() {
        if (hookedPlayer != null) {
            GameObject robot = Instantiate(robotPrefab, hookedPlayer.transform.position - new Vector3(0.0F, 4.83F, 0.0F) * scene.transform.localScale.y, theTeam.transform.rotation, scene.transform);
            robots.Add(robot);
            numRobots++;
            robot.GetComponent<ControllableRobot>().teamContainer = theTeam;
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
                robot.GetComponent<ControllableRobot>().SetPlayerControlled();
            } else {
                robot.GetComponent<ControllableRobot>().SetPlayerNotControlled();
            }
        }
    }

    [Command]
    void CmdSpawnHook() {
        hookedPlayer = Instantiate(hookedPlayerPrefab, theTeam.transform.position + new Vector3(0.0F, 5.3F, 0.0F) * scene.transform.localScale.y, theTeam.transform.rotation, scene.transform);
        NetworkServer.SpawnWithClientAuthority(hookedPlayer, connectionToClient);
    }

    [Command]
    void CmdSpawnBomb(Vector3 pos) {
        if (GameObject.FindObjectsOfType<ItemBomb>().Length == 0) {
            GameObject bomb = Instantiate(bombPrefab, pos, Quaternion.identity, scene.transform);
            // bomb.GetComponent<Rigidbody>().velocity = (actingPlayer.transform.forward + Vector3.up) * 25.0F;
            NetworkServer.SpawnWithClientAuthority(bomb, connectionToClient);
        }
    }

    [Command]
    void CmdSpawnRocket() {
        GameObject rocket = Instantiate(rocketPrefab, actingPlayer.transform.position + new Vector3(0.0F, 3.0F, 0.0F) + actingPlayer.transform.forward, theTeam.transform.rotation);
        rocket.transform.Rotate(new Vector3(-45.0F, 0.0F, 0.0F), Space.Self);
        // rocket.GetComponent<Rigidbody>().isKinematic = true;
        // rocket.GetComponent<Rigidbody>().useGravity = false;
        NetworkServer.SpawnWithClientAuthority(rocket, connectionToClient);
        spawnedRocket = rocket;
        RpcOnRocketSpawn(spawnedRocket);
    }

    [Command]
    void CmdSpawnLaser() {
        GameObject laser = Instantiate(laserPrefab, actingPlayer.transform.position + new Vector3(0.0F, 2.0F, 0.0F), theTeam.transform.rotation);
        NetworkServer.SpawnWithClientAuthority(laser, connectionToClient);
        spawnedLaser = laser;
        laser.GetComponent<ItemLaser>().firingPlayer = actingPlayer;
        RpcOnLaserSpawn(spawnedLaser, actingPlayer);
    }

    [Command]
    void CmdDestroyLaser() {
        if (spawnedLaser != null) {
            Destroy(spawnedLaser);
        }
    }

    [ClientRpc]
    private void RpcOnRocketSpawn(GameObject rocket) {
        if (hasAuthority) {
            spawnedRocket = rocket;
        }
    }

    [ClientRpc]
    private void RpcOnLaserSpawn(GameObject laser, GameObject player) {
        if (hasAuthority) {
            laser.GetComponent<ItemLaser>().firingPlayer = player;
            spawnedLaser = laser;
        }
    }

    [Command]
    void CmdSpawnTeam() {
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

    private Dictionary<int, float> buttonPressedTimes = new Dictionary<int, float>();
    private Dictionary<int, float> lastPressedTimes = new Dictionary<int, float>();

    public void TurnButtonPressed(int index) {
        if (hasAuthority) {
            if (!buttonPressedTimes.ContainsKey(index)) {
                buttonPressedTimes.Add(index, 0.0F);
            } else {
                buttonPressedTimes[index] = 0.0F;
            }
            if (!lastPressedTimes.ContainsKey(index)) {
                lastPressedTimes.Add(index, Time.time);
            } else {
                lastPressedTimes[index] = Time.time;
            }
        }
    }

    public void TurnButtonHeld(int index) {
        if (hasAuthority && buttonPressedTimes.ContainsKey(index)) {
            float diff = Time.time - lastPressedTimes[index];
            if (diff > 2.0F) { // This should never be true, but for safety
                diff = 0.0F;
                lastPressedTimes[index] = 0.0F;
                buttonPressedTimes[index] = 0.0F;
            } else {
                buttonPressedTimes[index] += diff;
                if (buttonPressedTimes[index] >= 1.0F) {
                    switch (index) {
                        case 0:
                            ChangeMode(PlayerMode.CHOOSING_MOVE);
                            break;
                        case 1:
                            ChangeMode(PlayerMode.CHOOSING_ACT);
                            break;
                        case 2:
                            EndTurn(false, false);
                            break;
                        case 3:
                            if (isMyTurn) {
                                Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward.normalized / 1.25F;
                                CmdSpawnBomb(pos);
                                GameObject.Find("PalmPivot").GetComponent<FacingCamera>().Disable(true);
                            }
                            break;
                        case 4:
                            if (isMyTurn) {
                                GameObject.Find("FadeScreen").GetComponent<VRFader>().FadeToBlack(1.0F, FadeCallback, 3);
                                GameObject.Find("PalmPivot").GetComponent<FacingCamera>().Disable(true);
                            }
                            break;
                        case 5:
                            if (isMyTurn) {
                                firingLaser = true;
                                GameObject.Find("PalmPivot").GetComponent<FacingCamera>().Disable(true);
                            }
                            break;
                    }
                    buttonPressedTimes.Clear(); // Reset this button and all others when pressed
                }
                lastPressedTimes[index] = Time.time;
            }
        }
    }

    public void TurnButtonReleased(int index) {
        if (hasAuthority) {
            buttonPressedTimes[index] = 0.0F;
        }
    }

    public float ButtonPressed(int index) {
        return buttonPressedTimes.ContainsKey(index) ? buttonPressedTimes[index] : 0.0F;
    }

    public void OnPlayerActChose(GameObject robot) {
        if (hasAuthority) {
            ChangeMode(PlayerMode.ACTING);
            actingPlayer = robot;
            if (!isServer) {
                CmdSetActingPlayer(actingPlayer);
            }
        }
    }

    public void StartMoveRobot(GameObject theHook) {
        if (hasAuthority) {
            ChangeMode(PlayerMode.MOVING);
            grabbedPlayer = getClosestPlayer();
            spawnedCylinder = Instantiate(moveCylinderPrefab, grabbedPlayer.transform.position, Quaternion.identity);
        }
    }

    public void FinishMoveRobot(GameObject theHook) {
        if (hasAuthority) {
            grabbedPlayer = null;
            Destroy(spawnedHook);
            spawnedHook = null;
            Destroy(spawnedCylinder);
            EndTurn(false, false);
            ChangeMode(PlayerMode.OBSERVING);
        }
    }

    public void DoAction(int actionNum) {
        // if (hasAuthority && isMyTurn) {
        //     switch (actionNum) {
        //     }
        // }
    }

    public void FingerExtended(bool state) {
        indexFingerExtended = state;
    }

    public void HandsOpened(bool state) {
        handsOpened = state;
        if (firingLaser) {
            if (state) {
                CmdSpawnLaser();
            } else {
                CmdDestroyLaser();
            }
        }
    }

    private GameObject getClosestPlayer() {
        // GameObject closestPlayerLeft = null;
        // GameObject closestPlayerRight = null;
        // float distLeft = 100.0F;
        // float distRight = 100.0F;
        // GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");
        // GameObject palmObjLeft = GameObject.Find("ForwardTransformPalmLeft");
        // GameObject palmObjRight = GameObject.Find("ForwardTransformPalmRight");
        // foreach (GameObject obj in objects) {
        //     ControllableRobot controller = obj.GetComponent<ControllableRobot>();
        //     if (controller != null) {
        //         if (controller.teamContainer == theTeam) { // On same team
        //             Vector3 diff = palmObjLeft.transform.position - obj.transform.position;
        //             if (diff.magnitude < distLeft) {
        //                 closestPlayerLeft = obj;
        //                 distLeft = diff.magnitude;
        //             }
        //             diff = palmObjRight.transform.position - obj.transform.position;
        //             if (diff.magnitude < distRight) {
        //                 closestPlayerRight = obj;
        //                 distRight = diff.magnitude;
        //             }
        //         }
        //     }
        // }
        // return new[] { closestPlayerLeft, closestPlayerRight };
        GameObject closestPlayerRight = null;
        float distRight = 100.0F;
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");
        GameObject palmObjRight = GameObject.Find("ForwardTransformPalmRight");
        foreach (GameObject obj in objects) {
            ControllableRobot controller = obj.GetComponent<ControllableRobot>();
            if (controller != null) {
                if (controller.teamContainer == theTeam) { // On same team
                    Vector3 diff = palmObjRight.transform.position - obj.transform.position;
                    if (diff.magnitude < distRight) {
                        closestPlayerRight = obj;
                        distRight = diff.magnitude;
                    }
                }
            }
        }
        return closestPlayerRight;
    }

    public void EndTurn(bool fadeAndMove, bool switchHands) {
        if (hasAuthority && isMyTurn) {
            if (fadeAndMove) {
                GameObject.Find("FadeScreen").GetComponent<VRFader>().FadeToBlack(1.0F, FadeCallback, switchHands ? 4 : 5);
            } else {
                actingPlayer = null;
            }
            // GameObject.Find("HintText").GetComponent<Text>().enabled = false;
            GameObject.Find("PalmPivot").GetComponent<FacingCamera>().Disable(true);
            isMyTurn = false;
            firingLaser = false;
            CmdFinishTurn();
        }
    }

    [Command]
    private void CmdFinishTurn() {
        TurnManager turnManager = NetworkManager.singleton.gameObject.GetComponent<TurnManager>();
        turnManager.NextTurn();
    }

    public void StartTurn() {
        if (isServer) {
            RpcTurnStarting();
        }
    }

    [ClientRpc]
    private void RpcTurnStarting() {
        if (hasAuthority) {
            isMyTurn = true;
            if (currentMode == PlayerMode.OBSERVING && !gameOver) {
                EnablePalmGui(0);
                // GameObject.Find("HintText").GetComponent<Text>().text = "Your Turn!";
                ShowText(0);
            }
        }
    }

    private void EnablePalmGui(int type) {
        FacingCamera[] facingCameras = Resources.FindObjectsOfTypeAll<FacingCamera>();
        facingCameras[0].Disable(false);
        PalmGuiEnableDisable[] palmGuis = Resources.FindObjectsOfTypeAll<PalmGuiEnableDisable>();
        switch (type) {
            case 0:
                foreach (PalmGuiEnableDisable gui in palmGuis) {
                    gui.gameObject.SetActive(gui.gameObject.name.Equals("PalmGui"));
                }
                break;
            case 1:
                foreach (PalmGuiEnableDisable gui in palmGuis) {
                    gui.gameObject.SetActive(gui.gameObject.name.Equals("PalmGuiAct"));
                }
                break;
        }
    }

    private void ShowText(int index) {
        switch (index) {
            case 0:
                GameObject.Find("YourTurnText").GetComponent<ObjectFader>().StartFade(3.0F);
                GameObject.Find("YouLostText").GetComponent<ObjectFader>().ForceDisable();
                GameObject.Find("YouWonText").GetComponent<ObjectFader>().ForceDisable();
                break;
            case 1:
                GameObject.Find("YouLostText").GetComponent<ObjectFader>().StartFade(1.5F);
                GameObject.Find("YourTurnText").GetComponent<ObjectFader>().ForceDisable();
                GameObject.Find("YouWonText").GetComponent<ObjectFader>().ForceDisable();
                break;
            case 2:
                GameObject.Find("YouWonText").GetComponent<ObjectFader>().StartFade(1.5F);
                GameObject.Find("YourTurnText").GetComponent<ObjectFader>().ForceDisable();
                GameObject.Find("YouLostText").GetComponent<ObjectFader>().ForceDisable();
                break;
        }
    }

    public void OnGameResolved(bool won) {
        RpcOnGameResolved(won);
        gameOver = true;
    }

    [ClientRpc]
    private void RpcOnGameResolved(bool won) {
        if (hasAuthority) {
            resultWin = won;
            if (currentMode == PlayerMode.OBSERVING) {
                ShowText(won ? 2 : 1);
            }
        }
        gameOver = true;
    }
}
