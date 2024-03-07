// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 07-29-2021
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 08-02-2021
// ***********************************************************************
// <copyright file="TpPrefabUtilities.cs" company="Jeff Sasmor">
//     Copyright (c) 2021 Jeff Sasmor. All rights reserved.
// </copyright>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;
using PrefabUtility = UnityEditor.PrefabUtility;

namespace TilePlus.Editor
{
    /// <summary>
    /// Utilities for dealing with prefabs and tile archives.
    /// </summary>
    public static class TpPrefabUtilities
    {
        /// <summary>
        /// Used when the Unity palette makes a
        /// selection and then MakeBundle is called from the Tools menu
        /// </summary>
        public enum SelectionBundling
        {
            /// <summary>
            /// Grid selection not used
            /// </summary>
            None,
            /// <summary>
            /// Bundle the Grid Selection target (ie tilemap) ONLY
            /// </summary>
            Target,
            /// <summary>
            /// Bundle ALL the tilemaps that are components on GO's that are chilren of the Grid
            /// </summary>
            All
        }


       
        
        /// <summary>
        /// Guide user thru creating tilemap archives of different types
        /// </summary>
        [MenuItem("Tools/TilePlus/Prefabs/Bundle Tilemaps", false, 1)]
        public static void MakeBundle()
        {
            SelectionBundling selectionBundling = SelectionBundling.None;
            GameObject        selectedGo        = null;
            var               hasGridSelection  = GridSelection.active;
           
            
            if (hasGridSelection)
            {
                var  selectionBoundsInt = GridSelection.position;

                if (selectionBoundsInt.size != Vector3Int.zero)
                {
                    if(GridSelection.grid != null) //if this is a grid, I have a few questions for ya
                    {
                        if (!EditorUtility.DisplayDialog("Use Grid Selection?", "There's an active Grid Selection and a Grid is selected in the Heirarchy.\nI can bundle the tiles within the Grid Selection for all child Tilemaps. How to proceed?", "Continue", "Cancel"))
                            return;
                        selectionBundling = SelectionBundling.All;
                        selectedGo        = GridSelection.grid.gameObject;
                    }
                    else if (GridSelection.target.TryGetComponent<Tilemap>(out _))
                    {
                        if (!EditorUtility.DisplayDialog("Use Grid Selection?", "There's an active Grid Selection and a Tilemap is selected in the Heirarchy.\nI can bundle the tiles within the Grid Selection for just this one tilemap. How to proceed?", "Continue", "Cancel"))
                            return;
                        selectionBundling = SelectionBundling.Target;                                                                        
                        selectedGo = GridSelection.target;
                    }
                }
            }

            if (selectionBundling == SelectionBundling.None)
            {
                if (Selection.count != 1)
                {
                    EditorUtility.DisplayDialog("SORRY!", "Cannot use multiple selection", "Continue");
                    return;
                }

                //Is the selected object a gameobject
                selectedGo = Selection.activeTransform != null
                             ? Selection.activeTransform.gameObject
                             : null;
                
            }
            if (selectedGo == null) //nope, we're done here. 
            {
                OopsMessage();
                return;
            }
            
            var selectionIsGrid =    selectedGo.TryGetComponent<Grid>(out _);


            //can't proceed within prefab editing context
            if (EditorSceneManager.IsPreviewSceneObject(selectedGo))
            {
                EditorUtility.DisplayDialog("SORRY!", "You can't do this in a prefab editing context.", "Continue");
                return;
            }
            
            //can't proceed if selection is part of a prefab
            if (PrefabUtility.IsPartOfPrefabAsset(selectedGo) || PrefabUtility.IsPartOfAnyPrefab(selectedGo))
            {
                EditorUtility.DisplayDialog("SORRY!", "The selected GameObject is part of a prefab. Can't proceed.", "Continue");
                return;
            }

            //does it have any tilemaps?
            var maps = selectedGo.GetComponentsInChildren<Tilemap>(true);
            if (maps == null || maps.Length == 0) //no tilemaps in the selection
            {
                OopsMessage();
                return;
            }

            var grids           = selectedGo.GetComponentsInChildren<Grid>(true);
            //var selectionIsGrid = selectedGo.TryGetComponent<Grid>(out _);
            
            if (selectionIsGrid)
            {
                //should only be one grid
                if (grids == null) //should not happen
                    return;
                if (grids.Length != 1)
                {
                    EditorUtility.DisplayDialog("SORRY!", "Too many grids! Must be a single grid with child tilemaps. Can't proceed.", "Continue");
                    return;
                }
            }
            else
            {
                if (!EditorUtility.DisplayDialog("No Grid", "Making a prefab out of a Tilemap without including the parent Grid: proceed?", "OK", "Cancel"))
                    return;
            }
            
            //Are any maps already in a prefab? 
            if (maps.Any(map => PrefabUtility.IsPartOfPrefabAsset(map) || PrefabUtility.IsPartOfAnyPrefab(map)))
            {
                EditorUtility.DisplayDialog("SORRY!", "One or more Tilemaps are already in Prefabs. Can't proceed.", "Continue");
                return;
            }

            //ask user for destination.
            var assetsPath = Path.GetDirectoryName(Application.dataPath) + "/Assets";
            var path       = EditorUtility.SaveFolderPanel("Select destination folder for saving Bundle and TileFab assets.", assetsPath, "");
            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.Log($"User cancelled saving Bundle and TileFab asset(s).");
                return;
            }

            //don't want root of assets folder
            var index = path.IndexOf("Assets", StringComparison.Ordinal);
            path = path.Substring(index);
            if (path is "Assets" or "")
            {
                EditorUtility.DisplayDialog("Not there!", "Choose a subfolder of 'Assets' - cancelling", "Continue");
                return;
            }

            
            //if parent GO or the heirarchy had a grid component somewhere, ask if we want to make a prefab.
            var makeTotalBundle = selectionIsGrid &&
                                  EditorUtility.DisplayDialog("Make a prefab?",
                                      "Make a prefab of the Grid and child Tilemaps?",
                                      "Yes",
                                      "No");

            //if making total bundle then no need to deal with the prefabs as that function will deal with it.
            var whatAboutPrefabsYo = makeTotalBundle
                ? 1
                : EditorUtility.DisplayDialogComplex("Bundle prefabs?",
                    "Bundle child Prefabs, too? Choose Variant or Unpacked, New Prefab.",
                    "Variant",
                    "No",
                    "Unpacked");

            //return value is 0 for Variant, 1 for No, 2 for Unpacked;
            var includePrefabs = whatAboutPrefabsYo != 1;
            var unpack         = whatAboutPrefabsYo == 2;

            Pack(selectedGo.scene.name,
                 path,
                 selectedGo,
                 hasGridSelection
                     ? GridSelection.position
                     : new BoundsInt(),
                 maps,
                 false,
                 makeTotalBundle,
                 selectionBundling,
                 includePrefabs,
                 unpack);
        }


        /// <summary>
        /// Pack a TileFab without all of the incessant questions! 
        /// </summary>
        /// <param name="sceneName">Name of the scene that the TileFab came from</param>
        /// <param name="path">Path for the output files</param>
        /// <param name="selectedGo">The selection object</param>
        /// <param name = "selectionBounds" >Bounds of the selection, if applicable. Can be null</param>
        /// <param name="maps">the maps to be bundled</param>
        /// <param name = "silent" >No messages if true</param>
        /// <param name="createPrefab">Create a prefab after the TileFab is created</param>
        /// <param name="selectionBundling">A value from the SelectionBundling Enum</param>
        /// <param name="includePrefabs">include prefabs in the bundle</param>
        /// <param name="unpack">create variant prefabs</param>
        /// <param name = "hideFromPainter" >set the 'Ignore in Painter' field to true if this is true</param>
        /// <returns> tuple of bounds (can be zero size), assetName (can be empty), path (can be empty),
        /// the fab instance (do not retain ourside of calling scope),
        /// and a bool (false if any sort of failure) and a second bool that indicates that failure was due to an empty region being examined</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public static (BoundsInt boundInt, string tileFabAssetName, string tileFabAssetPath, TpTileFab fab, bool success, bool failureWasEmptyArea) 
            Pack(string            sceneName,
                 string            path,
                 GameObject        selectedGo,
                 BoundsInt?        selectionBounds,
                 Tilemap[]         maps,
                 bool              silent            = false,
                 bool              createPrefab      = false,
                 SelectionBundling selectionBundling = SelectionBundling.All,
                 bool              includePrefabs    = true,
                 bool              unpack            = false,
                 bool              hideFromPainter   = false)
{
            
            //map tilemaps to generated assets
            var mapToBlobDict = new Dictionary<Tilemap, TpTileBundle>();

            var totalTpTilesCount    = 0;
            var totalUnityTilesCount = 0;
            var totalPrefabsCount    = 0;
            var timeStamp            = DateTime.UtcNow.ToString(@"M/d/yyyy hh:mm:ss tt") + " UTC";
            var blobPaths            = new List<string>();

            if (!selectedGo.TryGetComponent<Grid>(out _))
            {
                Debug.LogError("Top-level object was not GRID!");
                return (new BoundsInt(), string.Empty, string.Empty, null, false,false);
            }
            
            //scan all maps
            foreach (var map in maps)
            {
                //these two are used to adjust the position values if selectionBundling
                var gridSelOffset   = Vector3Int.zero;
                var worldSelOffset  = Vector3.zero;
                var zeroBasedBounds = new BoundsInt();

                BoundsInt bounds;
                if (selectionBundling == SelectionBundling.None || !selectionBounds.HasValue)
                {
                    map.CompressBounds();
                    bounds = map.cellBounds;
                }
                else
                {
                    bounds          = selectionBounds.Value;
                    gridSelOffset   = selectionBounds.Value.position;
                    worldSelOffset  = map.CellToWorld(gridSelOffset); 
                    zeroBasedBounds = new BoundsInt(Vector3Int.zero, selectionBounds.Value.size);
                }

                //Debug.Log($"BundlingMode: {selectionBundling} Bounds: {bounds}  map{map}");

                List<Transform> childPrefabs = null;
                int             childPrefabCount = 0;
                if (includePrefabs)
                {
                    childPrefabs = new List<Transform>();
                    var transform = map.transform;
                    var cCount    = transform.childCount;
                    //if chidren, add them to child prefabs list for later use
                    if (cCount != 0)
                    {
                        for (var i = 0; i < cCount; i++)
                        {
                            var child = transform.GetChild(i);
                            if (selectionBundling != SelectionBundling.None)
                            {
                                if(!bounds.Contains(map.WorldToCell(child.transform.position)))
                                   continue;
                            }
                            if (PrefabUtility.IsAnyPrefabInstanceRoot(child.gameObject))
                                childPrefabs.Add(child);
                        }
                    }

                    childPrefabCount = childPrefabs.Count;
                }

                //now deal with the map
                
                //separate lists for TP and UNITY tiles
                var tpTilesList        = new List<(Vector3Int pos, TilePlusBase t)>();
                var unityTilesList     = new List<(Vector3Int pos, TileBase tb)>();
                foreach (var pos in bounds.allPositionsWithin)
                {
                    var tile = map.GetTile(pos);
                    if (tile == null)
                        continue;
                    // ReSharper disable once ConvertIfStatementToSwitchStatement
                    if (tile is TilePlusBase tpb)
                        tpTilesList.Add((pos - gridSelOffset, tpb));
                    else  //TileBase is the only other possibility (this includes Tile)
                        unityTilesList.Add((pos - gridSelOffset, tile));
                }

                var tpTilesCount    = tpTilesList.Count;
                var unityTilesCount = unityTilesList.Count;
                //if nothing found, nothing to do on this map.
                if (tpTilesCount == 0 && unityTilesCount == 0 && childPrefabCount == 0) 
                {
                    Debug.Log($"No tiles or prefabs found on map {map} within bounds {bounds}");
                    continue;
                }

                totalTpTilesCount    += tpTilesCount;
                totalUnityTilesCount += unityTilesCount;
                totalPrefabsCount += childPrefabCount;

                //create the combined tiles asset.
                var sObj = ScriptableObject.CreateInstance<TpTileBundle>();
                if (sObj == null)
                {
                    Debug.LogError("Could not create TpTileBundle instance. Cancelling operation.");
                    return (new BoundsInt(), string.Empty, string.Empty, null, false,false);
                }

                var baseName      = map.transform.parent.name;
                var blobAssetName = $"{path}/{baseName}_{map.name}.Asset";
                sObj.m_TimeStamp         = timeStamp;
                sObj.m_ScenePath         = GetScenePathUpwards(map.gameObject);
                sObj.m_OriginalScene     = sceneName;
                sObj.m_TilemapBoundsInt = selectionBundling == SelectionBundling.None
                                              ? bounds
                                              : zeroBasedBounds;
                sObj.m_FromGridSelection = selectionBundling != SelectionBundling.None;
                sObj.AddGuid();
                var objPath = AssetDatabase.GenerateUniqueAssetPath(blobAssetName);
                AssetDatabase.CreateAsset(sObj, objPath);
                blobPaths.Add(objPath);

                //load it up
                var blob = AssetDatabase.LoadAssetAtPath<TpTileBundle>(objPath);
                //update the map->blobasset mapping
                mapToBlobDict.Add(map, blob);
                if (hideFromPainter)
                    blob.m_IgnoreInPainter = true;

                //deal with prefabs if necc.
                if (includePrefabs && childPrefabs != null && childPrefabs.Count != 0)
                {
                    foreach (var transform in childPrefabs)
                    {
                        var gameObj   = transform.gameObject;
                        var pPath     = $"{path}/{baseName}_{map.name}_{gameObj.name}.prefab";
                        var pObjPath  = AssetDatabase.GenerateUniqueAssetPath(pPath);
                        var newPrefab = PrefabUtility.SaveAsPrefabAsset(gameObj, pObjPath);
                        if (newPrefab != null)
                            blob.AddPrefab(newPrefab, transform.position - worldSelOffset);
                        if (unpack)
                        {
                            var newPrefabPath  = AssetDatabase.GetAssetPath(newPrefab);
                            var prefabToUnpack = PrefabUtility.LoadPrefabContents(newPrefabPath);
                            PrefabUtility.UnpackPrefabInstance(prefabToUnpack, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                            PrefabUtility.SaveAsPrefabAsset(prefabToUnpack, newPrefabPath);
                            PrefabUtility.UnloadPrefabContents(prefabToUnpack);
                        }
                    }
                }

                //lock up the TP tiles and add to blob                
                foreach (var (pos, t) in tpTilesList)
                {
                    var copy = UnityEngine.Object.Instantiate(t);
                    if (!copy.ChangeTileState(TileResetOperation.MakeLockedAsset))
                    {
                        Debug.Log("State change failed.");
                        continue;
                    }

                    AssetDatabase.AddObjectToAsset(copy, blob);
                    blob.AddTpbToListOfTiles(map, pos, copy, t);
                }

                //add the UNITY tiles to blob
                foreach (var (pos, ut) in unityTilesList)
                    blob.AddUnityTileToListOfTiles(map, pos, ut);

                TpLibEditor.PrefabBuildingActive = true;
                map.RefreshAllTiles(); //a bit of magic
                TpLibEditor.PrefabBuildingActive = false;

                blob.Seal();           //finalize the data structures in the blob
                
                //save it all
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
            }
            AssetDatabase.ForceReserializeAssets(blobPaths,ForceReserializeAssetsOptions.ReserializeAssets);
            
            var totalNumTiles = totalTpTilesCount + totalUnityTilesCount;

            //report to user
            if (!silent)
            {
                var actualBounds = selectionBundling !=  SelectionBundling.All
                                       ? GridSelection.position
                                       : selectionBounds.Value;
                
                var msg = $"Nothing found. Selection: Sel= {actualBounds.ToString()} Obj = {selectedGo} ";
                var mapS = maps.Length == 1
                               ? string.Empty
                               : "s";
                EditorUtility.DisplayDialog("Results",
                                            totalNumTiles == 0 && totalPrefabsCount == 0
                                                ? msg
                                                : $"Bundled {totalTpTilesCount + totalUnityTilesCount} Tiles {(includePrefabs && totalPrefabsCount != 0 ? totalPrefabsCount.ToString():"")} for {maps.Length} Tilemap{mapS}. Output is in the Project folder: {path}",
                                            "Continue");
            }

            if (totalNumTiles == 0 && totalPrefabsCount == 0) 
                return  (new BoundsInt(), string.Empty, string.Empty,null, false,true);

            //create tilefab asset
            var tileFabSobj = ScriptableObject.CreateInstance<TpTileFab>();
            if (tileFabSobj == null)
            {
                Debug.LogError("Could not create TpTileFab instance. Cancelling operation.");
                return  (new BoundsInt(),string.Empty,string.Empty,null, false, false);
            }
            
            if (hideFromPainter)
                tileFabSobj.m_IgnoreInPainter = true;
            var tileFabName = $"{selectedGo.name}_TileFab";
            var tileFabPath = $"{path}/{tileFabName}.asset";
            tileFabSobj.m_TimeStamp         = timeStamp;
            tileFabSobj.m_ScenePath         = GetScenePathUpwards(selectedGo);
            tileFabSobj.m_OriginalScene     = sceneName;
            tileFabSobj.m_FromGridSelection = selectionBundling != SelectionBundling.None;
            tileFabSobj.AddGuid(); 
            foreach (var map in maps)
            {
                if (mapToBlobDict.TryGetValue(map, out var bundle))
                    tileFabSobj.m_TileAssets.Add(new TpTileFab.AssetSpec
                    {
                        m_Asset       = bundle,
                        m_TilemapName = map.name,
                        m_TilemapTag  = map.tag
                    });
            }
            
            var tileFabObjPath     = AssetDatabase.GenerateUniqueAssetPath(tileFabPath);

            AssetDatabase.CreateAsset(tileFabSobj, tileFabObjPath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //if not creating a prefab we are done.
            if (!createPrefab)
            {
                return (tileFabSobj.LargestBounds, tileFabName, tileFabPath, tileFabSobj, true, false);
            }

            //else make a big prefab out of all we've done so far, excluding the tilefab.
            var bundleName = $"{selectedGo.name}_bundle";
            var bundlePath = $"{path}/{bundleName}.prefab";

            TpLibEditor.PrefabBuildingActive = true;
            var baseGameObject = UnityEngine.Object.Instantiate(selectedGo, null); //copy the grid or top-level GO
            if(!baseGameObject.TryGetComponent<Grid>(out _))
            {
                Debug.LogError("Error: top level object was not a Grid. The Prefab was not created but the TileFabs and Bundles are fine.");
                UnityEngine.Object.DestroyImmediate(baseGameObject);
                return (new BoundsInt(), string.Empty, string.Empty, null, false,false);
            }
            
            //add the prefabMarker (loads up the tilemaps when prefab is placed)
            var marker = baseGameObject.AddComponent<TpPrefabMarker>();
            if (marker == null)
            {
                Debug.LogError("Error: Could not add component to top level Grid's GO. The Prefab was not created but the TileFabs and Bundles are fine.");
                UnityEngine.Object.DestroyImmediate(baseGameObject);
                return (new BoundsInt(), string.Empty, string.Empty, null, false,false);
            }

            marker.m_TileFabForPrefab = tileFabSobj;
            
            var tilemaps       = baseGameObject.GetComponentsInChildren<Tilemap>();
            //don't want these tilemaps to have any tiles.
            foreach (var map in tilemaps)
                map.ClearAllTiles();
                //UnityEngine.Object.DestroyImmediate(map.gameObject);
            

            

            
            //save this new Grid prefab
            PrefabUtility.SaveAsPrefabAsset(baseGameObject, bundlePath);
            TpLibEditor.PrefabBuildingActive = false;

            
            
            /*
            /*reopen the grid prefab, add tilemaps from the mapToBlob dict,
            & load the tiles from the blob asset. Must inhibit TpLibEditor's 
            heirarchy and tilemap changed actions to avoid those callbacks from
            affecting this process.
            #1#
            TpLibEditor.PrefabBuildingActive = true;
            // ReSharper disable once ConvertToUsingDeclaration
            using (var pScope = new PrefabUtility.EditPrefabContentsScope(bundlePath))
            {
                var root      = pScope.prefabContentsRoot;
                var rootTrans = root.transform;
                foreach (var map in maps)
                {
                    if (!mapToBlobDict.TryGetValue(map, out var tileBundle))
                        continue; //this happens if the tilemap had no tiles: blob asset wasn't created.
                    //recreate the tilemaps. 
                    var mapGo = UnityEngine.Object.Instantiate(map.gameObject, rootTrans);
                    mapGo.name = mapGo.name.Split(s_DelimForClone)[0];

                    //NOTE that the tpt tiles have state changed to locked. NewGuids flag set here will CLEAR GUIDs so that new ones are assigned every time 
                    //the prefab is loaded.
                    //TileFabLib.LoadBundle(tileBundle, mapGo.GetComponent<Tilemap>(), Vector3Int.zero,TpTileBundle.TilemapRotation.Zero,FabOrBundleLoadFlags.Normal | FabOrBundleLoadFlags.NoClone);
                    //mapGo.AddComponent<TpPrefabMarker>();
                    
                }
            }
            */
            

            UnityEngine.Object.DestroyImmediate(baseGameObject); //don't need this anymore
            AssetDatabase.SaveAssets();                          //just to be sure!
            AssetDatabase.Refresh();
            TpLibEditor.PrefabBuildingActive = false;
            return  (tileFabSobj.LargestBounds, tileFabName, tileFabPath, tileFabSobj, true, false);
        }

       

        /// <summary>
        /// Used from a menu item to check if a tilemap has any unlocked tiles.
        /// </summary>
        [MenuItem("Tools/TilePlus/Prefabs/Unlocked Tiles test", false, 1000)]
        public static void TilemapStatus()
        {
            var go = Selection.activeTransform != null ? Selection.activeTransform.gameObject : null;
            if (go == null)
            {
                SelectOnlyOneMsg();
                return;
            }

            var map = go.GetComponent<Tilemap>();
            if (map == null)
            {
                SelectOnlyOneMsg();
                return;
            }


            if(!TpLib.IsTilemapLocked(map))
            {
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0}", "This isn't a locked Tilemap...");
                return;
            }

            map.CompressBounds();
            var bounds = map.cellBounds;
            var cells  = map.GetTilesBlock(bounds);

            var output = new List<string>();
            foreach (var cell in cells)
            {
                if (cell is TilePlusBase {IsClone: true} tpb)
                    output.Add($"{tpb.name} at {tpb.TileGridPosition}");
            }

            if (output.Count == 0)
                Debug.Log($"No unlocked tiles on Tilemap {map.name}");
            else
            {
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "Found unlocked tiles on Tilemap {0}...", map.name);

                foreach (var s in output)
                    Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0}", s);
            }
        }

        private static void SelectOnlyOneMsg()
        {
            EditorUtility.DisplayDialog("OOPS!", "For this to work, select a single Tilemap. Try again...", "Continue");
        }
        
        private static void OopsMessage([CanBeNull] string msg = "")
        {
            var message = string.IsNullOrWhiteSpace(msg) ? "For this to work, select a Tilemap or a GameObject in a Scene, with one or more child Tilemaps. Try again..." : msg;
            EditorUtility.DisplayDialog("OOPS!", message, "Exit");

        }

        [NotNull]
        private static string GetScenePathUpwards([NotNull] GameObject current)
        {
            var s = current.name;
            var t = current.transform;
            while ((t = t.parent) != null)
                s = $"{s}.{t.name}";
            return s;
        }
        
        /// <summary>
        /// Menu item handler for Bundle
        /// </summary>
        [MenuItem("GameObject/TilePlus Bundler",false,1000)]
        public static void Bundle()
        {
            MakeBundle();
        }

        /// <summary>
        /// Validator for 'Bundle' command
        /// </summary>
        /// <returns>true if GO has grid or tilemap component</returns>
        [MenuItem("GameObject/TilePlus Bundler",true,1000)]
        public static bool BundleValidate()
        {
            if (Selection.activeTransform == null)
                return false;
            var go = Selection.activeGameObject;
            if (PrefabUtility.IsPartOfAnyPrefab(go))
                return false;
            return go.TryGetComponent<Grid>(out _) || go.TryGetComponent<Tilemap>(out _);
        }



        
        
        
        
        
   }
}
