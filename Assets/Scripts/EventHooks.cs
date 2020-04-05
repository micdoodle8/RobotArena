using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;

public class EventHooks : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnHookGripRelease() {
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            ((ConnectedPlayerManager) o).SpawnRobot();
        }
    }

    public void OnGraspStart(InteractionController controller) {
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            ((ConnectedPlayerManager) o).StartMoveRobot(controller.isLeft ? 0 : 1);
        }
    }

    public void OnGraspEnd(InteractionController controller) {
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            ((ConnectedPlayerManager) o).FinishMoveRobot(controller.isLeft ? 0 : 1);
        }
    }

    public void OnMoveButtonPressed() {
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            ((ConnectedPlayerManager) o).StartMove();
        }
    }

    public void OnActButtonPressed() {
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            ((ConnectedPlayerManager) o).StartAct();
        }
    }

    public void OnPassButtonPressed() {
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            ((ConnectedPlayerManager) o).StartPass();
        }
    }
}
