using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WarningDetectionSign : MonoBehaviour
{
    [SerializeField] GameObject sign;
    public void Start()
    {
        VisionAgent agent = GetComponent<VisionAgent>();
        agent.onDetected += ShowSign;
        agent.onLoseDetection += HideSign;
    }
    public void ShowSign()
    {
        if(sign != null)
        {
            sign.SetActive(true);
        }
    }
    public void HideSign()
    {
        if(sign != null)
        {
            sign.SetActive(false);
        }
    }
}
