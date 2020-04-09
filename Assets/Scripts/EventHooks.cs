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

    public  void OnGraspStart(bool left) {
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            ((ConnectedPlayerManager) o).StartMoveRobot(gameObject);
        }
    }

    public void OnGraspEnd(bool left) {
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            ((ConnectedPlayerManager) o).FinishMoveRobot(gameObject);
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

    public void OnActButtonPressed(int button) {
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            ((ConnectedPlayerManager) o).DoAction(button);
        }
    }

    public void OnSelectPlayerButtonPressed() {
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            ((ConnectedPlayerManager) o).OnPlayerActChose(gameObject.transform.parent.parent.gameObject);
        }
    }

    public void OnBombGrasped() {
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Rigidbody>().useGravity = true;
    }

    public void OnBombGraspEnd() {
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Rigidbody>().useGravity = true;
        gameObject.GetComponent<Rigidbody>().velocity *= 7.5F;
    }

    public void FingerExtended(int finger) {
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            ((ConnectedPlayerManager) o).FingerExtended(true);
        }
    }

    public void FingerWithdrawn(int finger) {
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            ((ConnectedPlayerManager) o).FingerExtended(false);
        }
    }

    public void OnBothHandsOpened() {
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            ((ConnectedPlayerManager) o).HandsOpened(true);
        }
    }

    public void OnBothHandsClosed() {
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            ((ConnectedPlayerManager) o).HandsOpened(false);
        }
    }
}
