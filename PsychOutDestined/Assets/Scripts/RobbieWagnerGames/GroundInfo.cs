using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PsychOutDestined
{
    public enum GroundType
    {
        None,
        Stone,
        Wood,
        Dirt,
        Grass,
        Water,
        Sand,
        Gravel
    }
    public class GroundInfo : MonoBehaviour
    {
        [SerializeField] public GroundType groundType = GroundType.None;
    }
}
