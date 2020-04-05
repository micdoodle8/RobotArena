using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActingHandler : MonoBehaviour
{
    ConnectedPlayerManager playerManager;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = GetComponent<ConnectedPlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoAction(int actionNum) {
    }
}
