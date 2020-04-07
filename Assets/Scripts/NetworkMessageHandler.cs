using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkMessageHandler : NetworkBehaviour
{
    public const short MOVEMENT_MSG = 1111;

    public class EntityMovementMessage : MessageBase {
        public GameObject obj;
        public Vector3 pos;
        public Quaternion rot;
        public float time;
    }
}
