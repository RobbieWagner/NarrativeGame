// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 11-01-2021
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 12-31-2022
// ***********************************************************************
// <copyright file="TilePlusPainterWindow.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>Editor window for Tile+Painter</summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static TilePlus.Editor.TpIconLib;
using static TilePlus.TpLib;
using Label = UnityEngine.UIElements.Label;
using Object = UnityEngine.Object;


#nullable  enable

namespace TilePlus.Editor.Painter
{
    /// <summary>
    /// An alternate Palette for painting Unity Tilemaps 
    /// </summary>
    public class TilePlusPainterWindow : EditorWindow
    {
        #region subscriptions

        /// <summary>
        /// Subscribe to this to get notified when painter's modes change.
        /// </summary>
        public static event Action<GlobalMode, TpPainterTool, TpPainterMoveSequenceStates>? OnPainterModeChange;
        
        #endregion
        
        #region privateAndInternalFields

        /// <summary>
        /// this window instance. Never more than 1.
        /// </summary>
        private static TilePlusPainterWindow? s_PainterWindow;
        
        //overall mode: Painting or Editing
        /// <summary>
        /// The global mode indicates Painting or Editing modes
        /// </summary>
        [SerializeField]
        private GlobalMode m_GlobalMode = GlobalMode.PaintingView;
        /// <summary>
        /// The previous global mode
        /// </summary>
        [SerializeField] 
        private GlobalMode m_PreviousGlobalMode = GlobalMode.EditingView;
        //the current tool
        /// <summary>
        /// The current tool
        /// </summary>
        [SerializeField]
        private TpPainterTool m_CurrentTool   = TpPainterTool.None;
        /// <summary>
        /// The previous tool
        /// </summary>
        [SerializeField]
        private TpPainterTool m_PreviousTool  = TpPainterTool.None;
        /// <summary>
        /// The tool to restore
        /// </summary>
        [SerializeField]
        private TpPainterTool m_ToolToRestore = TpPainterTool.None;

        //operation targets
        /// <summary>
        /// Which tilemap to paint or edit
        /// </summary>
        [SerializeField]
        internal PaintTarget? m_TilemapPaintTarget; //tilemap to use
        /// <summary>
        /// Which palette (Palette,Tilefab|Bundle, History) is selected
        /// </summary>
        [SerializeField]
        internal PaletteListItem? m_PaletteTarget;
        
       
        //source data for UIElements ListViews
        /// <summary>
        /// data for the tilemap ListView 
        /// </summary>
        [SerializeField] 
        internal List<TilemapData> m_TilemapListViewItems = new(32);  
        /// <summary>
        /// source for the list of tiles to display in the right pane in either mode.
        /// note that the source for the above list can be from a palette, favorites, or Tilefab chunk (Paint mode) or from the tiles in a selected tilemap (Edit mode)
        /// </summary>
        [SerializeField] 
        internal List<TargetTileData> m_TilesToDisplay = new(128);
        
        /// <summary>
        /// source for the list of tiles (center pane) when in Tilemaps view
        /// </summary>
        [SerializeField]
        internal List<TileBase> m_CurrentTileList = new(128);
        
        

        /// <summary>
        /// current Type to filter withNotify
        /// </summary>
        internal Type m_FilterType = typeof(TileBase);

        /// <summary>
        /// current tag to filter withNotify  
        /// </summary>
        internal string m_FilterTag = ReservedTag;

        /// <summary>
        /// The editor selection lock is used to avoid infinite recursion/stack overflow for certain UIElement selection operations.
        /// </summary>
        [SerializeField]
        private bool m_EditorSelectionLock;

        /// <summary>
        /// Is the GUI initialized?
        /// </summary>
        [NonSerialized]
        private bool guiInitialized;

        /// <summary>
        /// Indicates that this window has created its own control ID .
        /// </summary>
        [FormerlySerializedAs("hasControlId")]
        [SerializeField]
        private bool m_HasControlId;

        /// <summary>
        /// Indicates that an Editor update should refresh the tiles list
        /// </summary>
        private bool wantsRefreshTilesList;

        /// <summary>
        /// Flag to refresh the tiles view when a Scene has been loaded/unloaded
        /// </summary>
        private bool updateOnSceneChange;

        /// <summary>
        /// TRUE if the TpPainterTool (withNotify the UNITY toolbar) is activated.
        /// </summary>
        [SerializeField] 
        internal bool m_ToolActivated; 

        /// <summary>
        /// The current palette search string
        /// </summary>
        internal string m_CurrentPaletteSearchString = string.Empty;

        /// <summary>
        /// lists objects that want to get notified about changes to TilePluConfig
        /// </summary>
        private readonly List<ISettingsChangeWatcher> settingsChangeWatchers = new();

        /// <summary>
        /// Control ID for this window
        /// </summary>
        [SerializeField] 
        private int m_MyControlId;
        /// <summary>
        /// # of iterations of CreateGui whilst waiting for TpLib to be ready.
        /// </summary>
        [SerializeField] 
        private int m_CreateGuiIterations; 

        /// <summary>
        /// The history list
        /// </summary>
        [SerializeField] //mod tpt2b1
        private List<TileBase> m_HistoryStack = new();

        //uielements refs
        #nullable disable
        /// <summary>
        /// The tab bar
        /// </summary>
        private TpPainterTabBar        tabBar;
        /// <summary>
        /// The tp painter tilemaps panel: the view which shows all the tilemaps
        /// </summary>
        private TpPainterTilemapsPanel tpPainterTilemapsPanel;  
        /// <summary>
        /// The tp painter content panel: the view which shows palettes/palette content OR list of tiles/tile inspector
        /// </summary>
        private TpPainterContentPanel  tpPainterContentPanel;   
        /// <summary>
        /// The tp painter settings panel
        /// </summary>
        private TpPainterSettingsPanel tpPainterSettingsPanel;  
        /// <summary>
        /// The tp painter help panel
        /// </summary>
        private TpPainterHelpPanel     tpPainterHelpPanel;
        /// <summary>
        /// The tilemaps and content panel: contains tilemaps and content panels. Hidden when Settings or Help panels are displayed.
        /// </summary>
        private VisualElement          tilemapsAndContentPanel; 
        /// <summary>
        /// The status area mini buttons: mini-buttons at the bottom of the window.
        /// </summary>
        private TpPainterMiniButtons   statusAreaMiniButtons;   
        /// <summary>
        /// The status label: status shown next to minibuttons
        /// </summary>
        private Label                  statusLabel;

        private TpSplitter mainSplitView;
        
        #nullable enable
        
        /// <summary>
        /// Set to indicate it's time to refresh the tag filter
        /// </summary>
        [NonSerialized]
        private bool refreshTagFilter;

        /// <summary>
        /// Set to indicate it's time to refresh the Type filter
        /// </summary>
        [NonSerialized]
        private bool refreshTypeFilter;

        
        private int lastTilemapSelectionIndex = -1;
        private int lastPaintModeSelectionIndex = -1;
        private int lastEditModeSelectionIndex = -1;
        private int lastPaintModeInspectorListSelectionIndex = -1;
        
        
        #endregion

        #region properties

        
        /// <summary>
        /// Is this editor window active? Only true if control IDs match or focusedWindow == this window or its SceneView window..
        /// </summary>
        public bool IsActive
        {
            get
            {
                try //note: need try/catch here because this is polled from a static class and odd stuff can happen when computer 'unsleeps' & window is open.
                {
                    return guiInitialized && 
                           (GUIUtility.hotControl == m_MyControlId
                            || focusedWindow == this
                            || (mouseOverWindow != null && mouseOverWindow.GetType() == typeof(SceneView)));
                }
                catch
                {
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Is Preview active: is a preview being shown in the Scene view?
        /// </summary>
        internal bool PreviewActive => guiInitialized && TpPainterSceneView.PreviewActive;

        /// <summary>
        /// Is the mouse over the Painter Editor window?
        /// </summary>
        internal bool MouseOverTpPainter =>
            mouseOverWindow != null && mouseOverWindow.GetType() == typeof(TilePlusPainterWindow);

        /// <summary>
        /// Control ID for this EditorWindow
        /// </summary>
        internal int PainterWindowControlId => m_MyControlId;
        
        /// <summary>
        /// Get the painter instance directly. will not open any window. Use with care...
        /// </summary>
        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public static TilePlusPainterWindow? RawInstance => s_PainterWindow;

        /// <summary>
        /// STATIC method to gets the painter window instance.
        /// </summary>
        /// <value>The painter window instance</value>
        /// <remarks>Yes, this is a singleton editor window!</remarks>
        // ReSharper disable once InconsistentNaming
        public static TilePlusPainterWindow? instance
        {
            get
            {
                if(s_PainterWindow != null)
                    return s_PainterWindow;
                
                var objs = Resources.FindObjectsOfTypeAll<TilePlusPainterWindow>(); 
                if (objs is { Length: > 0 })
                {

                    s_PainterWindow = objs[0];
                    return s_PainterWindow;
                }

                s_PainterWindow              = GetWindow<TilePlusPainterWindow>();
                s_PainterWindow.titleContent = new GUIContent("Tile+Painter", FindIcon(TpIconType.TptIcon)); 
                s_PainterWindow.minSize      = new Vector2(WindowMinWidth, 384);
                s_PainterWindow.ConditionalResetState();

                return s_PainterWindow;
            }
            private set => s_PainterWindow = value;
        }
        
        

        /// <summary>
        /// Gets the current tool.
        /// </summary>
        /// <value>The current tool.</value>
        public TpPainterTool         CurrentTool   => m_CurrentTool;

        /// <summary>
        /// Does the current tool have any Tilemap effect?
        /// </summary>
        internal bool CurrentToolHasTilemapEffect =>
            m_CurrentTool != TpPainterTool.Help
            && m_CurrentTool != TpPainterTool.Settings
            && m_CurrentTool != TpPainterTool.None;


        /// <summary>
        /// Which Tilemap are we using??
        /// </summary>
        internal PaintTarget? TilemapPaintTarget => m_TilemapPaintTarget;
        
        /// <summary>
        /// a count of # of tiles in Edit mode (center column).
        /// </summary>
        internal int TilemapPaintTargetCount { get; private set; }


        /// <summary>
        /// the selected or picked tile to paint/move (Palette mode) or the selected/picked tile to inspect.
        /// </summary>
        internal TargetTileData? TileTarget { get; set; }

        /// <summary>
        /// is the GUI initialized?
        /// </summary>
        /// <value>T/F</value>
        internal bool GuiInitialized => guiInitialized;

        /// <summary>
        /// Are we discarding list selection events?
        /// </summary>
        /// <value>T/F</value>
        internal bool DiscardListSelectionEvents => m_EditorSelectionLock;

        /// <summary>
        /// What's the global mode?
        /// </summary>
        /// <value>The global mode.</value>
        internal GlobalMode GlobalMode => m_GlobalMode;

        /// <summary>
        /// Get the tab bar instance
        /// </summary>
        /// <value>The tab bar.</value>
        internal TpPainterTabBar TabBar => tabBar;

        /// <summary>
        /// Get the TilePlusConfig instance
        /// </summary>
        /// <value>The configuration.</value>
        private TilePlusPainterConfig Config => TilePlusPainterConfig.instance;

        /// <summary>
        /// How many items are in the history stack
        /// </summary>
        /// <value>The size of the history stack.</value>
        internal int HistoryStackSize => m_HistoryStack.Count;

        /// <summary>
        /// The sequence state for Move operations
        /// </summary>
        [SerializeField]
        private TpPainterMoveSequenceStates m_TpPainterMoveSequenceState;

        internal TpPainterMoveSequenceStates TpPainterMoveSequenceState
        {
            get => m_TpPainterMoveSequenceState;
            set
            {
                m_TpPainterMoveSequenceState = value;
                OnPainterModeChange?.Invoke(m_GlobalMode, m_CurrentTool, m_TpPainterMoveSequenceState);
            }
        }

        /// <summary>
        /// Get a string to use for the tilemap list header.
        /// </summary>
        /// <value>string</value>
        private string TilemapListHeader
        {
            get
            {
                return m_GlobalMode switch
                       {
                           GlobalMode.EditingView  => "Editing Tilemap",
                           GlobalMode.PaintingView => "Painting Tilemap",
                           _                       => "GridSelection Target"
                       };
            }
        }

        /// <summary>
        /// Is the tilemap selection valid?
        /// </summary>
        /// <value>T/F</value>
        internal bool ValidTilemapSelection => m_TilemapPaintTarget is { Valid: true };

        /// <summary>
        /// Is painting allowed?
        /// </summary>
        /// <value>The painting allowed.</value>
        internal bool PaintingAllowed
        {
            get
            {
                //note that if a tile is picked this implies that one had selected a tilemap and that s_PaletteTileTarget != null
                var wasPicked      = TileTarget is { WasPickedTile: true, Valid: true };
                var isValid        = TileTarget is { Valid        : true };
                //next line determines if we have a valid place to paint. This is overridden if the thing to paint is a TileFab since the Tilemap selection in the window is irrelevant.
                var hasPaintTarget = m_TilemapPaintTarget is
                                     {
                                         Valid: true
                                     }
                                                    ////TileFabs don't need a Tilemap selection.
                                     || (isValid && (TileTarget is not null && TileTarget.ItemVariety == TargetTileData.Variety.TileFabItem));   
                return hasPaintTarget && (wasPicked || isValid);        //OK to paint if we have a paint target AND the tile was Picked or is just Valid
            }
        }

        /// <summary>
        /// Data source for the list of tiles (RIGHTMOST column) when in Palette mode
        /// </summary>
        /// <value>ListView items.</value>
        internal List<TargetTileData> TargetPaletteListViewItems
        {
            get
            {
                TileSourceAssetScanner();
                return m_TilesToDisplay;
            }
        }

        /// <summary>
        /// Data source for the list of tilemaps in the LEFTMOST column
        /// </summary>
        /// <value>ListView items.</value>
        private List<TilemapData> TilemapListViewItems
        {
            get
            {
                m_TilemapListViewItems.Clear();
                TpPainterScanners.ValidateMapCache();
                foreach (var map in TpPainterScanners.CurrentTilemaps)
                {
                    var stage      = PrefabStageUtility.GetCurrentPrefabStage();
                    var objInStage = stage != null && stage.IsPartOfPrefabContents(map.gameObject);
                    m_TilemapListViewItems.Add(new TilemapData(map, PrefabUtility.IsPartOfPrefabInstance(map.gameObject),objInStage));
                }

                return m_TilemapListViewItems;
            }
        }

        internal int NumEditModeTiles => m_CurrentTileList.Count;

        
        
        #endregion


        #region constants

        /// <summary>
        /// The empty field label
        /// </summary>
        internal const string EmptyFieldLabel        = "-----";
        /// <summary>
        /// The toolbar container height
        /// </summary>
        private const  float  ToolbarContainerHeight = 20; 

        /// <summary>
        /// The view panes minimum width
        /// </summary>
        internal const float ViewPanesMinWidth = 150;
        /// <summary>
        /// The mode bar minimum width
        /// </summary>
        private const  float  ModeBarMinWidth        = 80;
        /// <summary>
        /// The window minimum width
        /// </summary>
        private const  float  WindowMinWidth         = 550;
        /// <summary>
        /// The splitter size: width of the splitter handle
        /// </summary>
        internal const float  SplitterSize           = 4;
        
        
        
        
        #endregion


        #region init

        /// <summary>
        /// Open the TilePlusViewer window
        /// </summary>
        [Shortcut("TilePlus/Painter:Open", KeyCode.Alpha1, ShortcutModifiers.Alt)]
        [MenuItem("Tools/TilePlus/Tile+Painter", false, 0)] 
        public static void ShowWindow()
        {
            if (RawInstance != null) //ie window already created
            {
                GetWindow<TilePlusPainterWindow>();
                instance!.m_GlobalMode = GlobalMode.PaintingView;
                instance.m_PreviousGlobalMode = GlobalMode.EditingView;
                return;
            }
            instance              = GetWindow<TilePlusPainterWindow>();
            instance.titleContent = new GUIContent("Tile+Painter", FindIcon(TpIconType.TptIcon)); 
            instance.minSize      = new Vector2(WindowMinWidth, 384);
            instance.m_GlobalMode = GlobalMode.PaintingView;
            instance.m_PreviousGlobalMode = GlobalMode.EditingView;
            instance.ConditionalResetState();
            
            
        }

        private void ConditionalResetState()
        {
            var window = RawInstance;
            if (window == null)
            {
                Debug.LogError("No painter instance found!");
                return;
            }
            if(TpLibEditor.Informational)
                TpLog("T+P: Waiting for init to complete... Launch task");
            TpConditionalTasks.ConditionalDelayedCallback(window, () =>
                                                     {
                                                         //use of RawInstance is important here otherwise window could/would re-open
                                                         var win = RawInstance;
                                                         if (win == null)
                                                         {
                                                             Debug.LogError("No painter instance found!");
                                                             return;
                                                         }

                                                         refreshTagFilter             = true;
                                                         refreshTypeFilter            = true; 
                                                         TpPainterMoveSequenceState = TpPainterMoveSequenceStates.None;

                                                         if (Application.isPlaying)
                                                         {
                                                             m_GlobalMode = GlobalMode.EditingView;
                                                             m_PreviousGlobalMode = GlobalMode.PaintingView; //needed for handler to work correctly
                                                             tabBar.ActivateModeBarButton(GlobalMode.EditingView, true);
                                                         }
                                                         else
                                                         {
                                                             m_GlobalMode = GlobalMode.PaintingView;
                                                             m_PreviousGlobalMode = GlobalMode.EditingView;
                                                             tabBar.ActivateModeBarButton(GlobalMode.PaintingView, true);
                                                         }

                                                         var domainPrefs              = TpEditorBridge.DomainPrefs;
                                                         var domainPrefsDisableReload = domainPrefs is { EnterPlayModeOptionsEnabled: true, DisableDomainReload: true }; 
                                                         wantsRefreshTilesList = domainPrefsDisableReload;
                                                         updateOnSceneChange   = domainPrefsDisableReload;

                                                         TpPainterSceneView.Reset();
                                                            
                                                         m_EditorSelectionLock          = false;
                                                         m_CurrentPaletteSearchString = string.Empty;

                                                         var     sel              = Selection.activeGameObject;
                                                         Tilemap? map              = null;
                                                         var     restoreSelection = sel != null && sel.TryGetComponent(out map);
                                                       
                                                         TpPainterScanners.TransformAssetScanner();
                                                         TpPainterScanners.TilemapsScan(); 
                                                         if (TilemapListViewItems.Count != 0)
                                                         {
                                                             tpPainterTilemapsPanel.UpdateTilemapsList(TilemapListViewItems);
                                                             if(restoreSelection)
                                                                 TryRestoreTilemapSelection(map!.GetInstanceID(),true);
                                                             else
                                                                 TpLib.DelayedCallback(this,() => tpPainterTilemapsPanel.SetSelectionWithoutNotify(-1),
                                                                                                    "T+P: RebuildTilemapsList");
                                                         }
                                                         else
                                                             tpPainterTilemapsPanel.SetSelectionLabel(EmptyFieldLabel);
                                                       
                                                         

                                                     },Condition, "T+P: Conditional Reset State");
                                                     

            //note: if this were a lambda there would be closures of the variables which is incorrect in this
            //case as we want to know current values.
            //Also, important to use RawInstance here. If instance is used and it were null, the window would reopen.
            bool Condition(int _) => TpLibIsInitialized && RawInstance != null && RawInstance.guiInitialized;
        }


        //DO NOT REMOVE THIS!!
        /// <summary>
        /// Init On Load method. IE, this executes after a scripting reload.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            if(TpLibEditor.Informational)
                TpLog("T+P: OnLoad - 'InitializeOnLoadMethod"); 
            
            ResetSpriteCache(); //in TpIconLib

            //important to use RawInstance here to avoid auto-reopening the window.
            if(!HasOpenInstances<TilePlusPainterWindow>() || RawInstance == null)
                return;

            //in case this is a re-load, clear any active tasks.
            TpConditionalTasks.KillConditionalTasksForObject(RawInstance);
                RawInstance.ReInit();
        }


        #endregion

        #region Events

        
        
        /// <summary>
        /// UPDATE event
        /// </summary>
        private void Update()
        {
            if(!guiInitialized)
                return;

            if (updateOnSceneChange && !IsSceneScanActive)
            {
                updateOnSceneChange = false;
                RebuildTilemapsList();
                RefreshTilesView();
            }

            if (!Application.isPlaying &&
                TpPreviewUtility.PluginCount == 0) //since there are always at least two, this would be an error
            {
                TpPreviewUtility.Reset();
            }
                
            
            if (Application.isPlaying)
            {
                if (Config.PainterAutoRefresh)
                    tpPainterContentPanel.RepaintImgui(m_GlobalMode);
            }
            else if (m_GlobalMode == GlobalMode.EditingView)
            {
                if (refreshTagFilter && tpPainterContentPanel != null)
                {
                    refreshTagFilter = false;
                    tpPainterContentPanel.RecomputeTagFilter();
                }
                if (refreshTypeFilter && tpPainterContentPanel != null)
                {
                    refreshTypeFilter = false;
                    tpPainterContentPanel.RecomputeTypeFilter();
                }
            }
        }

        /// <summary>
        /// Unity Inspector Update event
        /// </summary>
        private void OnInspectorUpdate()
        {
            if (!guiInitialized)
                return;
            var dragLockInfo = TpPainterSceneView.DragLock;
            var text         = string.Empty;
            if (dragLockInfo.m_DragX)
                text = "Active + Drag Lock X";
            else if (dragLockInfo.m_DragY)
                text = "Active + Drag Lock Y";
            else if (TpPainterShortCuts.TpPainterActive)
                text = "Active";

            if (Config.PainterFabAuthoringMode)
                text = $"{text} [Snapping:ON]";

            var (active, bounds, _) = TpPainterSceneView.GridSelMarqueeState;
            if (active)
                text = $"{text} [Marquee-Drag][Selection:{bounds.ToString()}]";

            statusLabel.text = text;
            
            //disable right-hand splitview when in move mode. This dims/inactivates the list of Palettes and their assets.
            tpPainterContentPanel.EnableContentPanelSplitView(m_CurrentTool != TpPainterTool.Move);

            //show/hide the red indicator for the SelectionSync mini-button
            statusAreaMiniButtons.SetActivatedIndicator(m_ToolActivated);

            if (wantsRefreshTilesList)
            {
                wantsRefreshTilesList = false;
                if (m_GlobalMode == GlobalMode.EditingView)
                    RefreshTilesView();
            }

            //enable/disable main toolbar buttons
            var notHelpOrSettings                    = m_CurrentTool != TpPainterTool.Help && m_CurrentTool != TpPainterTool.Settings;
            var enableAllActions                     = m_GlobalMode != GlobalMode.GridSelView && notHelpOrSettings;
            var enableManipulators                   = enableAllActions && ValidTilemapSelection;
            var enableManipulatorsExceptWhenSnapping = enableManipulators && (!Config.PainterFabAuthoringMode || m_GlobalMode == GlobalMode.EditingView);
            //erase is a bit more complex. It's active for enableManipulators but only if (not chunk snapping) or if chunk snapping, need an active zone manager instance.
            
            //toolbar button enable/disable
            tabBar.EnableToolbarButton(TpPainterTool.None,           enableAllActions);
            tabBar.EnableToolbarButton(TpPainterTool.Paint,          m_GlobalMode == GlobalMode.PaintingView && PaintingAllowed && enableAllActions);
            tabBar.EnableToolbarButton(TpPainterTool.Erase,          enableManipulators);
            tabBar.EnableToolbarButton(TpPainterTool.Pick,           enableManipulatorsExceptWhenSnapping);
            tabBar.EnableToolbarButton(TpPainterTool.Move,           enableManipulatorsExceptWhenSnapping);
            tabBar.EnableToolbarButton(TpPainterTool.RotateCw,       enableManipulatorsExceptWhenSnapping);
            tabBar.EnableToolbarButton(TpPainterTool.RotateCcw,      enableManipulatorsExceptWhenSnapping);
            tabBar.EnableToolbarButton(TpPainterTool.FlipX,          enableManipulatorsExceptWhenSnapping);
            tabBar.EnableToolbarButton(TpPainterTool.FlipY,          enableManipulatorsExceptWhenSnapping);
            tabBar.EnableToolbarButton(TpPainterTool.ResetTransform, enableManipulatorsExceptWhenSnapping);

            tabBar.EnableToolbarButton(TpPainterTool.Help,     m_CurrentTool != TpPainterTool.Settings);
            tabBar.EnableToolbarButton(TpPainterTool.Settings, m_CurrentTool != TpPainterTool.Help);

            tabBar?.EnableModeBarButton(GlobalMode.GridSelView,  notHelpOrSettings);
            tabBar?.EnableModeBarButton(GlobalMode.PaintingView, notHelpOrSettings);
            tabBar?.EnableModeBarButton(GlobalMode.EditingView,  notHelpOrSettings);

            //The tilemaps column (left) is always visible
            var selIndex = tpPainterTilemapsPanel.SelectionIndex;
            if (selIndex >= 0)
            {
                if (lastTilemapSelectionIndex == -1)
                    lastTilemapSelectionIndex = selIndex;
                else if (selIndex != lastTilemapSelectionIndex)
                {
                    tpPainterTilemapsPanel.SetTarget(selIndex);
                    lastTilemapSelectionIndex = selIndex;
                }

            }

            //painting view: check list selection for center column
            if (m_GlobalMode == GlobalMode.PaintingView)
            {
                selIndex = tpPainterContentPanel.PaintModeAssetSelectionIndex;
                if (selIndex > 0)
                {
                    if (lastPaintModeSelectionIndex == -1)
                        lastPaintModeSelectionIndex = selIndex;
                    else if (selIndex != lastPaintModeSelectionIndex)
                    {
                        lastPaintModeSelectionIndex = selIndex;
                        tpPainterContentPanel.SelectPaletteOrOtherSource(selIndex);
                    }
                }

                //and check selection for right column: list of tiles etc.
                selIndex = tpPainterContentPanel.PaintModeTilesListViewSelectionIndex;
                if (selIndex > 0)
                {
                    if (lastPaintModeInspectorListSelectionIndex == -1)
                        lastPaintModeInspectorListSelectionIndex = selIndex;
                    else if (selIndex != lastPaintModeInspectorListSelectionIndex)
                    {
                        lastPaintModeInspectorListSelectionIndex = selIndex;
                        tpPainterContentPanel.SelectBrushInspectorTarget(selIndex);
                    }
                }

            }
            //edit view: check list selection.
            else if (m_GlobalMode == GlobalMode.EditingView)
            {
                selIndex = tpPainterContentPanel.EditModeAssetSelectionIndex;
                if (selIndex > 0)
                {
                    if (lastEditModeSelectionIndex == -1)
                        lastEditModeSelectionIndex = selIndex;
                    else if (selIndex != lastEditModeSelectionIndex)
                    {
                        lastEditModeSelectionIndex = selIndex;
                        tpPainterContentPanel.SelectTile(selIndex);
                    }
                }
            }

            tpPainterTilemapsPanel.SetSelectionLabel(m_TilemapPaintTarget is { Valid: true }
                                                         ? m_TilemapPaintTarget.Name
                                                         : EmptyFieldLabel);
    
            //label for which tilemap is selected
            if (m_GlobalMode == GlobalMode.PaintingView)
                tpPainterContentPanel.ShowTilemapsListSelectionNeededHelpBox(false);
            else  if(m_GlobalMode == GlobalMode.EditingView)//editing view
                tpPainterContentPanel.ShowTilemapsListSelectionNeededHelpBox(m_TilemapPaintTarget != null && m_TilemapPaintTarget is not { Valid: true });
        }

       

        /// <summary>
        /// OnBecameVisible event
        /// </summary>
        private void OnBecameVisible()
        {
            ResetSelections();
            OnHierarchyChange();
        }

        /// <summary>
        /// OnBecameInvisible event
        /// </summary>
        private void OnBecameInvisible()
        {
            ResetSelections();
        }
        
       

        /// <summary>
        /// OnEnable event
        /// </summary>
        private void OnEnable()
        {
            if(RawInstance != null)
                RawInstance.ConditionalResetState();
            
            var controlIdInfo = TpLibEditor.GetPermanentControlId(); //Get the control ID for this window.
            if (controlIdInfo.valid)
                m_HasControlId = true;
            else
                return;

            m_MyControlId = controlIdInfo.id;

            TpPainterScanners.ResetTilemapScanData();
            TpPainterScanners.ResetPaletteScanData();
            m_CurrentTileList.Clear();

            
            TpLibEditor.ResetGridPalettesCache(); //hack to get the palette list code in this TileMaps internal class to work properly.
            //U3D Code for this is buggy IMO.
            //Part of the issue is that unless the Unity Palette painter window is opened the cache isn't set up correctly (?)

            EditorApplication.playModeStateChanged     += OnplayModeStateChanged;
            ToolManager.activeToolChanged              += OnToolmanagerActiveToolChanged;
            TpEditorUtilities.RefreshOnSettingsChange  += OnSettingsRefreshRequest;
            OnTpLibChanged                             += OnTilemapDbChangedSingle; //when a single change happens
            OnSceneScanComplete                        += SceneScanComplete;
            EditorSceneManager.sceneClosed             += OnSceneClosed;
            EditorSceneManager.sceneOpened             += OnSceneOpened;
            TpLib.OnTypeOrTagChanged                   += OnTypeOrTagChanged; //when a Type or tag is added or removed from TpLib.
            
            TpEditorUtilities.RefreshOnTilemapsCleared += UpdateTilemaps;
            GridSelection.gridSelectionChanged         += OngridSelectionChanged;

        }

 
        private void UpdateTilemaps()
        {
            TpPainterScanners.TilemapsScan();
            if (tpPainterTilemapsPanel != null) 
                tpPainterTilemapsPanel.UpdateTilemapsList(TilemapListViewItems);
        }
        
        /// <summary>
        /// When this window is added as a tab to another window via drag/drop
        /// </summary>
        private void OnAddedAsTab()
        {
            if(TpConditionalTasks.IsActiveConditionalTask(RawInstance) != 0)
                ReInit();
        }
        
        /// <summary>
        ///When this window is removed as a tab of another window
        /// </summary>
        private void OnTabDetached()
        {
            if(TpConditionalTasks.IsActiveConditionalTask(RawInstance) != 0)
                ReInit();
        }
        
        

        //this would be called when a grid sel is made from the Palette
        private void OngridSelectionChanged()
        {
            if(m_GlobalMode != GlobalMode.GridSelView )
                return;
            if (!GridSelection.active || !GridSelection.target.activeSelf || !GridSelection.target.TryGetComponent<Tilemap>(out _))
                return;
            tpPainterContentPanel.AddGridSelection(GridSelection.position);
        }

        //this would be called when adding a grid sel from the GridSelPanel while mouse-dragging w/hotkey for grid selection creation (default is alt+5)
        internal void AddGridSelection(Object target, BoundsInt bounds)
        {
            tpPainterContentPanel.AddGridSelection(target,bounds);
        }

        //this is called when adding a grid sel from TpPainterSceneView after ALT+5 dragging
        internal void AddGridSelection(BoundsInt bounds, bool silent = false)
        {
            tpPainterContentPanel.AddGridSelection(bounds,silent);
        }
        

        /// <summary>
        /// OnDisable event
        /// </summary>
        private void OnDisable()
        {
            /*#if UNITY_EDITOR
            Debug.Log("ONDISABLE");
            #endif*/
            guiInitialized = false;
            rootVisualElement.Clear();
            DeselectGridSelPanel();
            TpPainterMoveSequenceState = TpPainterMoveSequenceStates.None;
            instance                     = null!;
            
            //in case this really isn't a prequel to DESTRUCTION!!
            m_GlobalMode         = GlobalMode.PaintingView;
            m_PreviousGlobalMode = GlobalMode.EditingView;
            m_CurrentTool        = TpPainterTool.None;
            m_ToolToRestore      = TpPainterTool.None;
            m_PreviousTool       = TpPainterTool.None;
            

            EditorApplication.playModeStateChanged -= OnplayModeStateChanged;
            OnTpLibChanged                         -= OnTilemapDbChangedSingle; //when a single change happens
            OnSceneScanComplete                    -= SceneScanComplete;
            TpLib.OnTypeOrTagChanged               -= OnTypeOrTagChanged;

            EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorSceneManager.sceneOpened -= OnSceneOpened;


            ToolManager.activeToolChanged              -= OnToolmanagerActiveToolChanged;
            TpEditorUtilities.RefreshOnSettingsChange  -= OnSettingsRefreshRequest;
            TpEditorUtilities.RefreshOnTilemapsCleared -= UpdateTilemaps;
            GridSelection.gridSelectionChanged         -= OngridSelectionChanged;

            
            if (ToolManager.activeToolType == typeof(TilePlusPainterTool))
            {
                if (Selection.activeObject is GameObject go)
                {
                    var possibleTilemap = go.GetComponent<Tilemap>();
                    if (possibleTilemap != null)
                    {
                        var grid = GetParentGrid(possibleTilemap.transform);
                        if (grid != null)
                            Selection.SetActiveObjectWithContext(grid.gameObject, null);
                    }
                }

                // Try to activate previously used tool
                TpLib.DelayedCallback(this, ToolManager.RestorePreviousPersistentTool, "TpPainterTool.RestoreToolOnDisable");

            }
        }
        
        
        

        /// <summary>
        /// PlayMode StateChange handler
        /// </summary>
        /// <param name="change">The change.</param>
        private void OnplayModeStateChanged(PlayModeStateChange change)
        {
            DeselectGridSelPanel();
            if (change is not (PlayModeStateChange.EnteredEditMode or PlayModeStateChange.EnteredPlayMode))
                return;

            
            if (change is PlayModeStateChange.EnteredPlayMode)
            {
                TpLib.DelayedCallback(this,() =>
                                           {
                                               m_PreviousGlobalMode = GlobalMode.PaintingView; //needed for handler to work correctly
                                               tabBar?.ActivateModeBarButton(GlobalMode.EditingView, true);
                                           }, "T+P: EnterPlayMode-forceEditView", 100);

            }
            wantsRefreshTilesList = true;
            updateOnSceneChange   = true;
            m_CurrentTileList.Clear(); //added 1 feb 23

            //tpPainterContentPanel.TilesListViewSetSelection(-1);

            /*var enteredPlayMode = change is PlayModeStateChange.EnteredPlayMode;
            if (enteredPlayMode)
                guiInitialized = false;
            TpTaskManager.ConditionalDelayedCallback(null,() => 
                                          { 
                                              wantsRefreshTilesList = true;
                                              updateOnSceneChange       = true;
                                              //ClearClipboard();
                                              
                                              //tpPainterContentPanel.ResetFilters();
                                              //tpPainterContentPanel.TilesListViewSetSelection(-1);
                                          }, (int frameCount) => TpLibIsInitialized &&
                                                                !IsSceneScanActive &&
                                                                 frameCount > 1000 &&
                                                                 RawInstance != null 
                                                                 && RawInstance.guiInitialized,
                                                     "T+P-ONPLAYMODESTATECHANGE");*/
        }

        /// <summary>
        /// Callback from TpLib when something has changed. 
        /// </summary>
        /// <param name="args">DbChangedArgs instance</param>
        private void OnTilemapDbChangedSingle(DbChangedArgs args)
        {
            if(!guiInitialized)
                return;
            
            if (IsSceneScanActive)
            {
                TpLog("T+P: OnTilemapDbChangedSingle - ignoring during Scene scan; Resetting filters");
                tpPainterContentPanel.ResetFilters();
                return;
            }

            if (TpLibEditor.Informational)
                TpLog($"T+P: OnTilemapDbChangedSingle [changeType: {args.m_ChangeType.ToString()}][PartOfGroup:{args.m_IsPartOfGroup}");

            if (args.m_IsPartOfGroup)
            {
                return;
            }

            //need to test if an addition of a TPT tile to a tilemap w/out any TPT tiles previously.
            //In this case, need to update the bindings for the tilemap list so that
            //the TPT icon is correctly shown. This is delayed because
            //this can get called during Awake/Startup or OnEnable which will cause an exception.
            if (args.m_ChangeType == DbChangedArgs.ChangeType.AddedToEmptyMap)
            {
                DelayedCallback(this, () => tpPainterTilemapsPanel.ReBindElement(TilemapListViewItems),
                    "T+P:Delayed-Rebind-onAddedToEmptyMap");
            }
            
            //same sort of idea for deletions
            else if (args.m_ChangeType == DbChangedArgs.ChangeType.Deleted)
            {
                if (!TpLib.IsTilemapRegistered(args.m_Tilemap))
                    tpPainterTilemapsPanel.ReBindElement(TilemapListViewItems);
            }
            
            
            else if (args.m_ChangeType == DbChangedArgs.ChangeType.TagsModified)
            {
                if (TpLibEditor.Informational)
                    TpLog("Tags were modified (T+Painter)");
                if (m_FilterTag == ReservedTag)
                    return;
                tpPainterContentPanel.ResetFilters();
            }

            //this one is tricky: it means that a Unity tile (ie Tile or TileBase)
            //was added OR modified. If modified, we don't need to refresh the
            //tiles list: actually we really DON'T want to change it since that
            //will cause any Selection Inspector (in TileDataGui) to revert to 
            //a brush inspector.
            else if (args.m_ChangeType == DbChangedArgs.ChangeType.ModifiedOrAdded)
            {
                //note that the args position param here is not valid.
                if (TileTarget == null)
                    return;

                //if not editing view or if tiletarget is null or if not picked tile or if it is a TPT tile.
                if (m_GlobalMode != GlobalMode.EditingView || TileTarget is { Valid:true, WasPickedTile: false, IsNotTilePlusBase: false })
                    return;
                var changedPositions    = NonTppTilesAddedOrModified; //this is created in the TpLib OntilemapTileChanged callback
                var numChangedPositions = changedPositions.Count;
                if(changedPositions.Count == 0)
                    return;
                
                //was something added? We only care about avoiding a tilemap scan when the mode
                //is EDIT and the selection inspector is showing a picked Unity tile.
                for (var i = 0; i < numChangedPositions; i++)
                {
                    if (TileTarget.Position == changedPositions[i])
                        continue;
                    wantsRefreshTilesList = true;
                    return;

                }
                
                return;
            }

            wantsRefreshTilesList = true;
        }

        /// <summary>
        /// Subcribed to TpLib.OnTypeOrTagChanged
        /// </summary>
        /// <param name="variety">which tag changed?</param>
        private void OnTypeOrTagChanged(OnTypeOrTagChangedVariety variety)
        {
            if (variety == OnTypeOrTagChangedVariety.Tag)
                refreshTagFilter = true;
            else
                refreshTypeFilter = true;
        }

        /// <summary>
        /// Sceme Closed delegate
        /// </summary>
        /// <param name="_">The .</param>
        private void OnSceneClosed(Scene _)
        {
            DeselectGridSelPanel();
            #if UNITY_EDITOR
            if(TpLibEditor.Informational)
                TpLog("T+P: OnSceneClosed");
            #endif
            updateOnSceneChange = true;

        }

        /// <summary>
        /// Scene Opened delegate
        /// </summary>
        /// <param name="scene">The scene.</param>
        /// <param name="mode">The mode.</param>
        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            DeselectGridSelPanel();
            #if UNITY_EDITOR
            if(TpLibEditor.Informational)
                TpLog("T+P: OnSceneOpened");
            #endif

            updateOnSceneChange = true;
        }
        
        /// <summary>
        /// Scene Scan Complete delegate
        /// </summary>
        private void SceneScanComplete()
        {
            #if UNITY_EDITOR
            if(TpLibEditor.Informational)
                TpLog("T+P: OnSceneScanComplete");
            #endif

            updateOnSceneChange = true;
        }


        /// <summary>
        /// Distributes settings change events to visual elements.
        /// </summary>
        /// <param name="change">The change.</param>
        /// <param name="changes">The new value.</param>
        private void OnSettingsRefreshRequest(string change, ConfigChangeInfo changes)
        {
            if (change == TPP_SettingThatChanged.FabAuthoring.ToString())
                RebuildPaletteListIfChanged();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < settingsChangeWatchers.Count; i++)
                settingsChangeWatchers[i].OnSettingsChange(change, changes);
        }

        
        /// <summary>
        /// OnHeirarchyChange delegate.
        /// </summary>
        private void OnHierarchyChange()
        {
            if(!guiInitialized)
                return;
            if (IsSceneScanActive)
            {
                TpLog("T+P: OnHierarchyChange - ignoring during Scene scan; Resetting filters");
                tpPainterContentPanel.ResetFilters();
                return;
            }
            
            if (TpPainterSceneView.IgnoreNextHierarchyChange)
            {
                if (TpLibEditor.Informational)
                    TpLog("Ignoring Heirarchy change in T+P");
                TpPainterSceneView.IgnoreNextHierarchyChange = false;
                return;

            }
            if (TpLibEditor.Informational)
                TpLog("Processing Heirarchy change in T+P");

            if (!guiInitialized)
                return;

            /*
            if(m_GlobalMode != GlobalMode.GridSelView)
                DeselectGridSelPanel();
                */

            
            if (m_GlobalMode == GlobalMode.PaintingView && !Application.isPlaying)
                RebuildPaletteListIfChanged(); //need to do this since the Palette is a scene with a tilemap so presumably "heirarchy" somehow.
            
            
            if (TpPainterScanners.TilemapsScan(true)) //if this returns true then num/names of tilemaps has changed
                tpPainterTilemapsPanel.UpdateTilemapsList(TilemapListViewItems);
            
            
            if (m_TilemapPaintTarget is { Valid: true })
            {
                var existingTilemapSelectionId = m_TilemapPaintTarget.TargetTilemap!.GetInstanceID();

                tpPainterTilemapsPanel.RebuildElement();
                if (!TryRestoreTilemapSelection(existingTilemapSelectionId))
                    tpPainterTilemapsPanel.SetSelectionLabel(EmptyFieldLabel);
            }
            else
            {
                tpPainterTilemapsPanel.RebuildElement();
                tpPainterTilemapsPanel.SetSelectionLabel(EmptyFieldLabel);
            }

            
        }

        //need to detect new assets like palettes etc. 
        /// <summary>
        /// OnProjectChange delegate
        /// </summary>
        private void OnProjectChange()
        {
            if (guiInitialized)
            {
                DeselectGridSelPanel();
                RebuildPaletteListIfChanged();
            }
        }

        /// <summary>
        /// Called from TpPainterTool when activated
        /// </summary>
        /// <param name="activeObject">object to select</param>
        internal void OnEditorSelectionChangeFromTool(Object activeObject)
        {
            if (!guiInitialized)
                return;

            if (!Config.TpPainterSyncSelection || activeObject is not GameObject go)
                return;
            
            //get the tilemap target
            var tilemapComponents = go.GetComponentsInChildren<Tilemap>();
            if (tilemapComponents.Length != 1 || m_TilemapPaintTarget is null )
                return;
            if(m_TilemapPaintTarget.TargetTilemap == null)
                return;
            if (tilemapComponents[0].GetInstanceID() == m_TilemapPaintTarget.TargetTilemap.GetInstanceID())
                return;
            
            EditorSelectionChangeHandler(activeObject);
        }

        /// <summary>
        /// Selection change event from Hierarchy
        /// </summary>
        private void OnSelectionChange()
        {
            if (!guiInitialized)
                return;

            if (!Config.TpPainterSyncSelection || m_EditorSelectionLock)
                return;
            
            if (Selection.activeObject == null || Selection.count != 1)
                return;
            EditorSelectionChangeHandler(Selection.activeObject);
        }

        /// <summary>
        ///  a system selection change to or from the Painter 'Tool' (TpPainterTool)
        /// </summary>
        /// <param name="selection">selected objects.</param>
        private void EditorSelectionChangeHandler(Object selection)
        {
            if (!guiInitialized || m_GlobalMode == GlobalMode.GridSelView) 
                return;

            if (selection is not GameObject go )
                return;

            var isGrid = go.TryGetComponent<Grid>(out _);
            var isMap  = go.TryGetComponent<Tilemap>(out _);
            if(!isGrid && !isMap)
                return;
            
            DeselectGridSelPanel();

            //reset the move sequence if in PICK state. Move state, no, because we want to be able to pick, change map, and THEN paint.
            if(TpPainterMoveSequenceState is TpPainterMoveSequenceStates.Pick)
                TpPainterMoveSequenceState = TpPainterMoveSequenceStates.None;

            var tilemapComponents = go.GetComponentsInChildren<Tilemap>();
            if (tilemapComponents.Length != 1) //if there are 0 or multiple tilemaps in the selection there's nothing to do but
            {
                tabBar.ActivateToolbarButton(TpPainterTool.None, true); //added notify 12/9
                EditorClearSelection();
                return;
            }

            //look in the itemSource so see if we can find this tilemap.
            var items    = tpPainterTilemapsPanel.DataSource;
            var numItems = items.Count;
            if (numItems == 0)
            {
                tabBar.ActivateToolbarButton(TpPainterTool.None, true); //added notify 12/9
                EditorClearSelection();
                return;
            }

            var selectedMapId  = tilemapComponents[0].GetInstanceID();
            var selectionIndex = -1;
            for (var i = 0; i < numItems; i++)
            {
                var item = items[i];
                if (item is not TilemapData ttd) //unlikely to fail, but need the cast to TilemapData anyway
                    continue;
                if (ttd.TargetMap.GetInstanceID() != selectedMapId) //if this map doesn't match the map in the selection 
                    continue;
                selectionIndex = i;
                break;

            }

            if (selectionIndex >= 0)
            {
                if (items[selectionIndex] is TilemapData tilemapTarget)
                {
                    //is this selected tilemap the same as the current tilemapPaintTarget? If it is, don't call SetPaintTarget because of side effects.
                    if (m_TilemapPaintTarget is not null && m_TilemapPaintTarget.Valid && m_TilemapPaintTarget.TargetTilemap != null 
                        && tilemapTarget.TargetMap.GetInstanceID() != m_TilemapPaintTarget.TargetTilemap.GetInstanceID())
                    {
                        SetPaintTarget(tilemapTarget.TargetMap); //shouldn't fail
                        if (m_GlobalMode == GlobalMode.EditingView)
                            SetInspectorTarget(tilemapTarget.TargetMap);
                    }
                    else
                    {
                        if (m_GlobalMode == GlobalMode.EditingView)
                            SetInspectorTarget(tilemapTarget.TargetMap);
                    }
                    tpPainterTilemapsPanel.SetSelectionWithoutNotify( selectionIndex );
                    
                }
            }
            else
            {
                EditorClearSelection();
                tabBar.ActivateToolbarButton(TpPainterTool.None, true);

            }
        }



        // ReSharper disable once Unity.RedundantEventFunction
        /// <summary>
        /// Focus Event
        /// </summary>
        private void OnFocus() { } //don't remove this.

        /// <summary>
        /// Lost Focus Event
        /// </summary>
        private void OnLostFocus()
        {
            if (!TpPainterSceneView.BrushOpInProgress || !guiInitialized)
                return;
            //DeselectGridSelPanel();
            GUIUtility.hotControl = m_MyControlId;
        }



        
        /// <summary>
        /// Called when Mode Bar is used to change the global mode
        /// </summary>
        /// <param name="choice">0,1, or 2 for Paint, Edit, or GridSel mode</param>
        internal void OnModeBarChanged(int choice)
        {
            TileFabLib.ClearPreview();
            TpPainterSceneView.RemovePreview();
            
            var candidate = (GlobalMode)choice;
            /*if (candidate == m_GlobalMode)
            {
                Debug.Log($"Same mode, no change. requested: {candidate} previous: {m_PreviousGlobalMode} ");
                return;
            }*/

            m_PreviousGlobalMode = m_GlobalMode;
            m_GlobalMode         = candidate;

            TpPainterMoveSequenceState = TpPainterMoveSequenceStates.None;
            
            ClearClipboard();
            tpPainterTilemapsPanel.SetTilemapsListHeader(TilemapListHeader);
            
            OnPainterModeChange?.Invoke(m_GlobalMode, m_CurrentTool, TpPainterMoveSequenceState);

            if ( (candidate == GlobalMode.GridSelView && m_PreviousGlobalMode != GlobalMode.GridSelView) ||
                 (candidate != GlobalMode.GridSelView && m_PreviousGlobalMode == GlobalMode.GridSelView))
            {
                TpPainterScanners.TilemapsScan(); 
                tpPainterTilemapsPanel.UpdateTilemapsList(TilemapListViewItems);
                tpPainterContentPanel.SetDisplayState(m_GlobalMode);
                tabBar.ShowToolbarButton(TpPainterTool.Paint, m_GlobalMode == GlobalMode.PaintingView);
                if(candidate == GlobalMode.GridSelView)
                    return;
            }
            
            if (m_TilemapPaintTarget is not null && m_TilemapPaintTarget.Valid && m_TilemapPaintTarget.TargetTilemap != null )
                SetInspectorTarget(m_TilemapPaintTarget.TargetTilemap);
            
            switch (m_GlobalMode)
            { 
                //changing to edit view
                case GlobalMode.EditingView: //   PaintingView:// when m_GlobalMode == GlobalMode.EditingView:
                    TpPainterMoveSequenceState = TpPainterMoveSequenceStates.None;
                    tpPainterContentPanel.SetDisplayState(m_GlobalMode);
                    /*if (m_TilemapPaintTarget is not null && m_TilemapPaintTarget.Valid && m_TilemapPaintTarget.TargetTilemap != null )
                        SetInspectorTarget(m_TilemapPaintTarget.TargetTilemap);*/
                    tpPainterContentPanel.RebuildTilesListView();

                    if (m_CurrentTool == TpPainterTool.Paint)
                    {
                        m_CurrentTool = TpPainterTool.None;
                        TpLib.DelayedCallback(this,() =>
                                                   {
                                                       tabBar.ActivateToolbarButton(TpPainterTool.None, true);
                                                       tabBar.ShowToolbarButton(TpPainterTool.Paint, false);
                                                   }, "T+P: GM->Palette_deactivate_PaintTool");
                    }
                    else
                        tabBar.ShowToolbarButton(TpPainterTool.Paint, false);

                    break;
                //changing to painting view
                case GlobalMode.PaintingView:  //EditingView: // when m_GlobalMode == GlobalMode.PaintingView:
                    RebuildPaletteListIfChanged(); 
                    tpPainterContentPanel.SetDisplayState(m_GlobalMode);
                    tabBar.ShowToolbarButton(TpPainterTool.Paint, true);
                    break;
                
                 
                
            }
            


        }

        /// <summary>
        /// Target for the Tab bar at the top of the page. Controls what the
        /// user sees in the UI.
        /// </summary>
        /// <param name="index">index of the tab from the (int)enum.</param>
        internal void OnMainToolbarChanged(int index)
        {
            //DeselectGridSelPanel();
            var candidate = (TpPainterTool)index;

            //two element stack: currentTool, previousTool
            m_PreviousTool = m_CurrentTool;
            
            //if tool changing then interrupt any possible Move sequence
            if (candidate != m_CurrentTool)
                TpPainterMoveSequenceState = TpPainterMoveSequenceStates.None;

            OnPainterModeChange?.Invoke(m_GlobalMode,TpPainterTool.None, TpPainterMoveSequenceStates.None);
            
            //handle clicking withNotify Settings or Help when either is activated
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if ((candidate == TpPainterTool.Settings && m_PreviousTool == TpPainterTool.Settings) ||
                (candidate == TpPainterTool.Help && m_PreviousTool == TpPainterTool.Help))
            {
                //since the prev tool = candidate, we want to restore the tool in use prior to using Help or Settings
                TpLib.DelayedCallback(this,() => tabBar.ActivateToolbarButton(m_ToolToRestore, true), "T+P restore tool", 50);
                return;

            }

            //handle switching help pane withNotify/off
            if (candidate == TpPainterTool.Help && m_CurrentTool != TpPainterTool.Help)
            {
                tilemapsAndContentPanel.style.display = DisplayStyle.None;
                tpPainterSettingsPanel.style.display = DisplayStyle.None;
                tpPainterHelpPanel.style.display = DisplayStyle.Flex;
                m_ToolToRestore                    = m_CurrentTool;
                m_CurrentTool                      = TpPainterTool.Help;
                return;
            }

            if (candidate != TpPainterTool.Help && m_CurrentTool == TpPainterTool.Help)
            {
                tilemapsAndContentPanel.style.display     = DisplayStyle.Flex;
                tpPainterSettingsPanel.style.display = DisplayStyle.None;
                tpPainterHelpPanel.style.display     = DisplayStyle.None;
            }

            //handle switching settings pane withNotify/off
            if (candidate == TpPainterTool.Settings && m_CurrentTool != TpPainterTool.Settings)
            {
                tilemapsAndContentPanel.style.display = DisplayStyle.None;
                tpPainterSettingsPanel.style.display  = DisplayStyle.Flex;
                tpPainterHelpPanel.style.display     = DisplayStyle.None;
                m_ToolToRestore               = m_CurrentTool;
                m_CurrentTool                 = TpPainterTool.Settings;
                return;
            }

            if (candidate != TpPainterTool.Settings && m_CurrentTool == TpPainterTool.Settings)
            {
                tilemapsAndContentPanel.style.display     = DisplayStyle.Flex;
                tpPainterSettingsPanel.style.display = DisplayStyle.None;
                tpPainterHelpPanel.style.display     = DisplayStyle.None;
            }
            
            //don't allow buttons to be selected if it doesn't make sense
            if ((candidate == TpPainterTool.Paint && !PaintingAllowed) ||
                (candidate is TpPainterTool.Erase
                              or TpPainterTool.Pick
                              or TpPainterTool.Move
                              or TpPainterTool.RotateCw
                              or TpPainterTool.RotateCcw
                              or TpPainterTool.FlipX
                              or TpPainterTool.FlipY
                              or TpPainterTool.ResetTransform
                 && !ValidTilemapSelection)) //ValidTilemapSelection means that a tilemap is selected
            {
                m_CurrentTool                     = TpPainterTool.None;
                TpLib.DelayedCallback(this,()=> tabBar.ActivateToolbarButton(TpPainterTool.None,false),"T+P force NONE tool",50); 
                return;
            }



            m_CurrentTool                     = candidate;
            
            OnPainterModeChange?.Invoke(m_GlobalMode, m_CurrentTool, TpPainterMoveSequenceStates.None);
            
            if (m_CurrentTool == TpPainterTool.None)
            {
                if(ToolManager.activeToolType == typeof(TilePlusPainterTool))
                    ToolManager.RestorePreviousTool();
            }
            else //if the current selection is a tilemap then change tools
            {
                var selection = Selection.activeGameObject;
                if (selection != null && selection.TryGetComponent(typeof(Tilemap), out _))
                    ToolManager.SetActiveTool(typeof(TilePlusPainterTool));
            }

            if (!CurrentToolHasTilemapEffect)
            {
                if (GUIUtility.hotControl == m_MyControlId)
                    GUIUtility.hotControl = 0;
            }
            else if (TpPainterSceneView.BrushOpInProgress)
            {
                if (GUIUtility.hotControl != m_MyControlId)
                    GUIUtility.hotControl = m_MyControlId;
            }


            var retestSelection = false;
            if (m_CurrentTool is TpPainterTool.None && m_PreviousTool is not TpPainterTool.None)
            {
                if (Config.TpPainterSyncSelection)
                {
                    Selection.activeGameObject = null;
                    retestSelection            = true; //flag to test for selection of any tilemap. Just because someone clicked the leftmost button doesn't mean that the selection is gone.
                }

                TpPainterMoveSequenceState = TpPainterMoveSequenceStates.None;

            }

            if (m_CurrentTool is TpPainterTool.None && m_PaletteTarget == null)
            {
                TpPainterMoveSequenceState = TpPainterMoveSequenceStates.None;
                ClearClipboard();
                if (retestSelection)
                    TestSelection();
                return;
            }


            if (m_CurrentTool is not TpPainterTool.Move && TpPainterMoveSequenceState != TpPainterMoveSequenceStates.None)
            {
                if (TileTarget is { WasPickedTile: true })
                    ClearClipboard();

                TpPainterMoveSequenceState = TpPainterMoveSequenceStates.None;
            }

            if (m_CurrentTool is TpPainterTool.Move)
            {
                TileTarget = null;
                
                switch (TpPainterMoveSequenceState)
                {
                    case TpPainterMoveSequenceStates.None:  //initial state.
                    case TpPainterMoveSequenceStates.Paint: //ie click in Paint state goes to Pick
                        TpPainterMoveSequenceState = TpPainterMoveSequenceStates.Pick;
                        break;
                    case TpPainterMoveSequenceStates.Pick:
                    default: //ie click when in Pick state -> none
                        TpPainterMoveSequenceState = TpPainterMoveSequenceStates.None;
                        m_CurrentTool                  = TpPainterTool.None;
                        TpLib.DelayedCallback(this,()=> tabBar.ActivateToolbarButton(TpPainterTool.None, false), "T+P force NONE tool withNotify cancel move", 50); 
                        break;
                }
            }


            //private method
            void TestSelection()
            {
                var currentSelection = Selection.activeGameObject;
                if (currentSelection == null || Selection.count != 1)
                    return;

                var tilemapComponents = currentSelection.GetComponentsInChildren<Tilemap>();
                if (tilemapComponents.Length != 1) //if there are 0 or multiple tilemaps in the selection there's nothing to do but
                    return;

                var mapToTest = tilemapComponents[0];
                if (!CheckForValidPaintingTarget(mapToTest))
                    return;

                //look in the itemSource so see if we can find the same tilemap.
                var items    = tpPainterTilemapsPanel.DataSource;
                var numItems = items.Count;
                if (numItems == 0)
                    return;

                var selectedMapId  = mapToTest.GetInstanceID();
                var selectionIndex = -1;
                for (var i = 0; i < numItems; i++)
                {
                    var item = items[i];
                    if (item is not TilemapData ttd) //unlikely to fail, but need the cast to TilemapData anyway
                        continue;
                    if (ttd.TargetMap.GetInstanceID() != selectedMapId) //if this map doesn't match the map in the selection 
                        continue;
                    selectionIndex = i;
                    break;

                }

                if (selectionIndex < 0)
                    return;
                if (items[selectionIndex] is TilemapData tilemapTarget)
                    SetPaintTarget(tilemapTarget.TargetMap); //shouldn't fail
                tpPainterTilemapsPanel.SetSelectionWithoutNotify(selectionIndex);

            }

        }

        /// <summary>
        /// Toolmanager tool changed delegate: issued just after tool changes
        /// </summary>
        private void OnToolmanagerActiveToolChanged()
        {
            if (!guiInitialized)
                return;
            
            //if the painter tool is already active then there's nothing to do. Also ignore if the Palette is active
            if (ToolManager.activeToolType == typeof(TilePlusPainterTool) || ToolManager.activeToolType == typeof(TilemapEditorTool))
                return;
            
            DeselectGridSelPanel();

            if(TpLibEditor.Informational)
                TpLog($"Editor Tool change to {ToolManager.activeToolType} ");

            if(m_CurrentTool != TpPainterTool.None)
                //since the painter is no longer the active tool, make it inactive.
                tabBar.ActivateToolbarButton(TpPainterTool.None,true);
        }

        #endregion

        #region CreateGui

        /// <summary>
        /// Used to populate the editor window's RootVisualElement.
        /// </summary>
        private void CreateGUI()
        {
            if (!CreateGuiChecks()) 
                return;
            
            rootVisualElement.Clear();
            TpPainterScanners.TilemapsScan();
            m_HistoryStack.RemoveAll((t) => t == null);
            
            //the four main panels added to the rootVe

            //the tab bar and status indicators
            tabBar = new TpPainterTabBar(ToolbarContainerHeight, ModeBarMinWidth);
            tabBar.SetPickedTileImage(null!);
            rootVisualElement.Add(tabBar);

            tpPainterHelpPanel = new TpPainterHelpPanel();
            rootVisualElement.Add(tpPainterHelpPanel);     //the help panel
            
            //the settings panel.
            tpPainterSettingsPanel = new TpPainterSettingsPanel(this);
            settingsChangeWatchers.Add(tpPainterSettingsPanel);
            rootVisualElement.Add(tpPainterSettingsPanel);

            //the fourth one is the main content panel.
            /*create a panel for the three splitviews.
              One of them holds a panel with the Tilemaps List (left) 
            */
            tilemapsAndContentPanel = new VisualElement { name = "tilemaps-and-Content", viewDataKey = "TPT.TPPAINTER.MAIN",
                                                            style = { flexGrow = 1, flexDirection = FlexDirection.Row } };
            rootVisualElement.Add(tilemapsAndContentPanel);
            

            //build up the MainPanel. This is a splitview with the Tilemaps list withNotify the left and another
            //splitview withNotify the right. This second splitview has a list of palettes or tilemap tiles, withNotify its left and yet a THIRD
            //splitview withNotify its right.
            //Both of these splitviews contain vertically-oriented splitviews:
            // in the center pane (list of palettes or tilemap tiles) that show list of Palette, Chunks, or favorites (Palette mode)
            // along with some controls in the bottom part of the SV. 
            // in the right pane is a VE with two subviews, only one of which is active at a time depending on Paint or Edit modes
            // Paint mode: vertical splitview that shows what's in the selection in the center column.
            //             the top of this view shows the selection contents: what's in the selected palette.
            //             Select from here and the bottom part of the SV is a mini brush inspector.
            // Edit mode;  Selection Inspector showing info about the tile that was selected from the center column.

            mainSplitView = new TpSplitter("painter-splitview-outer",
                                                "TPT.TPPAINTER.SPLITVIEW.LEFT", 
                                                100,
                                                TwoPaneSplitViewOrientation.Horizontal,
                                                0);
            
            var splitterHandle = mainSplitView.Q<VisualElement>("unity-dragline-anchor");
            splitterHandle.style.backgroundColor = Color.red;
            
            tilemapsAndContentPanel.Add(mainSplitView);

            //left split view requires two children:
            tpPainterTilemapsPanel = new TpPainterTilemapsPanel(TilemapListViewItems, ViewPanesMinWidth);
            tpPainterTilemapsPanel.SetTilemapsListHeader(TilemapListHeader);
            mainSplitView.Add(tpPainterTilemapsPanel);
            
            //this child is another splitter - tiles or palettes withNotify left,
            //info about what's selected withNotify the right: either a brush inspector or a selection inspector
            tpPainterContentPanel = new TpPainterContentPanel(ViewPanesMinWidth);
            mainSplitView.Add(tpPainterContentPanel);
            settingsChangeWatchers.Add(tpPainterContentPanel);

            
            
            var container = new VisualElement { style =
                                              {
                                                  flexGrow = 0,
                                                  flexShrink = 0,
                                                  minHeight = ToolbarContainerHeight + 2,
                                                  marginTop = 2,
                                                  flexDirection = FlexDirection.Row, alignContent = Align.Center
                                              } };
            rootVisualElement.Add(container);
            container.Add(statusAreaMiniButtons = new TpPainterMiniButtons(ToolbarContainerHeight,this));
            settingsChangeWatchers.Add(statusAreaMiniButtons);

            statusLabel = new Label{style =
                                   {
                                       color = Color.red,
                                       unityTextAlign = TextAnchor.MiddleCenter
                                       
                                   }};
            container.Add(new TpSpacer(10,20));
            container.Add(statusLabel);
            
            
            guiInitialized = true;
            

            //todo test removal re selection sync
            /*TpEditorUtilities.AddDelayedAction(() =>
                                               {
                                                   //tpPainterTilemapsPanel.SetSelection(0);
                                                   OnModeBarChanged((int)GlobalMode.PaintingView);
                                                   if(!Config.TpPainterUsedOnce)                                   
                                                        OnMainToolbarChanged((int)TpPainterTool.None);
                                               },
                                               "T+P: CreateGui: delayed toobar init",
                                               2);*/

            if (!Config.TpPainterUsedOnce)
            {
                Config.TpPainterUsedOnce = true;
                TpLib.DelayedCallback(this,() => OnMainToolbarChanged((int)TpPainterTool.Help), "T+P:force painter help tab", 100);
            }
            
            else
                TpConditionalTasks.ConditionalDelayedCallback(this,GuiComplete, (frames) => 
                                                                           (frames > 1000 && !TpLib.IsSceneScanActive && tpPainterContentPanel is { Valid: true })
                                                                           ,"T+P: Post gui actions" );
            

            //for convenience, when window opens, sync to current selection IF it is a Tilemap. Ignores config setting for Selection sync
            if (Selection.count == 0)
                return;

            var go = Selection.activeGameObject;
            if (go == null)
                return;

            var possibleMap = go.GetComponent<Tilemap>();
            if (possibleMap != null)
                SetPaintTarget(possibleMap);
        }

       
        private void GuiComplete()
        {
            if (Application.isPlaying)
            {
                m_PreviousGlobalMode = GlobalMode.PaintingView; //needed for handler to work correctly
                tabBar.ActivateModeBarButton(GlobalMode.EditingView, true);
            }
            else
            {
                m_PreviousGlobalMode = GlobalMode.EditingView;
                tabBar.ActivateModeBarButton(GlobalMode.PaintingView, true);
            }


            var domainPrefs = TpEditorBridge.DomainPrefs;
            if (domainPrefs is { EnterPlayModeOptionsEnabled: true, DisableDomainReload: true }) 
            {
                wantsRefreshTilesList = true;
                updateOnSceneChange   = true;
            }
        }


        #endregion


        

        
        #region scanners

        /// <summary>
        /// creates a list of data to display in the rightmost pane.
        /// </summary>
        /// <param name="refresh">refresh the associated view</param>
        
        internal void TileSourceAssetScanner(bool refresh = true)
        {
            m_TilesToDisplay.Clear();
            if (m_PaletteTarget == null)
                return;

            //since the History Stack can have clone TPT tiles and the stack MAY be saved via serialization, lose
            //any null values in the stack that may have persisted if a clone TPT tile was added to the stack
            //and the window closed/reopened or Unity restarts.

            m_HistoryStack.RemoveAll((t) => t == null);
            
            //which source did this data come from?
            //A palette, favorites-asset, or Tilefab chunk-asset (TpTileBundle)?
            if (m_PaletteTarget.ItemType == TpPaletteListItemType.Palette)
            {
                if(m_PaletteTarget.Palette == null)
                    return;
                var map = m_PaletteTarget.Palette.GetComponentInChildren<Tilemap>();
                if (map == null)
                    return;

                map.CompressBounds();
                var bounds = map.cellBounds;

                foreach (var pos in bounds.allPositionsWithin)
                {
                    var t = map.GetTile(pos);
                    if (t == null)
                        continue;
                    m_TilesToDisplay.Add(new TargetTileData(t, pos, map));
                }


                if (m_TilesToDisplay.Count > 1)
                    m_TilesToDisplay.Reverse();
            }

            else if (m_PaletteTarget.ItemType == TpPaletteListItemType.Bundle)
                m_TilesToDisplay.Add(new TargetTileData(m_PaletteTarget.Bundle));

            else if (m_PaletteTarget.ItemType == TpPaletteListItemType.TileFab)
                m_TilesToDisplay.Add(new TargetTileData(m_PaletteTarget.TileFab));
            
            else if (m_PaletteTarget.ItemType == TpPaletteListItemType.History)
                m_TilesToDisplay.AddRange(m_HistoryStack.Select(tb =>
                    new TargetTileData(tb, TilePlusBase.ImpossibleGridPosition, null)));
            //note ImpossibleGridPosition tells TargetTileData to ignore null tilemap source param

            if (tpPainterContentPanel != null)
            {
                var isBundleOrFab =
                    m_PaletteTarget.ItemType is TpPaletteListItemType.Bundle or TpPaletteListItemType.TileFab;

                tpPainterContentPanel.SetVirtualizationForPaintInspector(isBundleOrFab);
            }

            if (refresh && tpPainterContentPanel != null)
                tpPainterContentPanel.RefreshPaletteModeTilesListView();

        }



        #endregion

        #region rebuilders

        //todo this should try to restore selection.
        /// <summary>
        /// Rebuilds the tilemaps list.
        /// </summary>
        internal void RebuildTilemapsList()
        {
            TpPainterScanners.TilemapsScan(); //if this returns true then num of tilemaps has changed.
            if (TilemapListViewItems.Count != 0)
            {

                tpPainterTilemapsPanel.UpdateTilemapsList(TilemapListViewItems);
                TpLib.DelayedCallback(this,() =>
                                           {
                                               if (this != null && tpPainterTilemapsPanel != null)
                                                   tpPainterTilemapsPanel.SetSelectionWithoutNotify(-1);
                                           },
                                      "T+P: RebuildTilemapsList",
                                      50);
            }
            else
                tpPainterTilemapsPanel.SetSelectionLabel(EmptyFieldLabel);

        }

        /// <summary>
        /// Rebuilds the palette list if changed.
        /// </summary>
        internal void RebuildPaletteListIfChanged()
        {
            if (!guiInitialized)
                return;
            TpPainterScanners.PalettesScan(m_CurrentPaletteSearchString);
            /*if (!TpPainterScanners.PalettesScan(m_CurrentPaletteSearchString))
                return;*/

            RebuildPalettesListView();
            //ClearClipboard();
            TileSourceAssetScanner();

        }

        /// <summary>
        /// Rebuilds the palettes ListView.
        /// </summary>
        private void RebuildPalettesListView()
        {
            tpPainterContentPanel.RebuildPalettesListView();
        }

        /// <summary>
        /// Refreshes the palettes ListView.
        /// </summary>
        private void RefreshPalettesListView()
        {
            tpPainterContentPanel.RefreshPalettesListView();
        }

        /// <summary>
        /// Refreshes the tiles view.
        /// </summary>
        internal void RefreshTilesView()
        {
            if (m_TilemapPaintTarget is { Valid: true }  && m_TilemapPaintTarget.TargetTilemap != null)
            {
                SetInspectorTarget(m_TilemapPaintTarget.TargetTilemap);
            }
            else
            {
                SetInspectorTarget(null);
            }
            //leave in for debugging use
            /*else
                Debug.Log("tptarget invalid");*/

        }

        /// <summary>
        /// Refreshes the tiles ListView for Palette mode.
        /// </summary>
        private void RefreshPaletteModeTilesListView()
        {
            tpPainterContentPanel.RefreshPaletteModeTilesListView();
        }

        #endregion

        #region paintTarget

        /// <summary>
        /// Sets the paint target (a TileMap).
        /// </summary>
        /// <param name="target">target tilemap</param>
        /// <returns>true for no errors.</returns>
        internal bool SetPaintTarget(Tilemap? target)
        {
            if (target == null)
                return false;
            
            //ensure a reasonable choice
            if (!CheckForValidPaintingTarget(target))
                return false;

            if (m_TilemapPaintTarget != null
                && m_TilemapPaintTarget.TargetTilemap != null
                && m_TilemapPaintTarget.TargetTilemap == target) //no change, ignore
                return false;
            
            m_TilemapPaintTarget = new PaintTarget(target); //the tilemap to paint withNotify.
            if (!guiInitialized)
                return true;
            tpPainterTilemapsPanel.SetSelectionLabel(target.name);
            
            return true;
        }

        /// <summary>
        /// Checks for valid painting target, ie is the tilemap valid to paint.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>bool.</returns>
        private bool CheckForValidPaintingTarget(Tilemap target)
        {
            return target.gameObject.activeInHierarchy &&
                   target.GetComponentInParent<Grid>() != null &&
                   !PrefabUtility.IsPartOfPrefabAsset(target); 

        }

        #endregion

        #region inspectorTarget
        
        /// <summary>
        /// updates a list of tiles from a tilemap
        /// </summary>
        /// <param name="target">tilemap to obtain tiles from.</param>
        internal void SetInspectorTarget(Tilemap? target)
        {
            if (!guiInitialized)
                return;

            if (target == null)
            {
                m_CurrentTileList.Clear();
                TilemapPaintTargetCount = 0;
                tpPainterContentPanel.RebuildTilesListView();
                return;
            }
            
            tpPainterTilemapsPanel.SetSelectionLabel(target.name);
            var limit         = Config.MaxTilesForViewers;
            var previousCount = m_CurrentTileList.Count;
            m_CurrentTileList.Clear();

            //filterType being TileBase means 'wildcard'
            var wildcard = m_FilterType == typeof(TileBase);
            
            //usingPlugin will be true if the filter type is from a plugin
            //(typically means this is some other class of TileBase such as Rule tile)
            //but if the filter type is 'wildcard' then don't bother to check.
            var usingPlugin  = !wildcard && TpPreviewUtility.PluginExists(m_FilterType); 

            
            /* Type filtering
             * The filter type 'TileBase' means everything
             * The filter type 'TilePlusBase' means all TilePlus tiles
             * The filter type 'Tile' means all standard Unity Tiles + or any Subclasses like TilePlus tiles.
             * Others are dynamically added to the list of possible Types (see ComputeTypeFilter in TpPainterContentPanel.cs)
             */
            //process TPT tiles. First comes tag filtering
            //so we only do this step for TPT tiles ... if the filter isn't 'wildcard' or a subclass of TilePlusBase
            if(wildcard || m_FilterType == typeof(TilePlusBase) || m_FilterType.IsSubclassOf(typeof(TilePlusBase)))
            {
                var tpbList = GetAllTilePlusBaseForMap(target);
                if (tpbList != null)
                {
                    if (tpbList.Count > limit)
                    {
                        m_CurrentTileList.AddRange(tpbList.Take(limit).ToList());
                        TilemapPaintTargetCount = tpbList.Count;
                        tpPainterContentPanel.RebuildTilesListView();
                        return; //don't sort.
                    }


                    m_CurrentTileList.AddRange(wildcard //if wildcard, use all tiles.
                                                   ? tpbList 
                                                   : tpbList.Where(tpb => tpb.GetType() == m_FilterType));

                    //tag filtering only for TPT tiles.
                    if (m_FilterTag != ReservedTag)
                    {
                        m_CurrentTileList.RemoveAll(tb =>
                                                    {
                                                        var tpb = tb as TilePlusBase;
                                                        if (tpb == null)
                                                            return true; //if a tile's somehow null then filter it out.
                                                        var (num, tags) = tpb.TrimmedTags;
                                                        if (num == 0)
                                                            return true; //if no tag AND we're using tag filtering then remove this tile from the list.
                                                        for (var i = 0; i < num; i++)
                                                        {
                                                            var tag = tags[i];
                                                            if (string.IsNullOrWhiteSpace(tag))
                                                                continue;
                                                            if (m_FilterTag == tag)
                                                                return false; //tag match: keep this tile
                                                        }

                                                        return true; //no match, remove tile
                                                    });
                    }

                }
            }

            
            

            //if any tag filter at all, Unity tiles get filtered out.
            //usingPlugin is true when the type is derived from TileBase instead of Tile. This allows for Rule tiles,
            //which are NOT subclasses of Tile. 
            if (m_FilterTag == ReservedTag && (wildcard || usingPlugin || m_FilterType == typeof(Tile) )  ) 
            {
                //unity tiles: 
                var numTileAssets = target.GetUsedTilesCount();
                var arr           = new TileBase[numTileAssets];
                var end = limit < numTileAssets
                              ? limit
                              : numTileAssets;
                target.GetUsedTilesNonAlloc(arr);

                if (usingPlugin)
                {
                    for (var i = 0; i < end; i++)
                    {
                        var t = arr[i];
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                        if(t == null)
                            continue;
                        
                        if (t.GetType() == m_FilterType)
                            m_CurrentTileList.Add(t);
                    }
                }

                else if (wildcard) //ie no Type filtering //mod of tpt2b1
                {
                    for (var i = 0; i < end; i++)
                    {
                        var t = arr[i];
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                        if(t == null)
                            continue;
                        
                        if (t is not ITilePlus) //don't want to add these twice
                            m_CurrentTileList.Add(t);
                    }
                }
                else
                {
                    m_CurrentTileList.AddRange(arr.Where(t => t != null
                                                              && t is not TilePlusBase
                                                              && t.GetType() == m_FilterType));
                }
            }
            if (Config.TpPainterTileSorting != TpTileSorting.None)
                SortTileList();
            
            TilemapPaintTargetCount = m_CurrentTileList.Count;

            //now we try to get a selection. BUT we do not want to do that when in the Paint phase of
            //a MOVE operation.
            if (m_TpPainterMoveSequenceState == TpPainterMoveSequenceStates.Paint)
            {
                tpPainterContentPanel.TilesListViewSetSelection( -1 ); //no selection.
                return;
            }
            
            var currentSelection = tpPainterContentPanel.GetTilesListViewSelectionObject as TileBase;
            //if the currentSelectedObject can be found in the currentTilesList then use that as
            //the selection index
            if (currentSelection != null)
            {
                for (var ti = 0; ti < m_CurrentTileList.Count; ti++)
                {
                    if (m_CurrentTileList[ti] != currentSelection)
                        continue;
                    //else we found a match so use that
                    tpPainterContentPanel.RebuildTilesListView();
                    tpPainterContentPanel.TilesListViewSetSelection(ti);
                    return;
                }
            }
            
            tpPainterContentPanel.RebuildTilesListView();

            var count = m_CurrentTileList.Count;
            if (previousCount != count)
                tpPainterContentPanel.TilesListViewSetSelection( -1 );
            else if (count != 0)
                tpPainterContentPanel.TilesListViewSetSelection( 0 );


            void SortTileList()
            {

                if (Config.TpPainterTileSorting == TpTileSorting.Type)
                {
                    m_CurrentTileList.Sort((t0, t1) =>
                                           {
                                               if (t0 == null || t1 == null)
                                                   return 0;
                                               var type0 = t0.GetType();
                                               var type1 = t1.GetType();
                                               return type0 == type1
                                                          ? 0
                                                          : StringComparer.InvariantCulture.Compare(type0.ToString(), type1.ToString());
                                           });
                }
                else if (Config.TpPainterTileSorting == TpTileSorting.Id)
                {
                    m_CurrentTileList.Sort((t0, t1) =>
                                           {
                                               if (t0 == null || t1 == null)
                                                   return 0;
                                               var id0 = t0.GetInstanceID();
                                               var id1 = t1.GetInstanceID();
                                               return id0 == id1 ? 0 : id0 < id1 ? -1 : 1;

                                           });
                }
            }
        }


        #endregion



        #region localSelection

        /// <summary>
        /// Attempt to restore a tilemap selection.
        /// </summary>
        /// <param name="selectedMapId">The selected map identifier.</param>
        /// <param name = "notify" >notify or not when selection is changed</param>
        /// <returns>true for sucess</returns>
        private bool TryRestoreTilemapSelection(int selectedMapId, bool notify = false)
        {
            //look in the itemSource so see if we can find the same tilemap.
            var items    = tpPainterTilemapsPanel.DataSource;
            var numItems = items.Count;
            if (numItems == 0)
            {
                tabBar.ActivateToolbarButton(TpPainterTool.None, true); //added notify 12/9
                EditorClearSelection();
                return false;
            }


            var selectionIndex = -1;
            for (var i = 0; i < numItems; i++)
            {
                var item = items[i];
                if (item is not TilemapData ttd) //unlikely to fail, but need the cast to TilemapData anyway
                    continue;
                if (ttd.TargetMap.GetInstanceID() != selectedMapId) //if this map doesn't match the map in the selection 
                    continue;
                selectionIndex = i;
                break;

            }

            if (selectionIndex >= 0)
            {
                if (items[selectionIndex] is not TilemapData tilemapTarget)
                    return false;
                //is this selected tilemap the same as the current tilemapPaintTarget? If it is, don't call SetInspectorTarget because of side effects.
                if (m_TilemapPaintTarget is not null && m_TilemapPaintTarget.Valid && m_TilemapPaintTarget.TargetTilemap != null
                    && tilemapTarget.TargetMap.GetInstanceID() != m_TilemapPaintTarget.TargetTilemap.GetInstanceID()) 
                {
                    if(SetPaintTarget(tilemapTarget.TargetMap) && m_GlobalMode == GlobalMode.EditingView)
                        SetInspectorTarget(tilemapTarget.TargetMap);

                }
                else
                {
                    if (m_GlobalMode == GlobalMode.EditingView)
                        SetInspectorTarget(tilemapTarget.TargetMap);
                }
                if(notify)
                    tpPainterTilemapsPanel.SetSelection(selectionIndex);
                else
                    tpPainterTilemapsPanel.SetSelectionWithoutNotify(selectionIndex);
                return true;
            }
            else
            {
                EditorClearSelection();
                tabBar.ActivateToolbarButton(TpPainterTool.None, true); //added notify 12/9
                return false;
            }

        }

        /// <summary>
        /// Clear selection.
        /// </summary>
        private void EditorClearSelection()
        {
            //Debug.Log("Editor cleared selection");
            TpPainterMoveSequenceState = TpPainterMoveSequenceStates.None;

            m_TilemapPaintTarget              = null;
            tpPainterTilemapsPanel.SetSelectionLabel(EmptyFieldLabel);

            m_EditorSelectionLock = true;
            tpPainterTilemapsPanel?.ClearSelection();
            m_EditorSelectionLock = false;
            
            //locking-hack needed since Unity's clearselection will
            //re-call the OnEditorSelectionChange delegate. No way to do this w/o notification.

            m_CurrentTileList.Clear();
            tpPainterContentPanel.RebuildTilesListView();
            ClearClipboard();
            tpPainterContentPanel.SetAssetViewSelectionLabel(EmptyFieldLabel);
        }

        /// <summary>
        /// Resets the selections of this window.
        /// </summary>
        internal void ResetSelections(bool alsoClearTilemapsSelection = true)
        {
            if(alsoClearTilemapsSelection)
                m_TilemapPaintTarget = null;
            m_PaletteTarget      = null;
            TileTarget         = null;
            m_TilesToDisplay.Clear();
            TpPainterMoveSequenceState = TpPainterMoveSequenceStates.None;
        }

        /// <summary>
        /// Clears the clipboard.
        /// </summary>
        /// <param name="clearPaletteTileTarget">clear tileTarget if true.</param>
        internal void ClearClipboard(bool clearPaletteTileTarget = true)
        {
            //Debug.Log($"Editor cleared clipboard, clearing palette target? {clearPaletteTileTarget}");
            if(TpLibEditor.Informational)
                TpLog("Clearing clipboard");
            if (clearPaletteTileTarget)
                TileTarget = null;

            tabBar.SetPickedTileImage(null);
        }

        #endregion

        #region utils

        /// <summary>
        /// Deselect anything on the Grid Select panel
        /// </summary>
        internal void DeselectGridSelPanel()
        {
            if(!guiInitialized)
                return;
            tpPainterContentPanel.DeselectGridSelection();
        }

        internal SelectionElement? ActiveGridSelection => tpPainterContentPanel.GridSelectionElement;
        

        /// <summary>
        /// Adds a tile to the to history list and refreshes views.
        /// </summary>
        /// <param name="t">The tile to add.</param>
        internal void AddToHistory(TileBase t) //mod tpt2b1
        {
            m_HistoryStack.Insert(0,t); //insert at first position for stack-like operation.
            RefreshPalettesListView();
            TileSourceAssetScanner();
            RefreshPaletteModeTilesListView();
        }

        
        
        /// <summary>
        /// Forces a refresh of the tiles list.
        /// </summary>
        internal void ForceRefreshTilesList()
        {
            wantsRefreshTilesList = true;
        }

        internal void SetTileTarget(TileBase tile, TpPickedTileType pickType, Vector3Int tPosition, Tilemap map, bool wasPickedTile, bool sendToTabBar=true)
        {
            TileTarget = new TargetTileData(tile, tPosition, map, wasPickedTile);
            if(sendToTabBar)
                tabBar.SetPickedTileImage(tile,pickType,wasPickedTile);
        }

        internal void ActivateToolbarButton(TpPainterTool tool, bool withNotify)
        {
            tabBar.ActivateToolbarButton(tool, withNotify);
        }
        


        /// <summary>
        /// Checks prior to executing CreateGui
        /// </summary>
        /// <returns>bool.</returns>
        private bool CreateGuiChecks()
        {
            if (!m_HasControlId) //failure if this is missing.
            {
                rootVisualElement.Clear();
                rootVisualElement.Add(new Label("Fatal error: could not obtain control ID"));
                return false;

            }

            //this can happen if you open Unity and switch to a different PC/Mac/Linux app while Unity is loading,
            //Or if you're mucking about in your code editor and a domain reload occurs.
            if (!TpLibIsInitialized || GridPaintingState.instance == null)
            {
                //note cannot use the conditional version here since that one can time out.
                TpLib.DelayedCallback(this,CreateGUI, "T+P:CreateGuiWaitOnTpLib", 2000);
                rootVisualElement.Clear();
                rootVisualElement.Add(new Label($"Not ready yet...  {++m_CreateGuiIterations}"));
                return false;
            }

            TpPainterScanners.TilemapsScan();
            TpPainterScanners.PalettesScan(m_CurrentPaletteSearchString);

            return true;

        }

        /// <summary>
        /// Clears the history list.
        /// </summary>
        
        internal void ClearHistory()
        {
            m_HistoryStack.Clear();
        }


       

        /// <summary>
        /// Reinitialize this window.
        /// </summary>
        internal void ReInit()
        {
            if(TpLibEditor.Informational)
                TpLog("Reinitializing Tile+Painter...");
            TpPreviewUtility.Reset();
            TpPainterSceneView.Reset();
            TpPainterScanners.ResetTilemapScanData();
            TpPainterScanners.ResetPaletteScanData();
            TpPainterScanners.TransformAssetScanner();
            
            m_HistoryStack.RemoveAll((t) => t == null);
            
            rootVisualElement.Clear();
            rootVisualElement.Add(new Label("Wait...."));
            guiInitialized               = false;
            wantsRefreshTilesList        = false;
            m_EditorSelectionLock          = false;
            m_CurrentPaletteSearchString = string.Empty;
            m_CurrentTool                  = TpPainterTool.None;
            TpPainterMoveSequenceState = TpPainterMoveSequenceStates.None;
            m_GlobalMode                   = GlobalMode.PaintingView;
            m_PreviousGlobalMode           = GlobalMode.EditingView;
            m_PreviousTool                 = TpPainterTool.None;
            m_ToolToRestore                = TpPainterTool.None;
            TileTarget                 = null;
            m_PaletteTarget              = null;
            m_TilemapPaintTarget         = null;
            settingsChangeWatchers.Clear();
            m_FilterType                 = typeof(TileBase);
            m_FilterTag                  = ReservedTag;
            TilemapListViewItems.Clear();
            m_TilesToDisplay.Clear();
            m_CurrentTileList.Clear();
            m_CreateGuiIterations     = 0;
            tabBar                  = null;
            tpPainterTilemapsPanel  = null;
            tpPainterContentPanel   = null;
            tpPainterSettingsPanel  = null;
            tpPainterHelpPanel      = null;
            tilemapsAndContentPanel = null;
            statusAreaMiniButtons   = null;
            TpLib.DelayedCallback(this,DelayedReinitSequence,"T+P: Reinit",500); 
        }

        private void DelayedReinitSequence()
        {
            CreateGUI();
            ConditionalResetState();  //pushes another delayed, but conditional action.
        }
        
        #endregion

        
    }

}

