// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 01-01-2023
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 12-30-2022
// ***********************************************************************
// <copyright file="TpIconLib.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>Icon cache system</summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
// ReSharper disable ArrangeMethodOrOperatorBody
// ReSharper disable AnnotateCanBeNullTypeMember

// ReSharper disable MissingXmlDoc

namespace TilePlus.Editor
{
    /// <summary>
    /// Frequently used editor and TilePlus icons. Cached after first use.
    /// Doesn't use any significant memory until first used. Then each time
    /// one of the Icons is requested the cache is examined and if
    /// needed, the Icon is fetched and cached in the Dictionary for future use.
    /// This class also includes cached Sprites created from any Texture2D.
    /// </summary>
    [InitializeOnLoad]
    public static class TpIconLib
    {
        #region Ctor
        
        static TpIconLib()
        {
            //this allows TilePlus tile editors (embedded in the tile code)
            //to use this normally editor-only method. 
            TpEditorBridge.BridgeFindIcon = FindIcon;
        }
        
        #endregion
        
        #region data

        private static bool s_Initialized;
        private static bool ProSkin => EditorGUIUtility.isProSkin;
        //Icon cache
        private static Dictionary<TpIconType, Texture2D> s_IconCache;
        //Sprite cache
        private static readonly Dictionary<int, Sprite> s_SpriteCache = new();
        
        private static Dictionary<TpIconType, Func<Texture2D>> s_Lookup;
        
        #endregion
        
        #region iconCache
        private static void InitIconCache()
        {
            //size the dictionary properly
            if (Enum.GetValues(typeof(TpIconType)) is not TpIconType[] enums)
            {
                Debug.LogError("Fatal error in TpIconLib. No icons possible");
                return;
            }
            var numIcons = enums.Length;
            s_IconCache = new(numIcons);
            foreach (var e in enums)
                s_IconCache.Add(e, null);
        }
        
        private static GUISkin s_TpSkin;

        /// <summary>
        /// Get the cached TilePlus IMGUI skin.
        /// </summary>
        internal static GUISkin TpSkin
        {
            get
            {
                if (s_TpSkin != null)
                    return s_TpSkin;
                s_TpSkin = TpEditorUtilities.GetSkin();
                return s_TpSkin == null ? GUI.skin : s_TpSkin;
            }
        }
        
        private static void FindIcon(TpIconType tpIconType, out Texture2D icon)
        {
            if (!s_Initialized)
            {
                s_Initialized = true;
                InitFetchers(); //this really can't fail.
                InitIconCache();
                if (s_IconCache == null)
                {
                    icon = MinusIcon();
                    return;
                }
            }

            if (s_IconCache.TryGetValue(tpIconType, out icon) && icon != null)
                return;
            if (s_Lookup.TryGetValue(tpIconType, out var f))
            { 
               icon                   = f();
               if (icon != null)
               {
                   s_IconCache[tpIconType] = icon;
                   return;
               }
            }
            icon = MinusIcon();
            Debug.LogError($"Could not find icon {tpIconType.ToString()}");
            if(icon == null) //could still be true
                icon = new Texture2D(50, 50); //if all else fails return a blank texture to avoid a null-ref error.
        }

        /// <summary>
        /// Find an Icon for a specific TpIconType enum value
        /// </summary>
        /// <param name="tpIconType">enum value</param>
        /// <returns>Texture2D icon. If not found either a '-' icon is returned,
        /// and if that can't be found, a 50x50 tex2d is returned.</returns>
        /// <remarks>The TpIconType enum definition is in TpEditorBridge.cs</remarks>
        /// <remarks>Don't call this in a static editor class constructor, including fields
        /// initialized when loaded (eg static fields with initializers calling this) because
        /// some icons may not be available - it's a potential race condition.</remarks>
        public static Texture2D FindIcon(TpIconType tpIconType)
        {
            FindIcon(tpIconType, out var tex);
            return tex;
        }
        #endregion

        #region spriteFromTex
        
        /// <summary>
        /// clear the cache
        /// </summary>
        public static void ResetSpriteCache()
        {
            s_SpriteCache.Clear();
        }
        
        /// <summary>
        /// Create a sprite from a Texture2D. 
        /// </summary>
        /// <param name="tex">input texture</param>
        /// <returns>sprite instance</returns>
        /// <remarks>for efficiency the sprites are cached internally</remarks>
        internal static Sprite SpriteFromTexture(Texture2D tex)
        {
            if (tex == null)
            {
                Debug.LogError("SpriteFromTexture passed a null texture input!");
                return null;
            }
            if (s_SpriteCache.TryGetValue(tex.GetInstanceID(), out var sprite))
                return sprite;
           
            var texRect   = new Rect(0, 0, tex.width, tex.height);
            var newSprite =  Sprite.Create(tex, texRect, new Vector2(0.5f, 0.5f));
            s_SpriteCache.Add(newSprite.GetInstanceID(), newSprite);
            return newSprite;
        }
        
        #endregion
        
        
        #region iconFetchers

        private static void InitFetchers()
        {
            s_Lookup = new()
                       {
                           { TpIconType.UnityPaintIcon, UnityPaintIcon },
                           { TpIconType.UnityEraseIcon, UnityEraseIcon },
                           { TpIconType.UnityRotateCcwIcon, UnityRotateCcwIcon },
                           { TpIconType.UnityRotateCwIcon, UnityRotateCwIcon },
                           { TpIconType.UnityFlipXIcon, UnityFlipXIcon },
                           { TpIconType.UnityFlipYIcon, UnityFlipYIcon },
                           { TpIconType.UnityXIcon, UnityXIcon },
                           { TpIconType.UnityMoveIcon, UnityMoveIcon },
                           { TpIconType.UnityPickIcon, UnityPickIcon },
                           { TpIconType.UnityToolbarMinusIcon, UnityToolbarMinusIcon },
                           { TpIconType.TptIcon, TptIcon },
                           { TpIconType.LockedIcon, LockedIcon },
                           { TpIconType.UnLockedIcon, UnLockedIcon },
                           { TpIconType.PaletteIcon, PaletteIcon },
                           { TpIconType.ClipboardIcon, ClipboardIcon },
                           { TpIconType.PrefabIcon, PrefabIcon },
                           { TpIconType.TileIcon, TileIcon },
                           { TpIconType.TileFabIcon, TileFabIcon },
                           { TpIconType.TilemapIcon, TilemapIcon },
                           { TpIconType.HelpIcon, HelpIcon },
                           { TpIconType.SettingsIcon, SettingsIcon },
                           { TpIconType.InfoIcon, InfoIcon },
                           { TpIconType.FilterIcon, FilterIcon },
                           { TpIconType.ArrowUp, ArrowUp },
                           { TpIconType.ArrowDown, ArrowDown },
                           { TpIconType.ArrowRight, ArrowRight },
                           { TpIconType.TrashIcon, TrashIcon },
                           { TpIconType.PlusIcon, PlusIcon },
                           { TpIconType.MinusIcon, MinusIcon },
                           { TpIconType.EyeballIcon, EyeballIcon },
                           { TpIconType.LifepreserverIcon, LifePreserverIcon },
                           { TpIconType.SyncIcon, SyncIcon },
                           { TpIconType.RefreshIcon, RefreshIcon },
                           { TpIconType.UnityRefreshIcon, UnityRefreshIcon },
                           { TpIconType.UnityOrbitIcon, UnityOrbitIcon },
                           { TpIconType.UnityTextAssetIcon, UnityTextAssetIcon },
                           { TpIconType.CombinedTilesIcon, CombinedTilesIcon },
                           { TpIconType.PinIcon, PinIcon },
                           { TpIconType.UnityTransformIcon, UnityTransformIcon },
                           { TpIconType.UnityCameraIcon, UnityCameraIcon },
                           { TpIconType.UnitySaveIcon, UnitySaveIcon },
                           { TpIconType.UnityRecordIcon, UnityRecordIcon },
                           { TpIconType.UnityForwardIcon, UnityForwardIcon },
                           { TpIconType.UnityEyedropperIcon, UnityEyedropperIcon },
                           { TpIconType.UnityGridIcon, UnityGridIcon}
                       };
        }

        private static Texture2D UnityPaintIcon() => EditorGUIUtility.IconContent(ProSkin ? "Grid.PaintTool@2x" : "d_Grid.PaintTool@2x").image as Texture2D;
        private static Texture2D UnityEraseIcon() => EditorGUIUtility.IconContent(ProSkin ? "Grid.EraserTool@2x" : "d_Grid.EraserTool@2x").image as Texture2D;
        private static Texture2D UnityRotateCwIcon() => EditorGUIUtility.IconContent($"Packages/com.unity.2d.tilemap/Editor/Icons/{(ProSkin ? "Grid.RotateCW@2x" : "d_Grid.RotateCW@2x")}.png").image as Texture2D; //only available when tilemap editor installed.
        private static Texture2D UnityRotateCcwIcon() => EditorGUIUtility.IconContent($"Packages/com.unity.2d.tilemap/Editor/Icons/{(ProSkin ? "Grid.RotateACW@2x" : "d_Grid.RotateACW@2x")}.png").image as Texture2D;
        private static Texture2D UnityFlipXIcon()     => EditorGUIUtility.IconContent($"Packages/com.unity.2d.tilemap/Editor/Icons/{(ProSkin ? "Grid.FlipX@2x" : "d_Grid.FlipX@2x")}.png").image as Texture2D;
        private static Texture2D UnityFlipYIcon()     => EditorGUIUtility.IconContent($"Packages/com.unity.2d.tilemap/Editor/Icons/{(ProSkin ? "Grid.FlipY@2x": "d_Grid.FlipY@2x")}.png").image as Texture2D;
        private static Texture2D UnityRefreshIcon()      => EditorGUIUtility.IconContent(ProSkin ? "Refresh@2x" : "d_Refresh@2x").image as Texture2D;
        private static Texture2D UnityXIcon()            => EditorGUIUtility.IconContent(ProSkin ? "clear@2x" : "d_clear@2x").image as Texture2D;
        private static Texture2D UnityMoveIcon()         => EditorGUIUtility.IconContent(ProSkin ? "Grid.MoveTool@2x" : "d_Grid.MoveTool@2x").image as Texture2D;
        private static Texture2D UnityPickIcon()         =>  EditorGUIUtility.IconContent(ProSkin ? "Grid.PickingTool@2x" : "d_Grid.PickingTool@2x").image as Texture2D;
        private static Texture2D UnityToolbarMinusIcon() => EditorGUIUtility.IconContent(ProSkin ? "Toolbar Minus@2x" : "d_Toolbar Minus@2x").image as Texture2D;
        private static Texture2D TptIcon()               => Resources.Load<Texture2D>("TilePlus/TilePlusIcon");
        private static Texture2D LockedIcon()            => Resources.Load<Texture2D>(ProSkin ? "TilePlus/locked" : "TilePlus/lockedDark");
        private static Texture2D UnLockedIcon()          => Resources.Load<Texture2D>(ProSkin ? "TilePlus/unlocked" : "TilePlus/unlockedDark");
        private static Texture2D PaletteIcon()           => Resources.Load<Texture2D>(ProSkin ? "TilePlus/palette" : "TilePlus/paletteDark");
        private static Texture2D ClipboardIcon()         => Resources.Load<Texture2D>(ProSkin ? "TilePlus/clipboard" : "TilePlus/clipboardDark");
        private static Texture2D PrefabIcon()            => EditorGUIUtility.IconContent(ProSkin ? "d_Prefab Icon" : "Prefab Icon").image as Texture2D;
        private static Texture2D TileIcon()              => EditorGUIUtility.IconContent(ProSkin ? "d_ScriptableObject Icon" : "ScriptableObject Icon").image as Texture2D;
        private static Texture2D TileFabIcon()           => Resources.Load<Texture2D>(ProSkin ? "TilePlus/TileFab" : "TilePlus/TileFabDark");
        private static Texture2D TilemapIcon()           => EditorGUIUtility.IconContent(ProSkin ? "d_Tilemap Icon" : "Tilemap Icon").image as Texture2D;
        private static Texture2D HelpIcon()              => EditorGUIUtility.IconContent(ProSkin ? "d_Help@2x" : "Help@2X").image as Texture2D;
        private static Texture2D SettingsIcon()          => Resources.Load<Texture2D>(ProSkin ? "TilePlus/Options" :"TilePlus/OptionsDark");
        private static Texture2D InfoIcon()              => Resources.Load<Texture2D>( ProSkin ? "TilePlus/information" :"TilePlus/informationDark");
        private static Texture2D FilterIcon()            =>  Resources.Load<Texture2D>(ProSkin ? "TilePlus/filter" :"TilePlus/filterDark");
        private static Texture2D ArrowUp()               => Resources.Load<Texture2D>(ProSkin ? "TilePlus/ArrowUp" : "TilePlus/ArrowUpDark");
        private static Texture2D ArrowDown()             => Resources.Load<Texture2D>(ProSkin ? "TilePlus/ArrowDown" : "TilePlus/ArrowDownDark");
        private static Texture2D ArrowRight()            => Resources.Load<Texture2D>(ProSkin ? "TilePlus/arrowRight" : "TilePlus/arrowRightDark");
        private static Texture2D TrashIcon()             => Resources.Load<Texture2D>(!ProSkin ? "TilePlus/trashcanDark" : "TilePlus/trashcan");
        private static Texture2D PlusIcon()              => Resources.Load<Texture2D>(ProSkin ? "TilePlus/plus" : "TilePlus/plusDark");
        private static Texture2D MinusIcon()             => Resources.Load<Texture2D>(ProSkin ? "TilePlus/minus" : "TilePlus/minusDark");
        private static Texture2D EyeballIcon() => Resources.Load<Texture2D>(ProSkin? "TilePlus/eyeball" : "TilePlus/eyeballDark");
        private static Texture2D LifePreserverIcon() => Resources.Load<Texture2D>(ProSkin ? "TilePlus/lifePreserver" : "TilePlus/lifepreserverDark");
        private static Texture2D SyncIcon() => Resources.Load<Texture2D>(ProSkin ? "TilePlus/sync" : "TilePlus/syncDark");
        private static Texture2D RefreshIcon() => Resources.Load<Texture2D>(ProSkin ? "TilePlus/refresh" : "TilePlus/refreshDark");
        private static Texture2D UnityOrbitIcon() => EditorGUIUtility.IconContent(ProSkin ? "d_ViewToolOrbit@2x" : "ViewToolOrbit@2x").image as Texture2D;
        private static Texture2D UnityTextAssetIcon() => EditorGUIUtility.IconContent(ProSkin ? "d_TextAsset Icon" : "TextAsset Icon").image as Texture2D;
        private static Texture2D UnityTransformIcon() => EditorGUIUtility.IconContent(ProSkin ? "d_Transform Icon" : "Transform Icon").image as Texture2D;
        private static Texture2D CombinedTilesIcon() => Resources.Load<Texture2D>(ProSkin ? "TilePlus/CombinedTiles" : "TilePlus/CombinedTilesDark");
        private static Texture2D PinIcon() => Resources.Load<Texture2D>(ProSkin ? "TilePlus/pin"  : "TilePlus/pinDark");
        private static Texture2D UnityCameraIcon() => EditorGUIUtility.IconContent("Camera Icon" /*ProSkin ? "d_CameraIcon" : "Camera Icon"*/).image as Texture2D;
        private static Texture2D UnitySaveIcon() => EditorGUIUtility.IconContent(ProSkin ? "d_SaveAs@2x" : "SaveAs@2x").image as Texture2D;
        private static Texture2D UnityRecordIcon() => EditorGUIUtility.IconContent(ProSkin ? "d_Record Off@2x" : "Record Off@2x").image as Texture2D;
        private static Texture2D UnityForwardIcon() => EditorGUIUtility.IconContent(ProSkin ? "d_forward@2x" : "forward@2x").image as Texture2D;
        private static Texture2D UnityEyedropperIcon() => EditorGUIUtility.IconContent(ProSkin ? "d_eyeDropper.Large@2x" : "eyeDropper.Large@2x").image as Texture2D;
        
        private static Texture2D UnityGridIcon() => EditorGUIUtility.IconContent(ProSkin ?"d_Grid Icon" : "Grid Icon").image as Texture2D;
            
        #endregion
            
            
    }

}
