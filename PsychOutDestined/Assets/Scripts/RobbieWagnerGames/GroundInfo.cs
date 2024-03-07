using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GroundType
{
    Default = 0,
    Rock = 1,
    Grass = 2,
    Snow = 3,
    Wood = 4,
}

public class GroundInfo : MonoBehaviour
{
    [SerializeField] public GroundType groundType = GroundType.Default;
}
