#pragma warning disable 0618
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject leapRig;
    private bool lastFocused = false;

    void Start()
    {
        lastFocused = Application.isFocused;
        Application.runInBackground = true;
    }

    // Update is called once per frame
    void Update()
    {
        bool focused = Application.isFocused;
        if (focused && !lastFocused) {
            leapRig.SetActive(true);
        } else if (!focused && lastFocused) {
            leapRig.SetActive(false);
        }
        lastFocused = focused;
    }
}
