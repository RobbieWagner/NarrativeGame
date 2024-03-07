// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 11-08-2022
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 12-31-2022
// ***********************************************************************
// <copyright file="TpPainterDataGUI.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>GUI for inspectors in RIGHTMOST col of Tile+Painter</summary>
// ***********************************************************************

using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using static TilePlus.Editor.TpIconLib;

namespace TilePlus.Editor.Painter
{


    /// <summary>
    /// Static class with GUI for RIGHTMOST col of Tile+Painter
    /// </summary>
    internal static class TpPainterDataGUI
    {
        private static readonly GUIContent s_BrInsFocusGUIContent = new(FindIcon(TpIconType.UnityOrbitIcon), "Focus Project Window on Tile asset");
        private static readonly GUIContent s_BrInsOpenInEditorGUIContent = new(FindIcon(TpIconType.UnityTextAssetIcon), "Open Tile script in Editor");
        private static readonly GUIContent s_OpenInInspectorButtonGUIContent = new GUIContent(FindIcon(TpIconType.InfoIcon), "Open in Inspector");
        
        private static TilePlusPainterConfig Config  => TilePlusPainterConfig.instance;
        private const float          PreviewSize                     = 1f;
        private const  string         GuiText_UpdatingWhilePlaying    = "Updating while Playing is ON";
        private const  string         GuiText_NotUpdatingWhilePlaying = "Hidden while Playing. Toggle the 'Update in Play' setting to change.";

        [CanBeNull]
        private static TilePlusPainterWindow Win => TilePlusPainterWindow.instance;
        
        internal static void ImitateSelectionInspector()
        {
            GUI.skin = TpSkin;
            var tileTarget = Win!.TileTarget;
            GUILayout.Space(2);

            //when in TILEMAP global mode (the only time this code will be entered)
            if (tileTarget is not { Valid: true } )
            {
                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    EditorGUILayout.HelpBox("No selection: choose a tile from the list in the\nmiddle column or use the PICK tool.", MessageType.Info);
                }

                GUI.skin = null;
                return;
            }
            
            
            var tile         = tileTarget.Tile;

            if (tile == null ) //can happen after ending Play mode
            {
                if(tileTarget.SourceTilemap!= null)
                    tile = tileTarget.SourceTilemap.GetTile(tileTarget.Position);
                else
                {
                    GUI.skin = null;
                    return;
                }
            }
            
            var pos          = tileTarget.Position; //invalid when it's a TileBaseSubclass variety
            var tilemap      = tileTarget.SourceTilemap;

            var noEdit = false;
            var (noPaintLocked, (_, _, inPrefab, inStage)) = TpLibEditor.NoPaint(tilemap);

            if(noPaintLocked || inPrefab)
            {
                noEdit = true;
                var msg = inStage
                              ? "Please don't modify this locked tilemap in a Prefab editing context."
                              : "This Tilemap is in a prefab. DO NOT edit if there are any TilePlus tiles!!!";
                EditorGUILayout.HelpBox(msg, MessageType.Warning);
                
            }
        
            
            if (tileTarget.WasPickedTile)
                EditorGUILayout.HelpBox("This tile was picked from the scene.", MessageType.Info);
            
            if (Application.isPlaying)
            {
                if (Config.PainterAutoRefresh)
                {
                    EditorGUILayout.HelpBox(GuiText_UpdatingWhilePlaying, MessageType.Warning);
                    EditorGUILayout.Separator();
                }
                else
                {
                    EditorGUILayout.HelpBox(GuiText_NotUpdatingWhilePlaying, MessageType.Warning);
                    GUI.skin = null;
                    return;
                }
            }
            
            //this means that a TILE asset is the target, this was selected from a list of a tilemap's tiles
            //can't show much since this is just an asset (note that IsTile also includes IsTileBase state
            if (tileTarget.IsNotTilePlusBase && !tileTarget.WasPickedTile ) 
            {
                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    var asset       = tileTarget.Tile;
                    var assetIsNull = asset == null;
                    EditorGUILayout.HelpBox($"Inspecting tile asset [{(assetIsNull ? "???":asset.name)}]\nTo edit a tile in a scene,\nselect a tile with the PICK tool.", MessageType.None);
                    EditorGUILayout.Separator();

                    var preview = assetIsNull ? TpIconLib.FindIcon(TpIconType.HelpIcon) :  TpPreviewUtility.PreviewIcon(asset);
                    if (preview != null)
                        GUILayout.Box(preview, GUILayout.Height(50 * PreviewSize), GUILayout.MaxWidth(50 * PreviewSize));
                
                    GUILayout.Space(20);
                    
                    if (GUILayout.Button(new GUIContent("Inspect", "Click to inspect the asset")))
                        TpEditorUtilities.OpenInspectorDelayed(asset, null);
                    GUI.skin = null;
                    return;
                }
            }
            
            
            //now inspect Tileplus tiles
            SelectionInspectorGui.Gui(tilemap,
                                      tile,
                                      pos,
                                      TilePlusConfig.instance.AutoSave,
                                      Application.isPlaying,
                                      noEdit);
            GUILayout.Space(2);

            GUI.skin = null;

        }


        internal static void ImitateBrushInspector()
        {
            var tileTarget = Win!.TileTarget;         
            GUI.skin = TpSkin;
            GUILayout.Space(2);

            if (tileTarget is not { Valid: true }) //null or invalid
            {
                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    EditorGUILayout.HelpBox(Win.m_PaletteTarget == null
                                                ? "Select a Palette from the center column"
                                                : Win.m_PaletteTarget.Count != 0 ?  "Select an item from the list above" : "Empty source" , MessageType.Info);
                }

                GUI.skin = null;
                return;
            }

            var tile      = tileTarget.Tile;
            var wasPicked = tileTarget.WasPickedTile;


            var config = TilePlusConfig.instance;
            if (config == null)
            {
                EditorGUILayout.HelpBox("Can't get configuration.", MessageType.Error);
                GUI.skin = null;
                return;
            }

            var buttonSize = config.BrushInspectorButtonSize;

            if (wasPicked)
            {
                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    // EditorGUILayout.HelpBox(tile is TilePlusBase && win.CurrentTool != TpPainterTool.Move
                    EditorGUILayout.HelpBox(tileTarget is { IsTilePlusBase: true } && Win.CurrentTool != TpPainterTool.Move
                                                ? "Picked from scene: TilePlus tile in Clipboard will be re-cloned if painted."
                                                : "Picked from scene", MessageType.None);
                }
            }

            if(!wasPicked && tileTarget.ItemVariety == TargetTileData.Variety.TileItem && tileTarget.IsClonedTilePlusBase)
                EditorGUILayout.HelpBox("Selected tile is a clone: it'll be be re-cloned if painted.", MessageType.None);

            else if (tileTarget.ItemVariety == TargetTileData.Variety.BundleItem)
            {
                var chunk = tileTarget.Bundle;
                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    EditorGUILayout.HelpBox($"Inspecting Bundle [{(chunk == null ? "Unknown!" : chunk.name)}]", MessageType.None);

                    if (GUILayout.Button(new GUIContent("Inspect", "Click to inspect the asset")))
                        TpEditorUtilities.OpenInspectorDelayed(chunk, null);
                }
                GUI.skin = null;
                return;
            }

            else if (tileTarget.ItemVariety == TargetTileData.Variety.TileFabItem)
            {
                var fab = tileTarget.TileFab;
                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    EditorGUILayout.HelpBox($"Inspecting a TileFab [{(fab == null ? "Unknown!": fab.name)}]", MessageType.None);

                    if (GUILayout.Button(new GUIContent("Inspect", "Click to inspect the asset")))
                        TpEditorUtilities.OpenInspectorDelayed(fab, null);
                }
                GUI.skin = null;
                return;
            }

            //so it's a tile of some sort: Tile, TileBase, TilePlus. Which is it???
            if (tileTarget is { IsTilePlusBase:true, Tile: ITilePlus iTilePlusInstance })
            {   
                GameObject prefab            = null;
                var        hasPrefab         = false;
                //var        iTilePlusInstance = tileTarget.Tile as ITilePlus;
                var        id                = tileTarget.Id;

                if (iTilePlusInstance.InstantiatedGameObject != null)
                {
                    prefab    = iTilePlusInstance.InstantiatedGameObject;
                    hasPrefab = prefab != null;
                }

                const int truncateAt = 40;
                EditorGUILayout.BeginHorizontal();
                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    var prefabInfo = string.Empty;
                    if (hasPrefab)
                        prefabInfo = $"\nPrefab: {prefab.name}";

                    var displayString = $"Name: {iTilePlusInstance.TileName}  {(wasPicked ? "ID:" : string.Empty)} {(wasPicked ? id : string.Empty)}{prefabInfo} ";
                    var desc          = iTilePlusInstance.Description;
                    if (desc == string.Empty)
                        desc = "Description: missing";
                    else if (desc.Length > truncateAt)
                        desc = desc.Substring(0, truncateAt - 1);
                    displayString += $"\nDescription:{desc}";
                    displayString += $"\nClearmode: {iTilePlusInstance.TileSpriteClear.ToString()}, ColliderMode: {iTilePlusInstance.TileColliderMode.ToString()}";
                    if (!string.IsNullOrEmpty(iTilePlusInstance.CustomTileInfo))
                        displayString += $"\nCustomInfo: {iTilePlusInstance.CustomTileInfo}";
                    var guiContent = new GUIContent(displayString) { tooltip = "Basic info about the tile selected in the palette. First line: Name (Type) [State]" };

                    EditorGUILayout.HelpBox(guiContent);
                    
                    using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(false)))
                    {
                        using (new EditorGUIUtility.IconSizeScope(new Vector2(buttonSize, buttonSize)))
                        {
                            if (GUILayout.Button(s_OpenInInspectorButtonGUIContent))
                                TpEditorUtilities.OpenInspectorDelayed(tile, null);
                            
                            if (GUILayout.Button(s_BrInsFocusGUIContent))
                                TpEditorUtilities.FocusProjectWindowDelayed(tile);
                            if (GUILayout.Button(s_BrInsOpenInEditorGUIContent))
                                TpEditorUtilities.OpenInIdeDelayed(tile);
                        }
                    }

                    //show the custom content
                    using (new EditorGUILayout.VerticalScope("Box"))
                    {
                        var obj             = (Object)((TilePlusBase)iTilePlusInstance);
                        var completionState = ImGuiTileEditor.GuiForTilePlus(obj,
                                                                             TppInspectorSpec.Brush,
                                                                             iTilePlusInstance.ParentScene,
                                                                             false);
                        GUI.skin = TpSkin;
                        if (!completionState.m_FoundTaggedTile)
                            EditorGUILayout.LabelField("no custom data");
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
            else //and also end up here if a tile in a Palette, Bundle, etc
            {
                if (tile != null)
                {
                    var typ = tile.GetType();
                    using (new EditorGUILayout.VerticalScope("Box"))
                    {
                        EditorGUILayout.LabelField($"Normal tile asset ('{tile.name}') of type ({typ}) : no special features",EditorStyles.wordWrappedLabel);
                        EditorGUILayout.Space(2f);

                        using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(false)))
                        {
                            using (new EditorGUIUtility.IconSizeScope(new Vector2(buttonSize, buttonSize)))
                            {
                                if (GUILayout.Button(s_OpenInInspectorButtonGUIContent))
                                    TpEditorUtilities.OpenInspectorDelayed(tile, null);
                                if (GUILayout.Button(s_BrInsFocusGUIContent))
                                    TpEditorUtilities.FocusProjectWindowDelayed(tile);
                            }
                        }
                    }
                }
                else
                {
                    using (new EditorGUILayout.VerticalScope("Box"))
                    {
                        EditorGUILayout.HelpBox("No tile selected or Null tile!", MessageType.Warning, true);
                        EditorGUILayout.Space(2f);
                    }
                }
            }

            EditorGUILayout.Separator();
            GUI.skin = null;
        }

        
    }
}
