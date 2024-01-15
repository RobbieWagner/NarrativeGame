using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RobbieWagnerGames
{

    [System.Serializable]
    public class GridLayers
    {
        public TileType tileType;
        [Range(0,1)]public float tileChance;
        public UnityEngine.Tilemaps.TileBase tile;
    }

    public class Grid : MonoBehaviour
    {
        [Header("Base Settings")]
        [SerializeField] protected int size = 10;
        [SerializeField] protected Tilemap tilemap;
        GridTile[,] grid;

        [Header("Procedural Generation")]
        [SerializeField] private float perlinScale = .1f;
        [Range(0.5f,2f)][SerializeField] private float falloffScale = 0f;
        [Tooltip("Layers on grid from lowest to highest with respective chances of spawning")]
        [SerializeField] private List<GridLayers> gridLayers;
        private float xOffset;
        private float yOffset;

        [Header("Extras")]
        [SerializeField] private List<GridFeature> features;

        protected virtual void Start()
        {
            xOffset = UnityEngine.Random.Range(-10000f, 10000f);
            yOffset = UnityEngine.Random.Range(-10000f, 10000f);

            tilemap.ClearAllTiles();

            grid = new GridTile[size, size];
            for(int y = 0; y < size; y++)
            {
                for(int x = 0; x < size; x++)
                {
                    AddTileToGrid(x, y);
                }
            }

            AddGridFeatures();
        }

        protected virtual void AddGridFeatures()
        {
            foreach(GridFeature gridFeature in features)
            {
                gridFeature.AddGridFeature(this);
            }
        }

        protected void AddTileToGrid(int x, int y)
        {
            float noiseValue = CalculateNoiseValue(x, y);
            TileType tileType = TileType.None;

            float cutoff = 0f;
            int tileTypeIndex = 0;
            while(tileTypeIndex < gridLayers.Count)
            {
                cutoff += gridLayers[tileTypeIndex].tileChance;
                //Debug.Log(x + " " + y + " " + cutoff + " " + noiseValue);
                if(noiseValue < cutoff) 
                {
                    tileType = gridLayers[tileTypeIndex].tileType;
                    break;
                }

                tileTypeIndex++;
            }

            tilemap.SetTile(new Vector3Int(x, y, 0), gridLayers[tileTypeIndex].tile);

            Debug.Log(tileType);
            GridTile tile = new GridTile(tileType);
            grid[x,y] = tile;
        }

        protected virtual float CalculateNoiseValue(int x, int y)
        {
            float returnValue = Mathf.PerlinNoise(x * perlinScale + xOffset, y * perlinScale + yOffset);
            
            float xv = x/(float)size * 2 - 1;
            float yv = y/(float)size * 2 - 1;
            float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));

            returnValue -= Mathf.Pow(v, 3f)/(Mathf.Pow(v, 3f) + Mathf.Pow(2.2f - 2.2f * v, 3f)) * falloffScale;

            return returnValue;
        }

        public int GetCurrentSize() { return size; }
        public Tilemap GetCurrentTilemap() { return tilemap; }
    }
}