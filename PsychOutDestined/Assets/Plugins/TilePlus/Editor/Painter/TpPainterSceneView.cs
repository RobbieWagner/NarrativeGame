// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 11-07-2022
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 01-05-2023
// ***********************************************************************
// <copyright file="TpPainterSceneView.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>

// ***********************************************************************
#nullable enable
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;
using static TilePlus.TpLib;
using static UnityEngine.ScriptableObject;
using static TilePlus.Editor.TpIconLib;
using Object = UnityEngine.Object;


namespace TilePlus.Editor.Painter
{
    
    /// <summary>
    /// Static class used to control the Scene View window
    /// </summary>
    [InitializeOnLoad]

    internal static class TpPainterSceneView
    {
        /// <summary>
        /// Information about draglock status
        /// </summary>
        public readonly struct DragLockInfo
        {
            /// <summary>
            /// Drag Lock X
            /// </summary>
            public readonly bool m_DragX;
            /// <summary>
            /// Drag Lock Y
            /// </summary>
            public readonly bool m_DragY;

            /// <summary>
            /// Ctor
            /// </summary>
            /// <param name="x">true if X values are locked</param>
            /// <param name="y">true if Y values are locked</param>
            public DragLockInfo(bool x, bool y)
            {
                m_DragX = x;
                m_DragY = y;
            }
        }
        
        
        #region properties
        /// <summary>
        /// Ignore next heirarchy change. Used when painting a Prefab
        /// instead of a tile so we can ignore the heirarchy event
        /// </summary>
        public static bool IgnoreNextHierarchyChange { get; set; }

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        /// <summary>
        /// Is preview currently-active
        /// </summary>
        public static bool PreviewActive => s_PreviewActive;

        /// <summary>
        /// Is a brush operation in progress
        /// </summary>
        public static bool BrushOpInProgress => s_BrushOpInProgress;

        /// <summary>
        /// Ref for Configuration
        /// </summary>
        private static TilePlusPainterConfig Config => TilePlusPainterConfig.instance;

        /// <summary>
        /// Get information about the Drag Lock state
        /// </summary>
        internal static DragLockInfo DragLock => new(s_DragLockX, s_DragLockY);
        

        /// <summary>
        /// Get the ScreenToLocal method.
        /// </summary>
        /// <remarks>Can't be done in constructor since
        /// it's possible that TpLib editor isn't inited prior to this class</remarks>
        private static MethodInfo? ScreenToLocal
        {
            get
            {
                if (s_GeuFuncScreenToLocalMi != null)
                    return s_GeuFuncScreenToLocalMi;
                s_GeuFuncScreenToLocalMi = TpLibEditor.GetGeuMethodInfo("ScreenToLocal", 2);
                return s_GeuFuncScreenToLocalMi;
            }
        }
        
        /// <summary>
        /// Get the state of the allow overwrite/ignore map hotkey
        /// </summary>
        /// <value>T/F</value>
        private static bool AllowOverwriteOrIgnoreMap => TpEditorUtilities.AllowPaintingOverwrite;


        internal static bool MarqueeDragActive => s_MarqueeDragActiveLastPass;
        
        internal static (bool active, BoundsInt bounds, Vector3Int startPosition) GridSelMarqueeState =>
            (s_MarqueeDragActiveLastPass, s_MarqueeDragBounds, s_MarqueeStartMousePosition);

        
        
        #endregion

        #region fields
        //for sceneview handler
        //note: these have to be initialized in Ctor. Do it right here and Unity throws an error.
        /// <summary>
        /// A preset GUI style
        /// </summary>
        private static GUIStyle? s_PositionTextGuiStyle;
        /// <summary>
        /// A preset GUI style
        /// </summary>
        private static GUIStyle? s_PositionTextAltGuiStyle;
        /// <summary>
        /// A preset GUI style
        /// </summary>
        private static GUIStyle? s_PickMsgGuiStyle;

        //preview (part of sceneview handler)
        /// <summary>
        /// is preview active?
        /// </summary>
        private static bool s_PreviewActive;
        /// <summary>
        /// The preview is for a Bundle Asset
        /// </summary>
        private static bool s_PreviewIsBundle;
        /// <summary>
        /// The preview is for a tilefab asset
        /// </summary>
        private static bool s_PreviewIsTileFab;
        /// <summary>
        /// The current preview position
        /// </summary>
        private static Vector3Int s_CurrentPreviewPosition;
        /// <summary>
        /// The current preview tilemap
        /// </summary>
        private static Tilemap? s_CurrentPreviewTilemap;
        /// <summary>
        /// A cached placeholder tile
        /// </summary>
        private static readonly Tile s_PlaceholderTile;
        /// <summary>
        /// Is preview the placeholder tile?
        /// </summary>
        private static bool s_PreviewIsPlaceholderTile;
        
        //these lists will contain tilemaps which are excluded from painting and included for painting.
        /// <summary>
        /// Tilemaps which are excluded
        /// </summary>
        private static List<string> s_ExcludedMaps = new();
        /// <summary>
        /// Tilemaps which are included
        /// </summary>
        private static List<string> s_IncludedMaps = new();

       
        /// <summary>
        /// Allowed events in SceneViewHandler
        /// </summary>
        private static readonly HashSet<EventType> s_AllowedEventsInSceneGui = new()
                                                                               {
                                                                                   EventType.Repaint,
                                                                                   EventType.MouseDown,
                                                                                   EventType.MouseUp,
                                                                                   EventType.MouseDrag
                                                                               };
        
        /// <summary>
        /// part of reflection-hack to access some Unity palette methods
        /// </summary>
        private static MethodInfo? s_GeuFuncScreenToLocalMi;
        
        
        //Paint/Erase dragging variables
        /// <summary>
        /// drag lock x
        /// </summary>
        private static bool s_DragLockX;

        /// <summary>
        /// drag lock y
        /// </summary>
        private static bool s_DragLockY;

        

        /// <summary>
        /// drag lock: constant X or Y
        /// </summary>
        private static int s_ConstantXorY;

        /// <summary>
        /// Indicates that a brush operation is in progress
        /// </summary>
        private static bool s_BrushOpInProgress; //NO serializeField on this
        
        
        /// <summary>
        /// The current mouse grid position
        /// </summary>
        private static Vector3Int s_CurrentMouseGridPosition = TilePlusBase.ImpossibleGridPosition; //the current mouse position.

        
        /// <summary>
        /// The last mouse grid position
        /// </summary>
        private static Vector3Int s_LastMouseGridPosition = TilePlusBase.ImpossibleGridPosition; //the previous mouse position
        
        private static Vector3 s_CurrentMouseLocalPosition = Vector3.zero;

        private static bool s_CurrentPaintingTilemapHasOriginZero;
        
        
        /// <summary>
        /// True if spot where user clicked isn't paintable
        /// </summary>
        private static bool s_CantPaintHereInterlock;

        /// <summary>
        /// State variable
        /// </summary>
        private static bool s_MarqueeDragActiveLastPass;

        /// <summary>
        /// Bounds for marquee when dragged. When drag ends, becomes GridSelection.
        /// </summary>
        private static BoundsInt s_MarqueeDragBounds;

        /// <summary>
        /// starting position for marquee dragging
        /// </summary>
        private static Vector3Int s_MarqueeStartMousePosition;

        
        private static float s_LastSizeAdjustment = 1;

       
        #endregion

        #region ctor
        /// <summary>
        /// Ctor
        /// </summary>
        static TpPainterSceneView()
        {
            SceneView.duringSceneGui  += OnSceneViewSceneGui;
            s_PlaceholderTile           =  CreateInstance<Tile>();
            s_PlaceholderTile.hideFlags =  HideFlags.HideAndDontSave;
            s_PlaceholderTile.sprite    =  SpriteFromTexture(FindIcon(TpIconType.UnityToolbarMinusIcon));
            ReinitializeGuiContent();

        }
        #endregion

        #region utils

        internal static void Reset()
        {
            s_DragLockX              = s_DragLockY = false;
            s_BrushOpInProgress      = false;
            s_LastMouseGridPosition  = TilePlusBase.ImpossibleGridPosition;
            s_CurrentMouseGridPosition = TilePlusBase.ImpossibleGridPosition;
            s_CantPaintHereInterlock = false;
            RemovePreview();
            ReinitializeGuiContent();

        }

        /// <summary>
        /// (Re) init the GUIContent for the scene view cursor overlays
        /// </summary>
        internal static void ReinitializeGuiContent(float sizeAdj = 1f)
        {
            s_PositionTextGuiStyle = new GUIStyle { normal = { textColor = Config.TpPainterSceneTextColor }, fontStyle = FontStyle.Bold, fontSize = (int)(TilePlusConfig.instance.BrushPositionFontSize * sizeAdj)};
            s_PositionTextAltGuiStyle = new GUIStyle { normal = { textColor = Config.TpPainterSceneTextColor }, fontStyle = FontStyle.BoldAndItalic, fontSize = (int)(TilePlusConfig.instance.BrushPositionFontSize * sizeAdj) };
            s_PickMsgGuiStyle      = new GUIStyle { normal = { textColor = Config.TpPainterSceneTextColor }, fontStyle = FontStyle.Bold, fontSize = (int)(TilePlusConfig.instance.BrushPositionFontSize * 0.75f * sizeAdj) };
        }
        
        /// <summary>
        /// Register a Unity undo.
        /// </summary>
        /// <param name="map">tilemap.</param>
        /// <param name="description">Undo description.</param>
        private static void RegisterUndo(Tilemap map, string description)
        {
            Undo.RegisterCompleteObjectUndo(new Object[] { map, map.gameObject }, $"Tile+Painter: {description}");
        }

        /// <summary>
        /// Is the mouse over the Scene View and
        /// position OK and Current Tool requires SceneView activity?
        /// </summary>
        private static bool MouseOverSceneView(TilePlusPainterWindow win)
        {
            var over        = EditorWindow.mouseOverWindow;
            var currentTool = win.CurrentTool;
            return over != null && over.GetType() == typeof(SceneView) 
                                && s_CurrentMouseGridPosition != TilePlusBase.ImpossibleGridPosition
                                && currentTool != TpPainterTool.Help && currentTool != TpPainterTool.Settings && currentTool != TpPainterTool.None;
        }


        private static bool PositionAlignedWithSgrid(Vector3Int position)
        {
            var size= Config.PainterFabAuthoringChunkSize;
            var relativePosition = position - Config.FabAuthWorldOrigin;
            return relativePosition.x % size == 0 && relativePosition.y % size == 0;
        }
        
        
        private static Vector3Int AlignToGrid(Vector3Int position)
        {
            var relPos = position - Config.FabAuthWorldOrigin;
            var size   = Config.PainterFabAuthoringChunkSize;
            
            var diffX = relPos.x % size;
            var diffY = relPos.y % size;

            return new Vector3Int(relPos.x - diffX, relPos.y - diffY, position.z);
        }
        
       
        
        
        
        #endregion

        #region sceneview
        
        /// <summary>
        /// SceneView delegate
        /// </summary>
        /// <param name="sceneView">The scene view.</param>
        private static void OnSceneViewSceneGui(SceneView sceneView)
        {
            IgnoreNextHierarchyChange = false;
            var win = TilePlusPainterWindow.RawInstance;
            if(win == null || !win.GuiInitialized)
                return;
            
            var evt            = Event.current;
            var currentEvtType = evt.type;

            
            
            var moveSeqState   = win.TpPainterMoveSequenceState;
            var paintTarget    = win.TilemapPaintTarget;
            var targetTileData = win.TileTarget;
            var currentTool    = win.CurrentTool;

            var noPaint = false;
            if (paintTarget is { Valid: true })
            {
                var (noPaintLocked, (allowPrefabEditing, _, _, _)) =
                    TpLibEditor.NoPaint(paintTarget.TargetTilemap);
                noPaint  = noPaintLocked && !allowPrefabEditing;
            }
            
            //are we in the correct scene view for a preview?
            if(MouseOverSceneView(win) && !noPaint)
                HandlePreviews(paintTarget!, targetTileData!,currentTool,moveSeqState);
            else if (s_PreviewActive)
                RemovePreview();
            
            var gridSelPanelActiveGridSelection = win.ActiveGridSelection;
            if (gridSelPanelActiveGridSelection != null && paintTarget != null)
            {
               var gridSelMap = paintTarget.TargetTilemap;
               if (gridSelMap != null)
               {
                   var bounds = gridSelPanelActiveGridSelection.m_BoundsInt;
                   var layout  = gridSelMap.layoutGrid;
                   bounds.position += layout.LocalToCell(gridSelMap.transform.localPosition);
                   TpLibEditor.TilemapMarquee(layout, bounds, Config.TpPainterMarqueeColor);
               }

            }
            
            
            //is the event appropriate? //allowed evts = mouseup/down, drag, and repaint ONLY.
            if (!s_AllowedEventsInSceneGui.Contains(currentEvtType)) 
                return;

            
            //is a valid tool selected and there's a tilemap target?
            if ((win.GlobalMode != GlobalMode.GridSelView && win.CurrentTool is TpPainterTool.None or TpPainterTool.Help or TpPainterTool.Settings)
                            || paintTarget is not { Valid: true } || paintTarget.TargetTilemap == null)
            {
                if (s_BrushOpInProgress)
                {
                    if(TpLibEditor.Informational)
                        TpLog("Cancelling brush operation from OnSceneGui");
                    s_BrushOpInProgress = false;
                }

                s_DragLockX = s_DragLockY = false;
                
                win.TpPainterMoveSequenceState = TpPainterMoveSequenceStates.None;
                s_CantPaintHereInterlock  = false;
                return;
            }

            //target info
            var targetTilemap         = paintTarget.TargetTilemap;
            var targetGridLayout      = paintTarget.TargetTilemapGridLayout;
            var gridTransform         = paintTarget.ParentGridTransform;
            if (gridTransform == null)
            {
                s_CantPaintHereInterlock  = false;
                return;
            }

            var mapName               = paintTarget.Name;
            if (string.IsNullOrEmpty(mapName))
                mapName = "Unknown";
            var controlId             = win.PainterWindowControlId;
            var globalMode            = win.GlobalMode;
            var validTilemapSelection = win.ValidTilemapSelection;

            s_CurrentPaintingTilemapHasOriginZero = targetTilemap.transform.position == Vector3.zero;
            
            //mouse position calcs
            
            s_CurrentMouseLocalPosition = (Vector3)ScreenToLocal!.Invoke(null, new object[]
                                                                               {
                                                                                   gridTransform,
                                                                                   (object) evt.mousePosition
                                                                               });
            s_LastMouseGridPosition    = s_CurrentMouseGridPosition;
            s_CurrentMouseGridPosition =  targetTilemap.LocalToCell(s_CurrentMouseLocalPosition); //  WorldToCell(s_CurrentMouseLocalPosition);
           
            var positionChanged = s_LastMouseGridPosition != TilePlusBase.ImpossibleGridPosition
                                  && s_LastMouseGridPosition != s_CurrentMouseGridPosition;
            
            var fabAuthoringMode = false;
            if (Config.PainterFabAuthoringMode && globalMode == GlobalMode.PaintingView)
            {
                if(currentTool == TpPainterTool.Paint)
                    fabAuthoringMode = targetTileData is { Valid: true, ItemVariety: TargetTileData.Variety.TileFabItem };
                else if (currentTool == TpPainterTool.Erase)
                    fabAuthoringMode = true;
            }
            
            var onGrid = fabAuthoringMode && PositionAlignedWithSgrid(s_CurrentMouseGridPosition);

            //Note that although drawing the marquees is done during a repaint, they're set up here and executed in
            //TpLibEditor.OnSceneViewSceneGui which only handles repaint.
            
            /* Handle marquee drag if TpPainterShortcuts MarqueeDragState is true and the GlobalMode is GridSel.
             * If that's true, manipulate a marquee. If it was true the last pass
             * but not this one, then the marquee size is sent to the GridSelection static class
             * (from the 2D Tilemap Editor) as a new GridSelection.
             */
            if (TpPainterShortCuts.MarqueeDragState)
            {
                var didPaint = false;
                if (currentEvtType == EventType.MouseDown)
                {
                    win.DeselectGridSelPanel();
                    
                    s_MarqueeDragActiveLastPass  = true;
                    s_MarqueeDragBounds.position = s_CurrentMouseGridPosition; //bounds position is initially the mouse position right now
                    s_MarqueeStartMousePosition  = s_CurrentMouseGridPosition;
                    s_MarqueeDragBounds.size     = Vector3Int.one;
                }
                else if (currentEvtType == EventType.MouseDrag && s_MarqueeDragActiveLastPass)// && positionChanged)
                {
                    s_MarqueeDragActiveLastPass = true;
                    var pos = new Vector3Int(Mathf.Min(s_MarqueeStartMousePosition.x, s_LastMouseGridPosition.x), Mathf.Min(s_MarqueeStartMousePosition.y, s_LastMouseGridPosition.y), 0);
                    var newSize = new Vector3Int(Mathf.Abs(s_MarqueeStartMousePosition.x - s_LastMouseGridPosition.x) + 1, Mathf.Abs(s_MarqueeStartMousePosition.y - s_LastMouseGridPosition.y) + 1, 1);
                    s_MarqueeDragBounds.size = newSize;
                    s_MarqueeDragBounds.position = pos;
                }
                else if (currentEvtType == EventType.Repaint && s_MarqueeDragActiveLastPass)
                    s_MarqueeDragActiveLastPass = true;
                else if (currentEvtType == EventType.MouseUp && s_MarqueeDragActiveLastPass)
                {
                    s_MarqueeDragActiveLastPass = false;

                    if (TpPainterShortCuts.MarqueeDragState)  //if the hotkey (default ALT+5) is still held down.
                    {
                        var pos  = new Vector3Int(s_MarqueeDragBounds.xMin,s_MarqueeDragBounds.yMin,s_MarqueeDragBounds.zMin);
                        var size = new Vector3Int(Mathf.Abs(s_MarqueeDragBounds.size.x), Mathf.Abs(s_MarqueeDragBounds.size.y), 1);
                        var sel  = new BoundsInt(pos, size);
                        if(size.x != 0 && size.y != 0)
                        {
                            win.AddGridSelection( sel);
                            
                            if (globalMode == GlobalMode.PaintingView &&
                                currentTool is TpPainterTool.Paint &&
                                ValidPaintTarget(paintTarget, targetTileData!) &&
                                targetTileData!.ItemVariety is TargetTileData.Variety.TileItem)
                            {
                                var tileToPaint = targetTileData.Tile;

                                if (tileToPaint != null && tileToPaint is not TilePlusBase { IsClone: true })
                                {
                                    didPaint = true;
                                    RegisterUndo(targetTilemap,
                                        $"T+P: Painting Tiles in GridSelection [{tileToPaint.name}] on Tilemap [{mapName}] at [{s_CurrentMouseGridPosition}] ");
                                    IgnoreNextHierarchyChange = true;
                                    var modifyTransform = targetTileData.TransformModified || targetTileData.WasPickedTile;

                                    foreach (var p in sel.allPositionsWithin)
                                    {
                                        targetTilemap.SetTile(p, tileToPaint);
                                        if (modifyTransform)
                                            targetTilemap.SetTransformMatrix(p, targetTileData.transform);
                                    }
                                }
                            }

                            Event.current.Use();
                            GUI.changed = true;

                            s_CantPaintHereInterlock = false;
                            IgnoreNextHierarchyChange = false;
                            s_BrushOpInProgress = false;
                            s_DragLockX = s_DragLockY = false;
                            GUIUtility.hotControl = didPaint ? 0 : controlId;

                        }
                    }
                    s_MarqueeDragActiveLastPass = false;
                }
                
            }
            else
                s_MarqueeDragActiveLastPass = false;

            TpLibEditor.TilemapMarquee(targetGridLayout, s_MarqueeDragActiveLastPass
                         ? s_MarqueeDragBounds
                         : new BoundsInt(s_CurrentMouseGridPosition, Vector3Int.one), Config.TpPainterMarqueeColor);

            

            if (fabAuthoringMode)
            {
                var color = Config.TpPainterMarqueeColor;
                var size  = Config.PainterFabAuthoringChunkSize;

                if (onGrid)
                    color *= Color.red;
                else
                {
                    var localAlignedPos = AlignToGrid(s_CurrentMouseGridPosition);
                    TpLibEditor.TilemapMarquee(targetGridLayout,
                                               new BoundsInt(localAlignedPos, new Vector3Int(size, size)),
                                               color);

                }
                TpLibEditor.TilemapMarquee(targetGridLayout,
                                           new BoundsInt(s_CurrentMouseGridPosition, new Vector3Int(size, size)),
                                           color);
            }
            
            //show brush position and other info if that's enabled. If in GridSel view and hot key not active then skip this.
            if (currentEvtType == EventType.Repaint && targetGridLayout != null)
            {
                if(!(globalMode == GlobalMode.GridSelView && !TpPainterShortCuts.MarqueeDragState))
                    HandleRepaint(targetTilemap, noPaint, targetTileData!, targetGridLayout, evt, currentTool, moveSeqState, globalMode, onGrid);
                return;
            }

            if (s_BrushOpInProgress)
            {
                //now handle mouse up event, marks the end of an operation.
                if (currentEvtType == EventType.MouseUp)
                {
                    s_CantPaintHereInterlock  = false;
                    IgnoreNextHierarchyChange = false;
                    s_BrushOpInProgress       = false;
                    s_DragLockX               = s_DragLockY = false;
                    GUIUtility.hotControl     = 0;
                    Event.current.Use();
                    GUI.changed = true;
                    return;
                }


                //handle mouse drag for repeat paint/erase ops while mouse button held down
                if ( currentEvtType == EventType.MouseDrag && currentTool is TpPainterTool.Paint or TpPainterTool.Erase)
                {
                    if (!positionChanged || !HandleDrag(evt))
                    {
                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                        GUI.changed = true;
                        s_CantPaintHereInterlock  = false;
                        return;
                    }
                }
            }

            //now handle the various painting modes
            if (currentTool != TpPainterTool.Move)
                win.TpPainterMoveSequenceState = TpPainterMoveSequenceStates.None;

            if (noPaint || s_MarqueeDragActiveLastPass)
            {
                GUIUtility.hotControl = controlId;
                Event.current.Use();
                s_CantPaintHereInterlock  = false;
                return;
            }

            //this allows right-drag within the scene window.
            if (evt.isMouse && evt.button != 0)
                return;
            
            switch (currentTool)
            {
                case TpPainterTool.Paint when ValidPaintTarget(paintTarget,targetTileData!):
                {
                    var doOvrChecks = true;
                    //if this is not a Tile or Tile subclass, check for a plugin and if there is one, does it tell us to ignore overwrites?
                    //this is useful for Rule tiles where you don't want to make these tests as it interferes with continuous painting.
                    if(targetTileData is {IsTileBase: true} && TpPreviewUtility.TryGetPlugin(targetTileData.TileType, out var plug))
                    {
                        if (plug != null && plug.m_IgnoreOverwriteChecks)
                            doOvrChecks = false;
                    }
                    
                    if (doOvrChecks && !AllowOverwriteOrIgnoreMap && TilePlusConfig.instance.NoOverwriteFromPalette /*&& targetTilemap != null*/ &&
                        targetTilemap.HasTile(s_CurrentMouseGridPosition))
                    {
                        if (TpLibEditor.Warnings)
                            TpLogWarning($"*** skipping location {s_CurrentMouseGridPosition.ToString()} due to overwrite of tile.");
                        break;
                    }

                    if (targetTileData!.ItemVariety is TargetTileData.Variety.TileItem)
                    {
                        //for TPT tiles we can restrict where they are painted.
                        var tileToPaint = targetTileData.Tile;
                        if (targetTileData is { IsTilePlusBase: true } && targetTileData.Tile != null )
                        {
                            if (AnyRestrictionsForThisTile((ITilePlus)targetTileData.Tile, mapName!))
                            {
                                s_CantPaintHereInterlock = true;
                                GUIUtility.hotControl = controlId;
                                Event.current.Use();
                                break;
                            }
                            else
                                s_CantPaintHereInterlock = false;
                            
                        }
                        else
                            s_CantPaintHereInterlock = false;

                        
                        if (evt.alt && !TpPainterShortCuts.MarqueeDragState && targetTileData.IsTile && tileToPaint is Tile t && t.gameObject != null)
                        {
                            s_BrushOpInProgress = true;
                            RegisterUndo(targetTilemap, $"T+P: Painting Prefab [{t.gameObject.name}] on Tilemap [{mapName}] at [{s_CurrentMouseGridPosition}] ");
                            GUIUtility.hotControl = controlId;

                            IgnoreNextHierarchyChange = true;
                            
                            var pos = targetTilemap.GetCellCenterWorld(s_CurrentMouseGridPosition);
                            var go = PrefabUtility.InstantiatePrefab(t.gameObject, targetTilemap.transform) as GameObject;
                            if (go != null)
                            {
                                go.transform.position = pos; //todo here would be a great place for z offset?
                                go.layer              = targetTilemap.gameObject.layer;
                            }

                            Event.current.Use();
                            GUI.changed = true;
                            
                        }
                        
                        else if(GridSelection.active 
                                && GridSelection.target.activeSelf
                                && GridSelection.target.TryGetComponent<Tilemap>(out var testmap) 
                                && testmap == targetTilemap
                                && tileToPaint != null 
                                && tileToPaint is not TilePlusBase {IsClone:true} 
                                && GridSelection.position.Contains(s_CurrentMouseGridPosition) )
                        {
                            s_BrushOpInProgress = true;
                            GUIUtility.hotControl = controlId;
                            RegisterUndo(targetTilemap, $"T+P: Painting Tiles in GridSelection [{tileToPaint.name}] on Tilemap [{mapName}] at [{s_CurrentMouseGridPosition}] ");
                            win.AddGridSelection(GridSelection.position,true);

                            var modifyTransform = targetTileData.TransformModified || targetTileData.WasPickedTile;

                            foreach (var pos in GridSelection.position.allPositionsWithin)
                            {
                                targetTilemap.SetTile(pos, tileToPaint);
                                if(modifyTransform)
                                    targetTilemap.SetTransformMatrix(pos, targetTileData.transform);
                            }

                            //GridSelection.Clear();
                            
                            Event.current.Use();
                            GUI.changed = true;

                        }
                        else if(tileToPaint != null) 
                        {
                            s_BrushOpInProgress = true;
                            RegisterUndo(targetTilemap, $"T+P: Painting Tile [{tileToPaint.name}] on Tilemap [{mapName}] at [{s_CurrentMouseGridPosition}] ");
                            GUIUtility.hotControl = controlId;

                            if (tileToPaint is TilePlusBase { IsClone: true } tpb)
                                CopyAndPasteTile(targetTilemap, tpb, s_CurrentMouseGridPosition);
                            else
                                targetTilemap.SetTile(s_CurrentMouseGridPosition, targetTileData.Tile);
                            
                            if(targetTileData.TransformModified || targetTileData.WasPickedTile)
                                targetTilemap.SetTransformMatrix(s_CurrentMouseGridPosition, targetTileData.transform);

                            Event.current.Use();
                            GUI.changed = true;
                        }

                    }
                    else if (targetTileData is { ItemVariety: TargetTileData.Variety.BundleItem, Valid: true })
                    {
                        var chunk = targetTileData.Bundle;
                        s_BrushOpInProgress = true;
                        GUIUtility.hotControl = controlId;

                        var pos = s_CurrentMouseGridPosition;
                        
                        RegisterUndo(targetTilemap, $"T+P: Painting Bundle [{chunk!.name}] on Tilemap [{mapName}] at [{s_CurrentMouseGridPosition}] ");


                        TileFabLib.LoadBundle(chunk,
                                              targetTilemap,
                                              pos,
                                              TpTileBundle.TilemapRotation.Zero,
                                              FabOrBundleLoadFlags.LoadPrefabs);

                        Event.current.Use();
                        GUI.changed = true;
                    }
                    else if (targetTileData.ItemVariety == TargetTileData.Variety.TileFabItem)
                    {
                        s_BrushOpInProgress = true;
                        GUIUtility.hotControl = controlId;
                        
                        //in 'fab authoring mode' ie placing things on a chunk-sized grid.
                        var snapPosition = fabAuthoringMode && !onGrid;

                        //in order for UNDO to work properly, need the tilemaps.
                        
                        var assets = targetTileData.TileFab!.m_TileAssets;
                        if (assets!.Count != 0)
                        {
                            var mapLookup = new Dictionary<string,Tilemap>(assets.Count);
                            foreach (var item in assets)
                            {
                                var foundMap = TileFabLib.FindTilemap(item);
                                if(foundMap != null)
                                    mapLookup.Add(item.m_TilemapName, foundMap);
                            }
                            if (mapLookup.Count != assets.Count)
                            {
                                GUIUtility.hotControl = controlId;
                                Event.current.Use();
                                GUI.changed = true;
                                break;
                            }
                            foreach(var kvp in mapLookup)
                                RegisterUndo(kvp.Value, $"T+P: Painting TileFab [{targetTileData.TileFab.name}] on Tilemap [{kvp.Key}] at [{s_CurrentMouseGridPosition}] ");

                            var placementPos = snapPosition
                                                   ? AlignToGrid(s_CurrentMouseGridPosition)
                                                   : s_CurrentMouseGridPosition;


                            var result = TileFabLib.LoadTileFab(targetTilemap,
                                                                targetTileData.TileFab,
                                                                placementPos,
                                                                TpTileBundle.TilemapRotation.Zero,
                                                                FabOrBundleLoadFlags.LoadPrefabs | FabOrBundleLoadFlags.NewGuids | FabOrBundleLoadFlags.NewGuids,
                                                                null,
                                                                mapLookup);
                            if (result == null)
                                TpLogError($"Loading of TileFab {targetTileData.TileFab.name} failed!");
                            else if (TpLibEditor.Informational)
                            {
                                TpLog($"Placed tiles from TileFab {targetTileData.TileFab}. Elapsed Time: {result.ElapsedTimeString} Results:");
                                if (result.LoadedBundles != null)
                                {
                                    foreach (var item in result.LoadedBundles)
                                        TpLog($"Asset: {item}");
                                }
                                
                            }

                            Event.current.Use();
                            GUI.changed = true;
                        }
                    }

                    break;
                }

                case TpPainterTool.Erase when validTilemapSelection:
                {
                    if (fabAuthoringMode) 
                    {
                        Event.current.Use();
                        GUI.changed = true;

                        s_BrushOpInProgress   = true;
                        GUIUtility.hotControl = controlId;

                        if (!onGrid)
                            break;

                        if (targetTileData == null || targetTileData.TileFab == null || !targetTileData.Valid)
                        {
                            TpLib.DelayedCallback(win, () =>
                                                       {
                                                           EditorUtility.DisplayDialog("Please Select....", "Please select a reference TileFab with the same Tilemaps and size as the area you want to erase.", "Continue");
                                                       }, "T+P-fab-erase-no-map-available,20");
                            

                            break;
                        }
                        //in order for UNDO to work properly, need the tilemaps.
                       
                        var assets = targetTileData.TileFab.m_TileAssets;
                        if (assets!.Count != 0)
                        {
                            var mapLookup = new Dictionary<string, Tilemap>(assets.Count);
                            foreach (var item in assets)
                            {
                                var foundMap = TileFabLib.FindTilemap(item);
                                if (foundMap != null)
                                    mapLookup.Add(item.m_TilemapName, foundMap);
                            }

                            if (mapLookup.Count != assets.Count)
                                break;

                            foreach (var kvp in mapLookup)
                                RegisterUndo(kvp.Value, $"T+P: Erasing area [{targetTileData.TileFab.name}] on Tilemap [{kvp.Key}] at [{s_CurrentMouseGridPosition}] ");

                            var placementPos = AlignToGrid(s_CurrentMouseGridPosition);

                            //area is 'largestbounds' from the asset (they're all the same for chunks)
                            var eraseBounds = targetTileData.TileFab.LargestBounds;
                            //offset it 
                            eraseBounds.position += placementPos;
                            var sz = eraseBounds.size;
                            sz.z             = 1;
                            eraseBounds.size = sz;

                            var nulls = new TileBase[sz.x * sz.y]; //these should all be null.

                            foreach (var map in mapLookup.Values)
                            {
                                map.SetTilesBlock(eraseBounds, nulls);
                                
                                var trans      = map.transform;
                                var numPrefabs = trans.childCount;
                                for(var i = 0; i < numPrefabs; i++)
                                {
                                    var t    = trans.GetChild(0);
                                    var tPos = map.WorldToCell(t.position);
                                    if(!eraseBounds.Contains(tPos))
                                        continue;
                                    Object.DestroyImmediate(t.gameObject, false);
                                }
                            }
                        }
                        break;
                    }
                    
                    var possibleTileToDelete = targetTilemap.GetTile(s_CurrentMouseGridPosition);
                    if (possibleTileToDelete == null)
                    {
                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                        GUI.changed = true;
                        break;
                    }

                    if (evt.shift || evt.control)
                    {
                        s_BrushOpInProgress = true;
                        DeleteTileActionWithConfirm(possibleTileToDelete, targetTilemap, s_CurrentMouseGridPosition, true);
                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                        GUI.changed = true;
                        break;
                    }

                    s_BrushOpInProgress = true;
                    //has to be delayed since can't open a modal dialog box from within scene view
                    TpLib.DelayedCallback(null,() => DeleteTileActionWithConfirm(possibleTileToDelete, targetTilemap, s_CurrentMouseGridPosition, false),
                                                  "Inspector Toolbar Delete Tile", 40);
                    GUIUtility.hotControl = controlId;
                    Event.current.Use();
                    GUI.changed = true;

                    break;
                }
                //ValidTilemapSelection just means that a tilemap is selected
                case TpPainterTool.Pick when validTilemapSelection:
                {
                    s_BrushOpInProgress   = true;
                    GUIUtility.hotControl = controlId;

                    //note that here we get the tile BUT the transform etc are out of the tilemap.
                    var tile = targetTilemap.GetTile(s_CurrentMouseGridPosition); 
                    if (tile != null)
                    {
                        
                        if( tile is ITilePlus { IsLocked: true } || //can't pick locked tiles.
                        evt is { control: true, shift: true }) //illegal combination
                        {
                            Event.current.Use();
                            GUI.changed = true;
                            break;
                        }
                        
                        
                        
                        if (evt.control)
                        {
                            TpLib.DelayedCallback(null,() => 
                                                          {
                                                              win.AddToHistory(tile);
                                                          },"T+P: SceneviewPick->History",50);
                        }
                        else
                        {
                            win.SetTileTarget(tile, TpPickedTileType.Tile, s_CurrentMouseGridPosition, targetTilemap, true);
                            
                            var pickIntent = Config.TpPainterPickToPaint ^ evt.shift;
                           
                            if(pickIntent && globalMode == GlobalMode.PaintingView && win.PaintingAllowed)
                                TpLib.DelayedCallback(null,() => win.ActivateToolbarButton(TpPainterTool.Paint, true), "T+V: SceneGuiModeChangeToPaint", 50);
                        }
                    }
                    else
                        win.ClearClipboard();

                    Event.current.Use();
                    GUI.changed = true;
                    break;
                }
                case TpPainterTool.Move when validTilemapSelection:
                {
                    s_BrushOpInProgress   = true;
                    GUIUtility.hotControl = controlId;

                    if (win.TpPainterMoveSequenceState == TpPainterMoveSequenceStates.Pick)
                    {
                        var tile = targetTilemap.GetTile(s_CurrentMouseGridPosition); 
                        if (tile != null)
                        {
                            //note that targetTilemap is the map when the tile was picked. The destination map could be different!
                            win.SetTileTarget(tile, TpPickedTileType.Tile,  s_CurrentMouseGridPosition, targetTilemap, true); //also send to tab bar picked tile image
                            win.TpPainterMoveSequenceState = TpPainterMoveSequenceStates.Paint;
                        }
                    }
                    else if (win.TpPainterMoveSequenceState == TpPainterMoveSequenceStates.Paint && ValidPaintTarget(paintTarget,targetTileData!))
                    {
                        if (targetTileData is {IsTilePlusBase : true})
                        {
                            if (AnyRestrictionsForThisTile(((ITilePlus)targetTileData.Tile!), mapName!))
                            {
                                s_CantPaintHereInterlock = true;
                                GUIUtility.hotControl    = controlId;
                                Event.current.Use();
                                break;
                            }
                            else
                                s_CantPaintHereInterlock = false;
                        }
                        s_CantPaintHereInterlock = false;
                        
                        //register undo for the tilemap that the picked tile came from
                        RegisterUndo(targetTileData!.SourceTilemap!, $"Move Op delete Tile [{mapName}] on Tilemap [{targetTileData.Tile!.name}] at [{s_CurrentMouseGridPosition}] ");
                        //using the tilemap and position from the saved target, clear the tile in the source tilemap and position
                        targetTileData.SourceTilemap!.SetTile(targetTileData.Position, null);

                        //now paint the tile at its destination. 
                        RegisterUndo(targetTilemap, $"Move Op placed Tile [{mapName}] on Tilemap [{targetTileData.Tile.name}] at [{s_CurrentMouseGridPosition}] ");

                        targetTilemap.SetTile(s_CurrentMouseGridPosition, targetTileData.Tile);
                        if(targetTileData.TransformModified)
                            targetTilemap.SetTransformMatrix(s_CurrentMouseGridPosition, targetTileData.transform);
                        win.ClearClipboard();
                        win.TpPainterMoveSequenceState = TpPainterMoveSequenceStates.Pick; 
                    }

                    Event.current.Use();
                    GUI.changed = true;
                    break;
                }
                case TpPainterTool.RotateCw or TpPainterTool.RotateCcw when validTilemapSelection:
                {
                    var tile = targetTilemap.GetTile(s_CurrentMouseGridPosition) as Tile;
                    if (tile != null)
                    {
                        s_BrushOpInProgress = true;
                        RegisterUndo(targetTilemap, $"Rotate Tile  [{tile.name}] on Tilemap [{mapName}] at [{s_CurrentMouseGridPosition}] ");
                        var rotationMatrix = TileUtil.RotatationMatixZ(currentTool == TpPainterTool.RotateCw
                                                                           ? -90
                                                                           : 90);
                        var transFromMap = targetTilemap.GetTransformMatrix(s_CurrentMouseGridPosition);
                        var trans        = transFromMap * rotationMatrix;
                        targetTilemap.SetTransformMatrix(s_CurrentMouseGridPosition, trans);
                    }

                    GUIUtility.hotControl = controlId;
                    Event.current.Use();
                    GUI.changed = true;
                    break;
                }

                case TpPainterTool.FlipX or TpPainterTool.FlipY when validTilemapSelection:
                {
                    var map  = paintTarget.TargetTilemap;
                    var tile = map.GetTile(s_CurrentMouseGridPosition) as Tile;
                    if (tile != null)
                    {
                        s_BrushOpInProgress = true;
                        RegisterUndo(map, $"Flip Tile [{tile.name}] on Tilemap [{map.name}] at [{s_CurrentMouseGridPosition}] ");

                        var transFromMap = map.GetTransformMatrix(s_CurrentMouseGridPosition);
                        var flipMatrix = TileUtil.ScaleMatrix(currentTool == TpPainterTool.FlipX
                                                                  ? new Vector3(-1, 1,  1)
                                                                  : new Vector3(1,  -1, 1),
                                                              Vector3Int.zero);
                        var trans = transFromMap * flipMatrix;
                        map.SetTransformMatrix(s_CurrentMouseGridPosition, trans);
                    }

                    GUIUtility.hotControl = controlId;
                    Event.current.Use();
                    GUI.changed = true;
                    break;

                }

                case TpPainterTool.ResetTransform when validTilemapSelection:
                {
                    var tile = targetTilemap.GetTile(s_CurrentMouseGridPosition) as Tile;
                    if (tile != null)
                    {
                        s_BrushOpInProgress = true;
                        RegisterUndo(targetTilemap, $"Reset Transform on Tile [{tile.name}] on Tilemap [{mapName}] at [{s_CurrentMouseGridPosition}] ");
                        var trans = Matrix4x4.TRS(Vector3Int.zero, Quaternion.identity, Vector3.one);
                        tile.transform = trans;
                        targetTilemap.SetTransformMatrix(s_CurrentMouseGridPosition, trans);
                    }

                    GUIUtility.hotControl = controlId;
                    Event.current.Use();
                    GUI.changed = true;
                    break;
                }
            }

        }

        
        /// <summary>
        /// is the paint target valid ie can we paint?
        /// </summary>
        /// <value>T/F</value>
        private static bool ValidPaintTarget(PaintTarget paintTarget, TargetTileData tileTargetData)
        {
            return s_CurrentMouseGridPosition != TilePlusBase.ImpossibleGridPosition
                   && paintTarget is { Valid: true } 
                   && tileTargetData is { Valid : true };
        }


        /// <summary>
        /// Test if a TilePlus tile has painting restrictions.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="mapName"></param>
        /// <returns>true if this tile can't paint on this map</returns>
        private static bool AnyRestrictionsForThisTile(ITilePlus tile, string mapName)
        {
            if (tile.IsLocked)
            {
                TpLog($"Can't paint Locked tiles.");
                return false;
            }
            
            var restrictions = AllowOverwriteOrIgnoreMap
                                   ? null
                                   : tile.PaintMaskList;
            //if the paintmasklist has restrictions, separate them into included and excluded maps.
            if (restrictions is { Count: > 0 })
            {
                TpLibEditor.ParsePaintMask(restrictions, ref s_IncludedMaps, ref s_ExcludedMaps);

                //see if this tilemap is paintable for this tile instance
                var noPaint        = false;
                var cleanLcMapName = mapName.ToLowerInvariant();
                if (s_ExcludedMaps.Count > 0 && s_ExcludedMaps.Contains(cleanLcMapName))
                {
                    if(TpLibEditor.Informational)
                        TpLog($"Currently selected tilemap is excluded from painting in the tile's PaintMask, Current map is {cleanLcMapName}.");
                    noPaint = true;
                }

                if (!noPaint && s_IncludedMaps.Count > 0 && !s_IncludedMaps.Contains(cleanLcMapName))
                {
                    if(TpLibEditor.Informational)
                        TpLog($"Currently Selected tilemap isn't included in the tile's PaintMask, Current map is {cleanLcMapName}.");
                    noPaint = true;
                }

                return noPaint;
            }

            return false;
        }


        /// <summary>
        /// Deletes the tile action with confirmation.
        /// </summary>
        /// <param name="tile">tile to delete</param>
        /// <param name="parentMap">tile's parent map.</param>
        /// <param name="pos">tile position.</param>
        /// <param name="skipConfirm">skip confirmation?</param>
        private static void DeleteTileActionWithConfirm(TileBase? tile, Tilemap? parentMap, Vector3Int pos, bool skipConfirm)
        {
            if (tile == null || parentMap == null || pos == TilePlusBase.ImpossibleGridPosition)
            {
                TpLogError("Invalid parameter: cancelling deletion");
                return;
            }

            if (!skipConfirm && TilePlusConfig.instance.ConfirmDeleteTile)
            {
                var doDelete = EditorUtility.DisplayDialog("Delete this tile?", "Do you really want to delete this tile?", "OK", "NOPE");
                if (!doDelete)
                    return;
            }

            if (tile is ITilePlus)
            {
                Undo.RegisterCompleteObjectUndo(new Object[] { parentMap, parentMap.gameObject }, $"T+P: delete TilePlus tile on Tilemap [{parentMap.name}] at [{s_CurrentMouseGridPosition}]");
                DeleteTile(parentMap, pos);
            }
            else
            {
                RegisterUndo(parentMap, $"Delete Tile on Tilemap [{parentMap.name}] at [{s_CurrentMouseGridPosition}] ");
                parentMap.SetTile(s_CurrentMouseGridPosition, null);
            }
        }

        #endregion
        

        
        #region dragging
        /* if shift held down AND position (CELL) changed and mode is paint or erase then let it paint OR erase again
        * if ctrl held down restrict to row or col movement (let it paint again as if it were shift held down).
        */
        //returning false ends the OnSceneViewSceneGui 
        /// <summary>
        /// Handles dragging.
        /// </summary>
        /// <param name="evt">The current event</param>
        /// <returns>false to cancel drag</returns>
        private static bool HandleDrag(Event evt)
        {
            if(evt is { shift: true, control: false }) 
                return true;
            
            //drag-lock when CTRL held down forces constant X or Y
            if (!evt.control)
                return false;
            //so, it is a mouse drag for Paint or Erase. If CTRL is held down rather than SHIFT
            //(or CTRL+SHIFT) then look at delta X or delta Y.
            //If the change is in X, then keep the old Y position, etc

            if (s_DragLockX)
                s_CurrentMouseGridPosition.x = s_ConstantXorY;
            else if (s_DragLockY)
                s_CurrentMouseGridPosition.y = s_ConstantXorY;
            else //if not drag-locked, should that state be entered? Let's see...
            {
                var deltaX = Mathf.Abs(s_CurrentMouseGridPosition.x - s_LastMouseGridPosition.x);
                var deltaY = Mathf.Abs(s_CurrentMouseGridPosition.y - s_LastMouseGridPosition.y);
                if (s_DragLockY || deltaX > 0)
                {
                    s_DragLockY                  = true;
                    s_ConstantXorY               = s_LastMouseGridPosition.y;
                    s_CurrentMouseGridPosition.y = s_LastMouseGridPosition.y;
                }
                else if (s_DragLockX || deltaY > 0)
                {
                    s_ConstantXorY               = s_LastMouseGridPosition.x;
                    s_DragLockX                  = true;
                    s_CurrentMouseGridPosition.x = s_LastMouseGridPosition.x;
                }
            }
            return true;
        }

        #endregion
        
        #region repaint

        /// <summary>
        /// Handles window repaint.
        /// </summary>
        /// <param name="target">current tilemap</param>
        /// <param name = "tileTarget" >current tile in clipboard</param>
        /// <param name="targetGridLayout">the tilemap's grid layout.</param>
        /// <param name = "evt" >the current event</param>
        /// <param name = "currentTool" >The current painter tool</param>
        /// <param name = "moveSeqState" >current move sequence state</param>
        /// <param name = "globalMode" >Painter global mode (paint/edit)</param>
        /// <param name = "noPaint" >True if this location isn't paintable</param>
        /// <param name = "onGrid" >True if Fab authoring is on and pointer is on the sGrid.</param>
        private static void HandleRepaint(Tilemap                     target,
                                          bool                        noPaint,
                                          TargetTileData              tileTarget,
                                          GridLayout               targetGridLayout,
                                          Event                       evt,
                                          TpPainterTool               currentTool,
                                          TpPainterMoveSequenceStates moveSeqState,
                                          GlobalMode                  globalMode,
                                          bool                        onGrid)
        {
            var cellSize      = targetGridLayout.cellSize;
            var mouseWorldPos = s_CurrentPaintingTilemapHasOriginZero ? target.GetCellCenterWorld(s_CurrentMouseGridPosition) : s_CurrentMouseLocalPosition;
            var hSize         = HandleUtility.GetHandleSize(new Vector3(s_CurrentMouseLocalPosition.x, s_CurrentMouseLocalPosition.y, 0.1f));
            
            if ( !Mathf.Approximately(hSize, s_LastSizeAdjustment))
            {
                var adjHsize = (1 / hSize) * 3;
                adjHsize = Mathf.Min(adjHsize, 10f);
                adjHsize = Mathf.Max(adjHsize, 1f);
                //Debug.Log($"Hs {hSize} ahs { adjHsize}");
                ReinitializeGuiContent(adjHsize );
            }

            s_LastSizeAdjustment =  hSize;

            const float fx                  = 2f;
            const float fy                  = 1.0f;
            const float factorAdj           = 0.2f;
            var         labelsPosition      = mouseWorldPos - (new Vector3(+(cellSize.x / fx), cellSize.y * fy) );
            var         positionLabelOffset = mouseWorldPos + new Vector3(-(cellSize.x / fx), cellSize.y * fy);
            var         factor              = hSize * factorAdj;
            var         hOffset             = new Vector3(factor, factor, 0);
            labelsPosition      += hOffset;
            positionLabelOffset += hOffset;
            labelsPosition.z      =  0.1f;
            positionLabelOffset.z =  0.1f;
           

            if (s_MarqueeDragActiveLastPass)
            {
                var size     = s_MarqueeDragBounds.size;
                var pos      = s_MarqueeDragBounds.position;
                var marqText = $" [XY {pos.x}:{pos.y}]\n Size:[{size.x}*{size.y}]";
                Handles.Label(labelsPosition, marqText, s_PositionTextGuiStyle);
                return;
            }
            
            if (noPaint)
            {
                Handles.Label(positionLabelOffset, "Locked/Prefab", s_PositionTextGuiStyle);
                    return;
            }

            if (globalMode == GlobalMode.PaintingView && currentTool == TpPainterTool.Paint && tileTarget is { IsTilePlusBase: true } && tileTarget.Tile != null )
               s_CantPaintHereInterlock = AnyRestrictionsForThisTile((ITilePlus)tileTarget.Tile, target.name);
            
            
            var text = globalMode != GlobalMode.GridSelView
                           ? onGrid
                                 ? $"<G>{ToolNames(currentTool)}"
                                 : ToolNames(currentTool)
                           : //allows for multiple langs
                           "Create Grid Selection";
            if (currentTool == TpPainterTool.Move && moveSeqState == TpPainterMoveSequenceStates.Pick)
                text = $"{text}-Picking";

            //note that the test for HasGameObject is never true for TileBase tiles eg Rule Tiles.
            if (currentTool == TpPainterTool.Paint && Event.current.alt && tileTarget is { Valid: true, HasGameObject: true })
                text = $"{text}-Only Prefab";

            if (TilePlusConfig.instance.ShowBrushPosition)
                text = $"{text} [{s_CurrentMouseGridPosition.x.ToString()}:{s_CurrentMouseGridPosition.y.ToString()}]";
            
            Handles.Label(positionLabelOffset, text, onGrid ? s_PositionTextAltGuiStyle : s_PositionTextGuiStyle);

            // ReSharper disable once InvertIf
            if (tileTarget is {Valid : true}
                && ((currentTool == TpPainterTool.Move && moveSeqState == TpPainterMoveSequenceStates.Paint)
                    || currentTool == TpPainterTool.Paint))
            {
                if (s_CantPaintHereInterlock)
                    Handles.Label(labelsPosition+hOffset, "Can't paint here", s_PositionTextGuiStyle);
                else if (s_PreviewIsPlaceholderTile)
                    Handles.Label(labelsPosition+hOffset, "Tile sprite is hidden", s_PositionTextGuiStyle);

                if (target.HasTile(s_CurrentMouseGridPosition))
                {
                    //if this is not a Tile or Tile subclass, check for a plugin and if there is one, does it tell us to ignore overwrites?
                    //if so then don't show the any of the below text.
                    if(tileTarget is { IsTile : true } || //if it's a tile then no further checks needed. Note this includes TilePlusBase classes since they're Tile subclasses
                        (TpPreviewUtility.TryGetPlugin(tileTarget.TileType, out var plug) && plug !=null && !plug.m_IgnoreOverwriteChecks))  
                        //if there's a plugin for this TileBase-derived tile and the plugin's IgnoreOverwriteChecks field is false then show the text.
                    {
                        var overwriteLabelOffset = labelsPosition;
                        if (s_PreviewIsPlaceholderTile)
                            overwriteLabelOffset.y -= 0.25f;
                        var noOvr = TilePlusConfig.instance.NoOverwriteFromPalette;
                        Handles.Label(overwriteLabelOffset, noOvr ^ AllowOverwriteOrIgnoreMap
                                                                ? "Protected"
                                                                : "Will Overwrite", s_PositionTextGuiStyle);
                    }
                }

                if (!AllowOverwriteOrIgnoreMap)
                    return;
                var center = new BoundsInt(s_CurrentMouseGridPosition, Vector3Int.one).center + target.transform.position;
                var len    = Vector2.one / 2f;
                var end    = new Vector2(center.x - len.x, center.y - len.y);
                var start  = new Vector2(center.x + len.x, center.y + len.y);
                TpLibEditor.TilemapLine(start, end, Color.black, 0);
            }

          
            
            
            if (globalMode != GlobalMode.GridSelView && currentTool == TpPainterTool.Pick)
            {
                /* if the pin button is 'true' then pick->clipboard->paint.
                 * if the pin button is 'false' then pick->clipboard.
                 * if the SHIFT key is depressed then this intent is reversed. 
                 * if the CTRL key is depressed then pick->History. Pin button ignored.
                 * if SHIFT and CTRL both depressed then no pick action is to History..
                 * -- below, pickIntent is false for pick->clipboard, true for pick->clipboard->paint.
                 */
                if (evt.control)
                    Handles.Label(/*mouseWorldPos - */labelsPosition, "Pick=>History",s_PickMsgGuiStyle);
                else
                {
                    if (globalMode == GlobalMode.PaintingView)
                    {
                        var pickIntent = Config.TpPainterPickToPaint ^ evt.shift;
                        Handles.Label(/*mouseWorldPos - */labelsPosition,
                                      pickIntent
                                          ? "Pick=>Clipboard=>Paint"
                                          : "Pick=>Clipboard",
                                      s_PickMsgGuiStyle);
                    }
                    else
                        Handles.Label(/*mouseWorldPos -*/ labelsPosition, "Pick=>Clipboard", s_PickMsgGuiStyle);
                }
            }
        }

        #endregion
        
        
        #region preview
        /// <summary>
        /// Used when rotating or flipping the tile when preview is active.
        /// </summary>
        internal static void RefreshPreview()
        {
            if(s_PreviewActive)
                RemovePreview();
        }
        
        /// <summary>
        /// Handles previews.
        /// </summary>
        private static void HandlePreviews(PaintTarget tilemapPaintTarget,  TargetTileData targetTile, TpPainterTool currentTool, TpPainterMoveSequenceStates moveState)
        {
            var validPaintTarget = ValidPaintTarget(tilemapPaintTarget, targetTile);
            if (!s_PreviewActive && validPaintTarget && (currentTool == TpPainterTool.Paint || (currentTool == TpPainterTool.Move && moveState == TpPainterMoveSequenceStates.Paint)))
            {
                if (targetTile.ItemVariety == TargetTileData.Variety.TileItem)
                {
                    //if (pt.Tile is ITilePlus itpTile)
                    if(targetTile is {IsTilePlusBase : true} && targetTile.Tile != null) //is TilePlusBase subclass
                    {
                        //if the tile sprite isn't being shown, use the placeholderTile
                        var usingPh = ((ITilePlus)targetTile.Tile).TileSpriteClear is SpriteClearMode.ClearInSceneView
                                      or SpriteClearMode.ClearInSceneViewAndOnStart;
                        var t = targetTile.Tile as Tile;
                        if (t != null)
                        {
                            if (usingPh)
                            {
                                ShowPreview(tilemapPaintTarget.TargetTilemap,
                                            s_CurrentMouseGridPosition,
                                            s_PlaceholderTile, Matrix4x4.identity, s_PlaceholderTile.color);    
                            }
                            else
                            {
                                ShowPreview(tilemapPaintTarget.TargetTilemap,
                                            s_CurrentMouseGridPosition,
                                            t,
                                            targetTile.transform,
                                            t.color);
                                
                            }
                            
                        }
                    }
                    else if (targetTile is {IsTile:true} && targetTile.Tile != null) //is Tile subclass but NOT TilePlusBase
                    {
                        var t = targetTile.Tile as Tile;
                        if (t != null)
                            ShowPreview(tilemapPaintTarget.TargetTilemap, s_CurrentMouseGridPosition, targetTile.Tile, targetTile.transform, t.color);
                    }
                    else if(targetTile.Tile != null) //is a TileBase tile, needs special handling
                            ShowPreviewSpecial(tilemapPaintTarget.TargetTilemap, s_CurrentMouseGridPosition, targetTile.Tile);        
                    
                }
                
                else if (targetTile.ItemVariety == TargetTileData.Variety.BundleItem)
                {
                    if(!TileFabLib.PreviewActive && tilemapPaintTarget.TargetTilemap != null && targetTile.Bundle != null)
                    {
                        s_PreviewActive          = true;
                        s_PreviewIsBundle         = true;
                        s_CurrentPreviewPosition = s_CurrentMouseGridPosition;
                        s_CurrentPreviewTilemap  = tilemapPaintTarget.TargetTilemap;
                        TileFabLib.PreviewImportedTilemap(tilemapPaintTarget.TargetTilemap, s_CurrentMouseGridPosition, targetTile.Bundle);
                    }
                }
                
                else if (targetTile.ItemVariety == TargetTileData.Variety.TileFabItem)
                {
                    if (!TileFabLib.PreviewActive && tilemapPaintTarget.TargetTilemap != null && targetTile.TileFab != null)
                    {
                            s_PreviewActive          = true;
                            s_PreviewIsTileFab       = true;
                            s_CurrentPreviewPosition = s_CurrentMouseGridPosition;
                            s_CurrentPreviewTilemap = tilemapPaintTarget.TargetTilemap;
                            TileFabLib.PreviewImportedTileFab(tilemapPaintTarget.TargetTilemap,
                                                              targetTile.TileFab,
                                                              s_CurrentMouseGridPosition);
                    }
                }
            }

            if (s_PreviewActive && !(currentTool == TpPainterTool.Paint || (currentTool == TpPainterTool.Move && moveState == TpPainterMoveSequenceStates.Paint)))
                RemovePreview();
            if (s_PreviewActive && s_CurrentMouseGridPosition != s_CurrentPreviewPosition) 
                RemovePreview();
        }

        /// <summary>
        /// Shows a tile preview.
        /// </summary>
        /// <param name="map">target tilemap.</param>
        /// <param name="pos">target position.</param>
        /// <param name="tile">tile to preview</param>
        /// <param name="transform">transform.</param>
        /// <param name="color">color.</param>
        private static void ShowPreview(Tilemap? map, Vector3Int pos, TileBase tile, Matrix4x4 transform, Color color) 
        {
            if (s_PreviewActive)
            {
                Debug.LogError("Can't show preview when already active");
                return;
            }

            if (map == null)
            {
                Debug.LogError("Null map reference to ShowPreview!!");
                return;
            }

            s_PreviewActive            = true;
            s_PreviewIsPlaceholderTile = tile == s_PlaceholderTile;
            s_CurrentPreviewPosition   = pos;
            s_CurrentPreviewTilemap    = map;
            map.SetEditorPreviewTile(pos, tile);
            map.SetEditorPreviewTransformMatrix(pos, transform);
            map.SetEditorPreviewColor(pos, color);
        }

        private static void ShowPreviewSpecial(Tilemap? map, Vector3Int pos, TileBase tile)
        {
            if (map == null)
            {
                Debug.LogError("Null map reference to ShowPreviewSpecial!!");
                return;
            }
            
            if (!TpPreviewUtility.TryGetPlugin(tile, out var plug) || plug == null)
                return;
            
            if (s_PreviewActive)
            {
                Debug.LogError("Can't show preview when already active");
                return;
            }

            s_PreviewActive            = true;
            s_PreviewIsPlaceholderTile = tile == s_PlaceholderTile;
            s_CurrentPreviewPosition   = pos;
            s_CurrentPreviewTilemap    = map;
            
            map.SetEditorPreviewTile(pos, tile);
            map.SetEditorPreviewTransformMatrix(pos, plug.GetTransformForTile(tile));
            map.SetEditorPreviewColor(pos, plug.GetColorForTile(tile));
        }
        
        
        /// <summary>
        /// Removes the preview.
        /// </summary>
        internal static void RemovePreview()
        {
            if (!s_PreviewActive)
                return;
                
            if(s_CurrentPreviewTilemap == null)
            {
                //don't need to spam this
                //Debug.LogError($"Can't clear preview if not active [map ref valid?:{s_CurrentPreviewTilemap != null}]");
                return;
            }

            s_PreviewActive = false;
            if (s_PreviewIsBundle || s_PreviewIsTileFab)
            {
                TileFabLib.ClearPreview();
                s_PreviewIsBundle   = false;
                s_PreviewIsTileFab = false;
            }
            else
            {

                s_CurrentPreviewTilemap.SetEditorPreviewTile(s_CurrentPreviewPosition, null);
                s_CurrentPreviewTilemap.SetEditorPreviewTransformMatrix(s_CurrentPreviewPosition, Matrix4x4.identity);
                s_CurrentPreviewTilemap.SetEditorPreviewColor(s_CurrentPreviewPosition, Color.white);
            }

            s_CurrentPreviewTilemap = null;
        }
        
        #endregion
        
        #region content
        private static readonly Dictionary<int, string[]> s_ToolNames = new()
                                                                        {
                                                                            {
                                                                                (int)SystemLanguage.English, new[] // strings for English
                                                                                                             {
                                                                                                                 "None", "Paint", "Erase", "Pick", "Move", "RotCW", "RotCCW", "FlipX", "FlipY", "Rst Transform", "Help", "Settings"
                                                                                                             }
                                                                            }
                                                                        };


        private static string ToolNames(TpPainterTool tool)
        {
            var index = (int)tool;
            var lang  = (int)Application.systemLanguage;
            if (!s_ToolNames.TryGetValue(lang, out var arr))
                return "?????";
            
            return index >= arr.Length
                       ? "?????"
                       : arr[index];

        }
        
        
        #endregion
        
    }
}
