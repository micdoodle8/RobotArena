﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideOnStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        gameObject.active = false;
    }
}
