// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 01-01-2023
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 12-30-2022
// ***********************************************************************
// <copyright file="TpPreviewUtility.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>Allows brushes to handle previews properly for TileBase tiles like Rule tiles. </summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[assembly: InternalsVisibleTo("TilePlusPainter")]
namespace TilePlus.Editor
{
    /// <summary>
    /// Utilities for handling tile previews, including control over TpPainterPlugins
    /// for TileBase tiles (those without sprite properties)
    /// </summary>
    
    internal static class TpPreviewUtility
    {
       
        /// <summary>
        /// A mapping from tile Types to plugins.
        /// </summary>
        private static readonly Dictionary<Type, TpPainterPluginBase> s_TileTypeToPluginMap = new();

        private static bool s_Initialized;

        /// <summary>
        /// Reset the Type->Plugin mapping
        /// </summary>
        internal static void Reset()
        {
            s_TileTypeToPluginMap.Clear();
            BuildTilePluginsMap();

        }

        private static void UpdateMapIfEmpty()
        {
            if (s_Initialized || s_TileTypeToPluginMap.Count != 0)
                return;
            BuildTilePluginsMap();
            s_Initialized = true;

        }

        /// <summary>
        /// How many plugins are there?
        /// </summary>
        internal static int PluginCount => s_TileTypeToPluginMap.Count;

        
        /// <summary>
        /// Get a list of all plugins
        /// </summary>
        [NotNull]
        internal static List<TpPainterPluginBase> AllPlugins => s_TileTypeToPluginMap.Values.ToList();

        /// <summary>
        /// Does a plugin exist for a Type
        /// </summary>
        /// <param name="t">a Type</param>
        /// <returns>true if a plugin exists for this Type</returns>
        internal static bool PluginExists([NotNull] Type t)
        {
            if(!s_Initialized)
                UpdateMapIfEmpty();
            return s_TileTypeToPluginMap.ContainsKey(t);
        }

        /// <summary>
        /// Get a plugin if it exists
        /// </summary>
        /// <param name="tileType">The type of tile</param>
        /// <param name="plug">out param for the plugin</param>
        /// <returns>true if a plugin for this type exists</returns>
        internal static bool TryGetPlugin([CanBeNull] Type tileType, [CanBeNull] out TpPainterPluginBase plug)
        {
            if (tileType != null)
            {
                if(!s_Initialized)
                    UpdateMapIfEmpty();
                return s_TileTypeToPluginMap.TryGetValue(tileType, out plug);
            }

            plug = null;
            return false;

        }

        /// <summary>
        /// Get a plugin if it exists
        /// </summary>
        /// <param name="tileBase">a tile instance</param>
        /// <param name="plug">out param for the plugin</param>
        /// <returns>true if a plugin for this type exists</returns>
        internal static bool TryGetPlugin([NotNull] TileBase tileBase, [CanBeNull] out TpPainterPluginBase plug)
        {
            if(!s_Initialized)
                UpdateMapIfEmpty();
            return s_TileTypeToPluginMap.TryGetValue(tileBase.GetType(), out plug);
        }

        /// <summary>
        /// Get a preview icon for a tile. 
        /// </summary>
        /// <param name="tileBase">tile to get a preview for</param>
        /// <returns>a preview icon or a '?' if none available</returns>
        internal static Texture2D PreviewIcon([NotNull] TileBase tileBase)
        {
            if(!s_Initialized)
                UpdateMapIfEmpty();

            Texture2D preview = null;
           
            if (tileBase is ITilePlus itp)
                preview = itp.PreviewIcon;
            if (preview == null && tileBase is Tile uTile)
                preview = AssetPreview.GetAssetPreview(uTile.sprite);
            if (preview == null)
            {
               if (s_TileTypeToPluginMap.TryGetValue(tileBase.GetType(), out var plug))
                   preview = AssetPreview.GetAssetPreview(plug.GetSpriteForTile(tileBase));
                
            }
            if (preview == null)
                preview = TpIconLib.FindIcon(TpIconType.HelpIcon);
            return preview;
        }
        
        /// <summary>
        /// Look for any  tile-type plugins and build the mapping.
        /// </summary>
        private static void BuildTilePluginsMap()
        {
            var guids     = AssetDatabase.FindAssets("t:TpPainterPluginBase");
            var ttPlugins = new TpPainterPluginBase[guids.Length];
            var index     = 0;
            for (var i = 0; i < guids.Length; i++)
            {
                var pluginScrObj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), typeof(TpPainterPluginBase)) as TpPainterPluginBase;
                if (pluginScrObj == null)
                    continue;
                ttPlugins[index++] = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(pluginScrObj)) as TpPainterPluginBase;
            }

            s_TileTypeToPluginMap.Clear();
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (var plug in ttPlugins)
            {
                if(plug.m_IgnoreThis)
                    continue;
                s_TileTypeToPluginMap.Add(plug.GetTargetTileType, plug);
            }

            if(TpLibEditor.Informational)
                TpLib.TpLog($"Found {s_TileTypeToPluginMap.Count} TileType Plugins");
        }
        
        
    }
}
