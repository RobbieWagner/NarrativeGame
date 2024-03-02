// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 11-18-2022
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 01-13-23
// ***********************************************************************
// <copyright file="TpPainterShortCuts.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>Shortcut handlers for Tile+Painter</summary>
// ***********************************************************************
#nullable disable
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
// ReSharper disable AnnotateNotNullTypeMember
// ReSharper disable PossibleNullReferenceException
// ReSharper disable AnnotateCanBeNullParameter
// ReSharper disable AnnotateCanBeNullTypeMember


/*
 * IMPORTANT NOTE: CHANGING ANY OF THE BINDING PATHS REQUIRE CORRESPONDING CHANGES ELSEWHERE FOR
 * LOOKUPS TO WORK CORRECTLY!!!!!
 */


namespace TilePlus.Editor.Painter
{
    /// <summary>
    /// This static class tracks the activity of the TilePlusPainterWindow and enables/disables shortcut buttons.
    /// It also is the tooltip provider for the Global mode and PainterTool action buttons.
    /// </summary>
    [InitializeOnLoad]
    internal static class TpPainterShortCuts
    {
        private static readonly Dictionary<TpPainterTool, string> s_ToolToShortCutId;
        
        /// <summary>
        /// Default Ctor called by InitializeOnLoad.
        /// </summary>
        static TpPainterShortCuts()
        {
            
            EditorApplication.update += PainterActiveCheck;
            s_ToolToShortCutId = new Dictionary<TpPainterTool, string>(8)
                               {
                                   { TpPainterTool.Paint, "TilePlus/Painter:Paint" },
                                   { TpPainterTool.Move, "TilePlus/Painter:Move" },
                                   { TpPainterTool.Erase, "TilePlus/Painter:Erase" },
                                   { TpPainterTool.Pick, "TilePlus/Painter:Pick" },
                                   { TpPainterTool.RotateCw, "TilePlus/Painter:RotateCW" },
                                   { TpPainterTool.RotateCcw, "TilePlus/Painter:RotateCCW" },
                                   { TpPainterTool.FlipX, "TilePlus/Painter:Flip X" },
                                   { TpPainterTool.FlipY, "TilePlus/Painter:Flip Y" },
                                   { TpPainterTool.ResetTransform, "TilePlus/Painter:Reset Transform" },
                                   { TpPainterTool.None, "TilePlus/Painter:Deactivate" }
                               };
           
        }

        /// <summary>
        /// This is called repeatedly by EditorApplication.update
        /// and tests the Painter state to see if the shortcuts should
        /// be enabled or not.
        /// </summary>
        private static void PainterActiveCheck()
        {
            var ri = TilePlusPainterWindow.RawInstance;
            if (ri == null)
            {
                TpPainterActive = false;
                return;
            }
            if(!ri.GuiInitialized)
                return;
            TpPainterActive = ri.IsActive && ri.CurrentToolHasTilemapEffect;
        }

        /// <summary>
        /// Get a formatted tooltip string for a tool. Accomodates user changing shortcut bindings
        /// </summary>
        /// <param name="tool">TpPainterTool</param>
        /// <returns>formatted string</returns>
        internal static string GetToolTipForTool(TpPainterTool tool)
        {
            if (!s_ToolToShortCutId.TryGetValue(tool, out var s))
                return "(Shortcut not found!)";
            var binding = ShortcutManager.instance.GetShortcutBinding(s);
            return $"({binding.ToString()})";
        }

        
        
        /// <summary>
        /// Get a 2-letter abbreviated tooltip for the Painter action buttons
        /// </summary>
        /// <param name="tool">value of TpPainterTool enum</param>
        /// <returns>string</returns>
        /// <remarks>Note that the 2-letter approach works for Ctrl+X or ALT+X but if someone uses Ctrl+Alt+X this fails.</remarks>
        internal static string GetAbbreviatedToolTipForTool(TpPainterTool tool)
        {
            var tip           = GetToolTipForTool(tool);
            var plusSignIndex = tip.IndexOf('+');
            if (plusSignIndex == -1)
                return tip.Substring(1,1); //note that '1' is used vs '0' that's because the first char is a (
            return $"{tip.Substring(1, 1)}{tip.Substring(plusSignIndex + 1, 1)}";
        }
        
        
        /// <summary>
        /// Get a tooltip for the T+P mode buttons
        /// </summary>
        /// <returns></returns>
        internal static string GetModeButtonTooltip()
        {
            var binding = ShortcutManager.instance.GetShortcutBinding("TilePlus/Painter:Toggle Mode");
            return $"({binding.ToString()} to toggle mode)";
        }
        
        //NB - this isn't really used anymore.
        /// <summary>
        /// Get abbreviated tooltip for Mode buttons. 
        /// </summary>
        /// <returns>string</returns>
        /// <remarks>Note that the 2-letter approach works for Ctrl+X or ALT+X but if someone uses Ctrl+Alt+X this fails.</remarks>
        internal static string GetModeButtonAbbreviatedTooltip()
        {
            var tip           = GetModeButtonTooltip();
            var plusSignIndex = tip.IndexOf('+');
            if (plusSignIndex == -1)
                return tip.Substring(1,1);
            return $"{tip.Substring(1, 1)}{tip.Substring(plusSignIndex + 1, 1)}";

        }

        /// <summary>
        /// Add one or more assets from the Project folder to the History stack
        /// </summary>
        [MenuItem("Assets/Add To Painter History",false,10000)]
        internal static void CopyToHistory()
        {
            var objects = Selection.objects;
            
            if (!EditorWindow.HasOpenInstances<TilePlusPainterWindow>())    
                TilePlusPainterWindow.ShowWindow();

            TpConditionalTasks.ConditionalDelayedCallback(TilePlusPainterWindow.RawInstance,
                () =>
                {

                    foreach (var obj in objects)
                    {
                        if (obj is TileBase tb)
                            TilePlusPainterWindow.instance.AddToHistory(tb);
                    }
                }, (_) => TilePlusPainterWindow.RawInstance.GuiInitialized,
                "Copy to history safety check");
        }
        
        /// <summary>
        /// Validator for CopyToHistory
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/Add To Painter History",true,10000)]
        internal static bool CopyToHistoryValidator()
        {
            var objects = Selection.objects;
            foreach (var obj in objects)
            {
                if (obj is not TileBase)
                    return false;
                if (obj is ITilePlus { IsAsset: false })
                    return false;
            }

            return true;
        }
        
        internal static bool MarqueeDragState { get; private set; }
        
        //NOTE: if this shortcut id changes then the MarqueeDragTooltip property needs to be changed too.
        //Also see TpPainterTab bar to update this shortcut ref as well.
        [ClutchShortcut("TilePlus/Painter/MarqueeDrag [C]", KeyCode.Alpha5, ShortcutModifiers.Alt)]
        internal static void MarqueeDrag(ShortcutArguments args)
        {
            MarqueeDragState = args.stage switch
                                     {
                                         ShortcutStage.Begin => true,
                                         ShortcutStage.End   => false,
                                         _                   => false
                                     };
        }
        
        /// <summary>
        /// Get a tooltip for the Marquee-Drag function
        /// </summary>
        /// <returns></returns>
        internal static string MarqueeDragTooltip => ShortcutManager.instance.GetShortcutBinding("TilePlus/Painter/MarqueeDrag [C]").ToString();
           
        
        
        /// <summary>
        /// Is the painter active?
        /// </summary>
        internal static bool TpPainterActive { get; private set; }

        /// <summary>
        /// if Painter window not open, ask. 
        /// </summary>
        /// <returns>null if user declined to open window or window not found</returns>
        private static TilePlusPainterWindow PainterOpenCheck()
        {
            var p = TilePlusPainterWindow.RawInstance;
            if (p != null)
                return p;
            if (!Guidance())
                return null;
            TilePlusPainterWindow.ShowWindow();
            return TilePlusPainterWindow.instance;
        }
        

        [Shortcut("TilePlus/Painter:Toggle Mode" , KeyCode.E, ShortcutModifiers.Alt)]
        internal static void TogglePaintEditMode(ShortcutArguments args)
        {
            var instance = PainterOpenCheck();
            if(instance == null)
                return;
           
            //toggle mode
            var current = (int)instance.GlobalMode;
            var count   = Enum.GetValues(typeof(GlobalMode)).Length;
            current = (current + 1) % count;
            instance.TabBar.ActivateModeBarButton((GlobalMode)current, true);
            //instance.TabBar.ActivateModeBarButton(instance.GlobalMode == GlobalMode.EditingView ? GlobalMode.PaintingView : GlobalMode.EditingView ,true);

        }
        
        /// <summary>
        /// Activate paint tool
        /// </summary>
        [Shortcut("TilePlus/Painter:Paint",  KeyCode.B, ShortcutModifiers.Alt)]
        internal static void ActivatePaintbrush()
        {
            var p = PainterOpenCheck();
            if(p == null)
                return;

            if(p.GlobalMode == GlobalMode.EditingView)
                p.TabBar.ActivateModeBarButton(GlobalMode.PaintingView,true);
            p.TabBar.ActivateToolbarButton(TpPainterTool.Paint,true);
        }

        /// <summary>
        /// Activate Move tool
        /// </summary>
        [Shortcut("TilePlus/Painter:Move",KeyCode.M, ShortcutModifiers.Alt)]
        internal static void ActivateMoveTool()
        {
            ActivateNormalTool(TpPainterTool.Move);
        }

        /// <summary>
        /// Activate Erase tool
        /// </summary>

        [Shortcut("TilePlus/Painter:Erase", KeyCode.D, ShortcutModifiers.Alt)]
        internal static void ActivateEraseTool()
        {
            ActivateNormalTool(TpPainterTool.Erase);
        }

        /// <summary>
        /// Activate Pick tool
        /// </summary>
        [Shortcut("TilePlus/Painter:Pick",  KeyCode.I, ShortcutModifiers.Alt)]
        internal static void ActivatePickTool()
        {
            ActivateNormalTool(TpPainterTool.Pick);
        }

        /// <summary>
        /// Activate RotateCW tool
        /// </summary>
        [Shortcut("TilePlus/Painter:RotateCW",  KeyCode.R, ShortcutModifiers.Alt)]
        internal static void ActivateRotateCwTool()
        {
            ActivateNormalTool(TpPainterTool.RotateCw, RotatePaintingTileCw);
        }
        
        /// <summary>
        /// Activate Rotate CCW tool
        /// </summary>
        [Shortcut("TilePlus/Painter:RotateCCW", KeyCode.T, ShortcutModifiers.Alt)]
        internal static void ActivateRotateCcwTool()
        {
            ActivateNormalTool(TpPainterTool.RotateCcw, RotatePaintingTileCcw);
        }
        
        /// <summary>
        /// Activate FlipX tool
        /// </summary>
        [Shortcut("TilePlus/Painter:Flip X", KeyCode.X, ShortcutModifiers.Alt)]
        internal static void ActivateFlipXTool()
        {
            ActivateNormalTool(TpPainterTool.FlipX, FlipXPaintingTile);
        }

        /// <summary>
        /// Activate FlipY tool
        /// </summary>
        [Shortcut("TilePlus/Painter:Flip Y",  KeyCode.C, ShortcutModifiers.Alt)]
        internal static void ActivateFlipYTool()
        {
            ActivateNormalTool(TpPainterTool.FlipY, FlipYPaintingTile);
        }
        
        /// <summary>
        /// Activate Reset Transform tool
        /// </summary>
        [Shortcut("TilePlus/Painter:Reset Transform",  KeyCode.Z, ShortcutModifiers.Alt)]
        internal static void ActivateResetTransformTool()
        {
            ActivateNormalTool(TpPainterTool.ResetTransform, ResetTransformForPaintingTile);
        }
        
        /// <summary>
        /// Activate Deactivate tool
        /// </summary>
        [Shortcut("TilePlus/Painter:Deactivate",  KeyCode.O, ShortcutModifiers.Alt)]
        internal static void ActivateNullTool()
        {
            ActivateNormalTool(TpPainterTool.None);
        }

       
        
        
        /// <summary>
        /// perform Apply Transform
        /// </summary>
        [Shortcut("TilePlus/Painter:Apply Transform", KeyCode.V, ShortcutModifiers.Alt)]
        internal static void ApplyCustomTransform()
        {
            if(!TpPainterActive)
                return;
            var win = TilePlusPainterWindow.instance;
            var tgt = win.TileTarget;
            if (!tgt.Valid)
            {
                TpLib.DelayedCallback(null,() =>
                                              {
                                                  EditorUtility.DisplayDialog("Sorry!", "Need something in the clipboard in order to use this!", "Move on...");

                                              },"T+TE: cant-delete");
                return;
            }

            var transforms = TpPainterScanners.PainterTransforms;
            if (transforms == null)
            {
                TpPainterScanners.TransformAssetScanner();
                transforms = TpPainterScanners.PainterTransforms;
                if (transforms == null)
                {
                    TpLib.DelayedCallback(null,() =>
                                                  {
                                                      EditorUtility.DisplayDialog("Sorry!", "Custom transforms asset not found!", "Move on...");

                                                  },"T+Shortcuts: cant-find-transforms");
                    return;
                }
                    
            }

            var customTransformsCount = TpPainterScanners.PainterTransformsCount;
            var selection             = TpPainterScanners.PainterTransforms.m_ActiveIndex;
            if (customTransformsCount >= 0 && selection >= 0 && selection < customTransformsCount)
            {
                if(TpLibEditor.Informational)
                    TpLib.TpLog($"Apply transform selection {selection} ");
                tgt.Apply(TpPainterScanners.PainterTransforms.m_PTransformsList[selection].m_Matrix);
                TpPainterSceneView.RefreshPreview();

            }
            else
            {
                TpLib.DelayedCallback(null,() =>
                                              {
                                                  EditorUtility.DisplayDialog("Sorry!", "No custom transforms or no selection", "Move on...");

                                              },"T+Shortcuts: cant-apply-transform");
            }
        }
        
        
        private static void RotatePaintingTileCw()
        {
            var p = PainterOpenCheck();
            if(p == null)
                return;
            var tgt = p.TileTarget;
            if (!tgt.Valid)
                return;
            tgt.Rotate();
            TpPainterSceneView.RefreshPreview();
        }

        private static void RotatePaintingTileCcw()
        {
            var p = PainterOpenCheck();
            if(p == null)
                return;
            var tgt = p.TileTarget;
            if (!tgt.Valid)
                return;
            tgt.Rotate(true);
            TpPainterSceneView.RefreshPreview();
        }


        private static void FlipXPaintingTile()
        {
            var p = PainterOpenCheck();
            if(p == null)
                return;
            var tgt = p.TileTarget;
            if (!tgt.Valid)
                return;
            tgt.Flip(true);
            TpPainterSceneView.RefreshPreview();
        }

        private static void FlipYPaintingTile()
        {
            var p = PainterOpenCheck();
            if(p == null)
                return;
            var tgt = p.TileTarget;
            if (!tgt.Valid)
                return;
            tgt.Flip();
            TpPainterSceneView.RefreshPreview();
        }

        private static void ResetTransformForPaintingTile()
        {
            var p = PainterOpenCheck();
            if(p == null)
                return;
            var tgt = p.TileTarget;
            if (!tgt.Valid)
                return;
            tgt.Restore();
            TpPainterSceneView.RefreshPreview();
        }
        
        private static void ActivateNormalTool(TpPainterTool tool, Action previewAction = null)
        {
            var p = PainterOpenCheck();
            if(p == null)
                return;
            if (!p.ValidTilemapSelection)
                return;

            if (previewAction != null)
            {
                if (p.PreviewActive)
                    previewAction();
                else if(p.MouseOverTpPainter)
                    p.TabBar.ActivateToolbarButton(tool, true);
            }
            else
                p.TabBar.ActivateToolbarButton(tool, true);
            
        }
        
        
        
        
        private static bool Guidance()
        {
            return EditorUtility.DisplayDialog("Help me out here!",
                                               "You clicked a shortcut for the Tile+Painter but there's no Painter window open. \nDo you want to open one?\n\n(Note that you'll have to try the shortcut again after the window opens)",
                                               "YES",
                                               "NOPE");
        }

    }
}
