using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RobbieWagnerGames
{
    public class GridFeature : MonoBehaviour
    {
        [SerializeField] protected Vector3Int startPos;
        [SerializeField] protected bool useStartingPos;

        public virtual void AddGridFeature(Grid grid)
        {
            Tilemap map = grid.GetCurrentTilemap();
            int size = grid.GetCurrentSize();
        }
    }
}