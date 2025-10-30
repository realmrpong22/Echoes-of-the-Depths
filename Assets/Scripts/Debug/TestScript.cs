using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

public class TestScript : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.PlayBGM("RegionMusic");
    }
}
