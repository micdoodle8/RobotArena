#pragma warning disable 0618
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

public class HandSwitcher : MonoBehaviour
{
    private HandModelManager handMgr;

    // Start is called before the first frame update
    void Start()
    {
        handMgr = GetComponent<HandModelManager>();
    }

    public void SwitchHands() {
        handMgr.ToggleGroup("RobotHands");
        handMgr.ToggleGroup("PolyHands");
    }
}
