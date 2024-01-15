using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobbieWagnerGames
{
    public enum TileType
    {
        None = -1,
        Default = 0,
        Water = 1,
        Beach = 2,
        Grass = 3,
        Forest = 4,
    }
    public class GridTile
    {
        public TileType tileType {get; private set;}

        public GridTile(TileType gridTileType = TileType.Default)
        {
            tileType = gridTileType;
        }

        public void ChangeTileType(TileType newType)
        {
            tileType = newType;
        }
    }
}