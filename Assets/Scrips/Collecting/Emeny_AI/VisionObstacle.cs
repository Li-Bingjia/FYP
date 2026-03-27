using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionObstacle : MonoBehaviour
{
    public bool solid = true;
    [Range(0f,1f)]
    [SerializeField] float obstaclePower = 0f;

    public float ObstaclePower {get {return 1f - obstaclePower; } }
    public float cutOffPoint = 1f;
}
