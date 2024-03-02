// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 11-10-2022
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 01-05-2023
// ***********************************************************************
// <copyright file="TpPainterScanners.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// ***********************************************************************
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;
using static TilePlus.TpLib;

namespace TilePlus.Editor.Painter
{
    /// <summary>
    /// Static class used to scan for tilemaps and Unity Palettes.
    /// Also scans for TpPainter transform assets.
    /// </summary>
    [InitializeOnLoad]
    public static class TpPainterScanners 
    {
        #region scannerDataForTilemaps
        private static          List<Tilemap>           s_TilemapScannerList         = new(8);
        private static readonly HashSet<Tilemap>        s_PreviousTilemapsHash       = new(8);
        private static readonly HashSet<Tilemap>        s_CurrentTilemapsHash        = new(8);
        private static          List<Tilemap>           s_CurrentlyAvailableTilemaps = new(); 
        private static readonly Dictionary<int, string> s_MapIdToMapNameMap          = new(8);
        private static readonly Dictionary<Tilemap,Grid> s_TilemapToGridDict         = new(8);
        
        /// <summary>
        /// Active tilemaps
        /// </summary>
        internal static         List<Tilemap>           CurrentTilemaps => s_CurrentlyAvailableTilemaps;
        #endregion
        
        
        
        #region scannerDataForPalettes
        /// <summary>
        /// source for the list of palettes, tilefabs, chunks, and Favorites when in Palettes view.
        /// </summary>
        private static readonly List<PaletteListItem> s_CurrentlyAvailablePalettes = new(32);
        /// <summary>
        /// Active palettes
        /// </summary>
        internal static List<PaletteListItem> CurrentPalettes => s_CurrentlyAvailablePalettes;
        
        #endregion
        
        #region scannerDataForPainterTransforms
        //for TpPainterTransforms assets
        private static TpPainterTransforms s_PainterTransforms;
        /// <summary>
        /// Property to obtain painter transforms.
        /// </summary>
        public static TpPainterTransforms PainterTransforms => s_PainterTransforms;
        
        /// <summary>
        /// # of items in the transform list. Returns -1 if no transform list.
        /// </summary>
        public static int PainterTransformsCount => s_PainterTransforms != null
                                                        ? s_PainterTransforms.m_PTransformsList.Count
                                                        : -1; 
        #endregion
        
        #region CtorAndReset
        
        /// <summary>
        /// Static Contructor
        /// </summary>
        static TpPainterScanners()
        {
            ResetTilemapScanData();
            ResetPaletteScanData();
            s_PainterTransforms = null;
        }
        
        /// <summary>
        /// Reset the tilemap data structures
        /// </summary>
        internal static void ResetTilemapScanData()
        {
            s_TilemapScannerList.Clear();
            s_PreviousTilemapsHash.Clear();
            s_CurrentTilemapsHash.Clear();
            s_CurrentlyAvailableTilemaps?.Clear(); //this could possibly be null, see LINQ code below
            s_MapIdToMapNameMap.Clear();
            s_TilemapToGridDict.Clear();
            MoreThanOneGrid = false;

        }

        /// <summary>
        /// Reset the palette data
        /// </summary>
        internal static void ResetPaletteScanData()
        {
            s_CurrentlyAvailablePalettes.Clear();
        }
        
        #endregion

        #region Scanners
        
        /// <summary>
        /// Get the TpPainterTransforms asset from the DB.
        /// </summary>
        public static void TransformAssetScanner()
        {
            s_PainterTransforms = TpPainterTransforms.instance;
        }


        internal static void ValidateMapCache()
        {
            var valid = true;
            foreach (var map in s_CurrentlyAvailableTilemaps)
            {
                if (map == null)
                {
                    valid = false;
                    break;
                }
            }

            if (!valid)
                TilemapsScan();

        }
        
        /// <summary>
        /// Scans scene for tilemaps
        /// </summary>
        /// <param name="testForChanges">test for changes to # or instances of tilemaps</param>
        /// <returns>TRUE for changes</returns>
        internal static bool TilemapsScan(bool testForChanges = false)
        {
            s_TilemapScannerList.Clear();
            s_TilemapToGridDict.Clear();
            MoreThanOneGrid = false;
            foreach (var scene in GetAllScenes())
                GetTilemapsInScene(scene, ref s_TilemapScannerList, true);
            
            if(s_TilemapScannerList.Count == 0)
            {
                s_CurrentlyAvailableTilemaps = s_TilemapScannerList; //zero-length list
                return true;
            }

            s_PreviousTilemapsHash.Clear();
            for(var i= 0; i < s_CurrentlyAvailableTilemaps.Count; i++)
                s_PreviousTilemapsHash.Add(s_CurrentlyAvailableTilemaps[i]);
            
            s_CurrentlyAvailableTilemaps = s_TilemapScannerList.Where(map =>
                                                                    {
                                                                        GameObject gameObject;
                                                                        return map.layoutGrid != null                            //eliminates palette maps
                                                                               && !map.TryGetComponent(typeof(TpNoPaint), out _) //eliminates unpaintable maps
                                                                               && (gameObject = map.gameObject).hideFlags != HideFlags.NotEditable
                                                                               && gameObject.hideFlags != HideFlags.HideAndDontSave;
                                                                    }).ToList();

            if (TilePlusPainterConfig.instance.TpPainterTilemapSorting)
            {
                s_CurrentlyAvailableTilemaps = s_CurrentlyAvailableTilemaps.OrderBy
                (tmap =>
                {
                    var renderer = tmap.gameObject.GetComponent<TilemapRenderer>();
                    return renderer.sortingLayerID;

                }).ThenBy(map =>
                {
                    var renderer = map.gameObject.GetComponent<TilemapRenderer>();
                    return renderer.sortingOrder;
                }).ToList();
            }
            else
                s_CurrentlyAvailableTilemaps = s_CurrentlyAvailableTilemaps.OrderBy(tmap => tmap.name).ToList();
            

            foreach (var map in s_CurrentlyAvailableTilemaps)
            {
                var grid = GetParentGrid(map.transform);
                if (grid != null)
                    s_TilemapToGridDict.Add(map, grid);
            }

            var gridHash = s_TilemapToGridDict.Values.ToHashSet();
            MoreThanOneGrid = gridHash.Count  > 1;
            
            var previousMapsCount = s_PreviousTilemapsHash.Count; //prior # of maps.
            
            //simplest case for different set of maps is if these two counts are different.
            if (previousMapsCount == 0 || !testForChanges || s_CurrentlyAvailableTilemaps.Count != previousMapsCount)
                return true;
            
            //a bit more work to do otherwise
            //has one of the NAMES changed?
            var nameHasChanged = false;
            var numMaps        = s_CurrentlyAvailableTilemaps.Count;
            for(var i = 0; i < numMaps; i++)
            {
                var map = s_CurrentlyAvailableTilemaps[i];
                //is this map a new map?
                if(!s_MapIdToMapNameMap.TryGetValue(map.GetInstanceID(), out var mapName))
                {
                    nameHasChanged = true;
                    break;
                }
                //if not, has the name changed?
                if(mapName != map.name)
                {
                    nameHasChanged = true;
                    break;
                }
            }

            s_MapIdToMapNameMap.Clear();
            for(var i = 0; i < numMaps; i++)
            {
                var map = s_CurrentlyAvailableTilemaps[i];
                s_MapIdToMapNameMap.Add(map.GetInstanceID(),map.name);
            }

            if (nameHasChanged)
                return true;
            
            //are there any changes to the number of maps?
            s_CurrentTilemapsHash.Clear();
            for(var i= 0; i < s_CurrentlyAvailableTilemaps.Count; i++)
                s_CurrentTilemapsHash.Add(s_CurrentlyAvailableTilemaps[i]);
            var same    = s_PreviousTilemapsHash.SetEquals(s_CurrentTilemapsHash);
            return !same;
        }

        /// <summary>
        /// Get a grid corresponding to the tilemap
        /// </summary>
        /// <param name="map">tilemap</param>
        /// <param name="grid">output placed here</param>
        /// <returns></returns>
        public static bool GetGridForTilemap([NotNull] Tilemap map, [CanBeNull] out Grid grid)
        {
            return s_TilemapToGridDict.TryGetValue(map, out grid);
        }
        
        /// <summary>
        /// Is there more than one Grid?
        /// </summary>
        public static bool MoreThanOneGrid { get; set; }
        
        /// <summary>
        /// Scans for palettes, chunks, tilefabs
        /// </summary>
        internal static void PalettesScan([NotNull] string searchString)
        {
            s_CurrentlyAvailablePalettes.Clear();
            //when this is true, only show chunk-tilefabs with proper chunk size
            var chunkSizeCheck = TilePlusPainterConfig.instance.PainterFabAuthoringMode;
            var chunkSize      = TilePlusPainterConfig.instance.PainterFabAuthoringChunkSize;


            var filter    = searchString.Trim().ToLowerInvariant();
            var useFilter = filter != string.Empty;
            
            //added check for palettes in 2.01 (GridPaintingState.palettes.Count != 0)
            if (!chunkSizeCheck && TilePlusPainterConfig.instance.TpPainterShowPalettes && GridPaintingState.palettes.Count != 0)
            {
                //find palettes
                s_CurrentlyAvailablePalettes.AddRange(!useFilter
                                                          ? GridPaintingState.palettes.Where(go=>go.layer == 0 ).Select(go => new PaletteListItem(go))
                                                          : GridPaintingState.palettes.Where(go => go.layer == 0 && go.name.ToLowerInvariant().Contains(filter))
                                                                             .Select(gameObj => new PaletteListItem(gameObj)));

            }

            if (!chunkSizeCheck && TilePlusPainterConfig.instance.TpPainterShowCombinedTiles)
            {
                //find Bundles
                var guids = AssetDatabase.FindAssets("t:TpTileBundle");
                for (var i = 0; i < guids.Length; i++)
                {
                    var bundle = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), typeof(TpTileBundle)) as TpTileBundle;
                    if (bundle == null)
                        continue;
                    if (useFilter)
                    {
                        var assetName = bundle.name.Split('(');
                        if (assetName != null
                            && assetName.Length != 0
                            && assetName[0] != string.Empty
                            && !assetName[0].ToLowerInvariant().Contains(filter))
                            continue;

                    }


                    var chunk = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(bundle)) as TpTileBundle;
                    if (chunk == null || chunk.m_IgnoreInPainter)
                        continue;
                    

                    s_CurrentlyAvailablePalettes.Add(new PaletteListItem(chunk));
                }

            }

            if (chunkSizeCheck || TilePlusPainterConfig.instance.TpPainterShowTilefabs) 
            {
                //find tilefabs
                var guids          = AssetDatabase.FindAssets("t:TpTileFab");
                for (var i = 0; i < guids.Length; i++)
                {
                    var fabAsset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), typeof(TpTileFab)) as TpTileFab;
                    if (fabAsset == null)
                        continue;
                    if (useFilter)
                    {
                        var assetName = fabAsset.name.Split('(');
                        if (assetName != null
                            && assetName.Length != 0
                            && assetName[0] != string.Empty
                            && !assetName[0].ToLowerInvariant().Contains(filter))
                            continue;

                    }


                    var fab = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(fabAsset)) as TpTileFab;
                    if (fab == null || fab.m_IgnoreInPainter)
                        continue;
                    
                    if(chunkSizeCheck && (!fab.m_FromGridSelection || fab.LargestBounds.size.x != chunkSize || fab.LargestBounds.size.y != chunkSize))
                       continue;

                    s_CurrentlyAvailablePalettes.Add(new PaletteListItem(fab));
                }
            }
            
        }
    #endregion
    }
}
