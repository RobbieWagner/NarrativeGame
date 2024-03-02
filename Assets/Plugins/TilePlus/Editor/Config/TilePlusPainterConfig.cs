// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 11-01-2022
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 12-31-2022
// ***********************************************************************
// <copyright file="TilePlusPainterConfig.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>Configuration options for TilePlus Painter</summary>
// ***********************************************************************
using UnityEditor;
using UnityEngine;

namespace TilePlus.Editor.Painter
{
    internal enum TPP_SettingThatChanged
    {
        /// <summary>
        /// For Painter - sync Rditor selection and Painter selection
        /// </summary>
        SyncSelection,
        /// <summary>
        /// Painter update-in-play
        /// </summary>
        UpdateInPlay,
        /// <summary>
        /// Painter max tiles shown setting
        /// </summary>
        MaxTilesInViewer,
        /// <summary>
        ///Fab authoring on/off
        /// </summary>
        FabAuthoring,
        /// <summary>
        /// Fab authoring chunk size 
        /// </summary>
        FabAuthoringChunkSize,
        /// <summary>
        /// FabAuthoring World origin
        /// </summary>
        FabAuthoringOrigin,
        /// <summary>
        /// Changed the Painter list item height.
        /// </summary>
        PainterListItemHeight
    }
    
    /// <summary>
    /// Scriptable Singleton for Painter config
    /// </summary>
    [FilePath("TpConfig/TilePlusPainterConfig.asset",FilePathAttribute.Location.ProjectFolder)]
    public class TilePlusPainterConfig : ScriptableSingleton<TilePlusPainterConfig>
    {
        private void OnEnable()
        {
            EditorApplication.update += DoSave;
        }

        [SerializeField]
        private float m_PainterListItemHeight = 16f;

        public float PainterListItemHeight
        {
            get => m_PainterListItemHeight;
            set
            {
                if (value < 14)
                    value = 14;
                if (value > 30)
                    value = 30;
                var change = new ConfigChangeInfo(m_PainterListItemHeight, value);

                m_PainterListItemHeight = value;
                RegisterSave();
                TpEditorUtilities.SettingHasChanged(TPP_SettingThatChanged.PainterListItemHeight.ToString(), change);  

            }
        }
        

        [SerializeField]
        private Color m_TpPainterMarqueeColor = Color.white;
        public Color TpPainterMarqueeColor
        {
            get => m_TpPainterMarqueeColor;
            set { m_TpPainterMarqueeColor = value; RegisterSave(); }
        }

        [SerializeField]
        private Color m_TpPainterSceneTextColor = Color.white;

        public Color TpPainterSceneTextColor
        {
            get => m_TpPainterSceneTextColor;
            set { m_TpPainterSceneTextColor = value; RegisterSave(); }
        }

        [SerializeField]
        private bool m_TpPainterUsedOnce;

        public bool TpPainterUsedOnce
        {
            get => m_TpPainterUsedOnce;
            set { m_TpPainterUsedOnce = value; RegisterSave(); }
        }

        [SerializeField]
        private bool m_TpPainterShowTilefabs;

        public bool TpPainterShowTilefabs
        {
            get => m_TpPainterShowTilefabs;
            set { m_TpPainterShowTilefabs = value; RegisterSave(); }
        }
        
        [SerializeField]
        private bool m_TpPainterShowCombinedTiles;

        public bool TpPainterShowCombinedTiles
        {
            get => m_TpPainterShowCombinedTiles;
            set { m_TpPainterShowCombinedTiles = value; RegisterSave(); }
        }
        
        [SerializeField]
        private bool m_TpPainterShowPalettes = true;

        public bool TpPainterShowPalettes
        {
            get => m_TpPainterShowPalettes;
            set { m_TpPainterShowPalettes = value; RegisterSave(); }
        }

        

        [SerializeField]
        private bool m_TpPainterShowIid;

        public bool TpPainterShowIid
        {
            get => m_TpPainterShowIid;
            set { m_TpPainterShowIid = value; RegisterSave(); }
        }


        [SerializeField]
        private bool m_TpPainterPickToPaint;

        public bool TpPainterPickToPaint
        {
            get => m_TpPainterPickToPaint;
            set { m_TpPainterPickToPaint = value; RegisterSave(); }

        }
        
        
        [SerializeField]
        private bool m_TpPainterSyncSelection;

        public bool TpPainterSyncSelection
        {
            get => m_TpPainterSyncSelection;
            set
            {
                var change = new ConfigChangeInfo(m_TpPainterSyncSelection, value);

                m_TpPainterSyncSelection = value;
                TpEditorUtilities.SettingHasChanged(TPP_SettingThatChanged.SyncSelection.ToString(),change);
                RegisterSave();
            }
        }

        [SerializeField] private bool m_TpPainterTilemapSorting; //when FALSE, simple alpha sort. When true, order by sorting layer then sorting order.

        public bool TpPainterTilemapSorting
        {
            get => m_TpPainterTilemapSorting;
            set { m_TpPainterTilemapSorting = value; RegisterSave(); }
        }
        
        [SerializeField]
        private TpTileSorting m_TpPainterTileSorting = TpTileSorting.Type;

        public TpTileSorting TpPainterTileSorting
        {
            get => m_TpPainterTileSorting;
            set { m_TpPainterTileSorting = value; RegisterSave(); }
        }
        
        
        //max num tiles for viewers.
        [SerializeField]
        private int m_MaxTilesForViewers = 200;

        public int MaxTilesForViewers
        {
            get => m_MaxTilesForViewers;
            set
            {
                value = value >= 50 ? value : 50;
                var change = new ConfigChangeInfo(m_MaxTilesForViewers, value);

                m_MaxTilesForViewers = value;
                TpEditorUtilities.SettingHasChanged(TPP_SettingThatChanged.MaxTilesInViewer.ToString(), change);  
                RegisterSave(); }
        }
        
        
        //auto-refresh for TpPainter
        [SerializeField]
        private bool m_PainterAutoRefresh = true;

        public bool PainterAutoRefresh
        {
            get => m_PainterAutoRefresh;
            set
            {
                var change = new ConfigChangeInfo(m_PainterAutoRefresh, value);

                m_PainterAutoRefresh = value; 
                RegisterSave(); 
                TpEditorUtilities.SettingHasChanged(TPP_SettingThatChanged.UpdateInPlay.ToString(),change);  

            }
        }

        
        [SerializeField]
        private bool m_PainterFabAuthoringMode;
        public bool PainterFabAuthoringMode
        {
            get => m_PainterFabAuthoringMode;
            set
            {
                var change = new ConfigChangeInfo(m_PainterFabAuthoringMode, value);

                m_PainterFabAuthoringMode       = value; 
                RegisterSave(); 
                
                TpEditorUtilities.SettingHasChanged(TPP_SettingThatChanged.FabAuthoring.ToString(),change);  

            }
        }
        
        [SerializeField]
        private int m_PainterFabAuthoringChunkSize = 16;

        public int PainterFabAuthoringChunkSize
        {
            get => m_PainterFabAuthoringChunkSize;
            set
            {
                if (value < 4 || value % 2 != 0)
                    value = 4;
                var change = new ConfigChangeInfo(m_PainterFabAuthoringChunkSize, value);

                m_PainterFabAuthoringChunkSize = value;
                RegisterSave();
                TpEditorUtilities.SettingHasChanged(TPP_SettingThatChanged.FabAuthoringChunkSize.ToString(), change);  

            }
        }
        
        [SerializeField]
        private Vector3Int m_FabAuthWorldOrigin;

        public Vector3Int FabAuthWorldOrigin
        {
            get => m_FabAuthWorldOrigin;
            set
            {
                var change = new ConfigChangeInfo(m_FabAuthWorldOrigin, value);

                m_FabAuthWorldOrigin = value;
                RegisterSave();
                TpEditorUtilities.SettingHasChanged(TPP_SettingThatChanged.FabAuthoringOrigin.ToString(), change);  

            }
        }
        
        
        [SerializeField]
        private bool m_PainterShowOnlyIcons;
        
        public bool PainterShowOnlyIcons
        {
            get => m_PainterShowOnlyIcons;
            // ReSharper disable once ValueParameterNotUsed
            set
            {
                //m_PainterShowOnlyIcons = value; 
                //RegisterSave(); 
            }
        }
        
        private bool needsSave;
        private void RegisterSave()
        {
            needsSave = true;
        }

        /// <summary>
        /// Called periodically 
        /// </summary>
        private void DoSave()
        {
            if (!needsSave)
                return;
            Save(true);
            needsSave = false;
        }

        public void Reset()
        {
            m_MaxTilesForViewers                    = 200;
            m_TpPainterMarqueeColor                 = Color.white;
            m_TpPainterSceneTextColor               = Color.white;
            m_TpPainterUsedOnce                     = false;
            m_TpPainterShowTilefabs                 = false;
            m_TpPainterShowPalettes                 = true;
            m_TpPainterSyncSelection                = false;
            m_PainterListItemHeight                 = 16f;
            TpPainterTileSorting                    = TpTileSorting.Type;
            m_PainterAutoRefresh                    = true;
            m_TpPainterShowIid                      = false;
            TpPainterPickToPaint                    = true;
            m_PainterShowOnlyIcons                  = false;
            m_PainterFabAuthoringMode               = false;
            m_PainterFabAuthoringChunkSize          = 16;
            m_FabAuthWorldOrigin                    = new Vector3Int(0, 0, 0);
            m_TpPainterTilemapSorting               = false;

        }
    }
}
