// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 11-04-2022
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 12-31-2022
// ***********************************************************************
// <copyright file="TpPainterSettingsPanel.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>Create the settings panel for Tile+Painter</summary>
// ***********************************************************************

using System.Globalization;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TilePlus.Editor.Painter
{
    /// <summary>
    /// TpPainterSettingsPanel creates the settings panel
    /// Implements the <see cref="VisualElement" />
    /// Implements the <see cref="TilePlus.Editor.ISettingsChangeWatcher" />
    /// </summary>
    /// <seealso cref="VisualElement" />
    /// <seealso cref="TilePlus.Editor.ISettingsChangeWatcher" />
    internal class TpPainterSettingsPanel : VisualElement, ISettingsChangeWatcher
    {
        private const string ButtonText_PlayUpdate       = "Update in Play";
        private const string ToolTip_PlayUpdate          = "Updates displays of TilePlus tile data when in Play mode.";
        private const string FieldText_MaxNumTiles       = "Max #tiles to display";
        private const string FieldText_SnappingChunkSize = "Chunk Size for Snapping";
        private const string FieldText_SnappingOrigin    = "Chunk Snapping World Origin";
        //private const string ButtonText_ShowIcons        = "Toolbars show only icons";
        private const string ToggleText_Snapping         = "Chunk Snapping";
        //private const string Tooltip_ShowIcons           = "If checked, don't show abbreviated tooltips (eg 'AO' == ALT+O) [Note: Redraws the Painter window]";
        private const string ToolTip_SnappingChunkSize   = "Grid Size:  (>= 4 and must be even number, if not even, value reduced by 1)";
        private const string ToolTip_SnappingOrigin      = "Chunk Snapping World Origin (usually just 0,0,0)";
        private const string ToolTip_SnappingMode        = "Turn on Chunk Snapping. ";
        private const string LabelText_HighlightTime     = "Highlight Time";
        private const string LabelToolTip_HighlightTime  = "Tile Highlight Time when selected a list of scene tiles.";
        private const string ToolTip_UiSize              = "Relative size of UI elements in Lists. From 14-30. Click the Refresh button (lower-left corner of window) to update.";
        private const string LabelText_UiSize            = "UI Size";
        private const string Label_MapSorting = "Tilemap Sorting";

        private const string ToolTip_MapSorting =
            "When checked, the Tilemaps list is sorted by Sorting Layer ID and then by Sorting Order within the Layer. Otherwise an Alpha-sort is used.";

        private readonly Toggle          overwriteToggle;
        private readonly Toggle          syncToggle;
        private readonly Toggle          updateToggle;
        private readonly IntegerField    chunkSize;
        private readonly Vector3IntField originField;
        private readonly TpSplitter      splitter;

        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="win">parent window</param>
        internal TpPainterSettingsPanel([NotNull] TilePlusPainterWindow win)
        {
            var           config = TilePlusPainterConfig.instance;
            style.display       = DisplayStyle.None;
            style.flexGrow      = 1;
            style.marginBottom  = 2;
            style.marginTop     = 4;
            style.alignItems    = Align.Stretch;

            name = "settings-container";
            Add(new TpHelpBox("Click the Settings button again to close this panel",
                              "settings-helpbox")
                {
                    style =
                    {
                        alignSelf = Align.Center, 
                        paddingBottom = 2
                    }
                });
            
            //add a centered header
            Add(new TpHeader("Settings","settings-header") { style = { paddingBottom = 1, alignSelf = Align.Center } });

            splitter = new TpSplitter("painter-setup-panel-splittter",
                                      "TPT.TPPAINTER.SETUP.SPLITVIEW",
                                      192,
                                      TwoPaneSplitViewOrientation.Vertical,
                                      0,
                                      SetupSplitterFix){ style = { minWidth = TilePlusPainterWindow.ViewPanesMinWidth } };
            
            Add(splitter);
            var topSplit = new VisualElement()
                           {
                               name = "setup-panel-top-of-split",
                               style   = {
                                             overflow = Overflow.Hidden, minHeight = 40f}
                           };

            //add local controls to top part
            topSplit.Add(new TpHelpBox("Tile+Painter settings (Certain settings are also controlled by the small buttons at the bottom of the window)", "settings-header")
                         {
                             style= {marginBottom = 2}
                         });
           
            var s          = "Active TileBase plugins: ";

            //this should not occur, so rescan
            if (!Application.isPlaying && TpPreviewUtility.PluginCount == 0)
                    TpPreviewUtility.Reset();

            var allPlugins = TpPreviewUtility.AllPlugins;
            
            if (allPlugins.Count == 0) //this should not occur, so rescan
                s += "None";
            else
            {
                foreach (var p in allPlugins)
                    s += $"{p.name}  ";
            }

            topSplit.Add(new TpHelpBox(s, "settings-plugins"){style={marginBottom = 2}});
            topSplit.Add(new TpSpacer(4, 20));

            var toggle = new TpToggleLeft("Overwrite Protection")
                         {
                             tooltip =
                                 "When checked, placed tiles can't be overwritten. See tooltip for LOCK icon above for more info",
                             name  = "setting-overwrite",
                             value = TilePlusConfig.instance.NoOverwriteFromPalette,
                         };
              
            toggle.RegisterValueChangedCallback(evt =>
                                                {
                                                    TilePlusConfig.instance.NoOverwriteFromPalette = evt.newValue;
                                                    
                                                });
            topSplit.Add(toggle);
            overwriteToggle = toggle;

            toggle = new TpToggleLeft(Label_MapSorting)
            {
                tooltip = ToolTip_MapSorting,
                name = "setting-map-sorting",
                value = config.TpPainterTilemapSorting
            };
            toggle.RegisterValueChangedCallback(evt =>
            {
                config.TpPainterTilemapSorting = evt.newValue;
                win.RebuildTilemapsList();
                
            });
            topSplit.Add(toggle);

            toggle = new TpToggleLeft("Sync Selection")
                     {
                         tooltip =
                             "When checked, this window will select a tilemap in the heirarchy when you click it in the window's Tilemap list, and when you select a Tilemap in the heirarchy with the mouse the selection in this window's Tilemap list will match.",
                         name  = "setting-selection-sync",
                         value = config.TpPainterSyncSelection
                     };
            toggle.RegisterValueChangedCallback(evt => { config.TpPainterSyncSelection = evt.newValue; });
            topSplit.Add(toggle);
            syncToggle = toggle;

            var marqColor = new ColorField("Scene Marquee Color")
                            {
                                tooltip = "Sets the color of the box at the cursor position",
                                value   = config.TpPainterMarqueeColor,
                                style =
                                {
                                    flexGrow = 0
                                }
                            };
            marqColor.RegisterValueChangedCallback(evt => { config.TpPainterMarqueeColor = evt.newValue; });
            topSplit.Add(marqColor);

            var textColor = new ColorField("Scene Text Color")
                            {
                                tooltip = "Sets the color of the text at the cursor position",
                                value   = config.TpPainterSceneTextColor,
                                style =
                                {
                                    flexGrow = 0
                                }
                            };
            textColor.RegisterValueChangedCallback(evt =>
                                                   {
                                                       config.TpPainterSceneTextColor = evt.newValue;
                                                       TpPainterSceneView.ReinitializeGuiContent();
                                                   });
            topSplit.Add(textColor);

            var highlightTime = new DropdownField
                                {
                                    label   = LabelText_HighlightTime,
                                    tooltip = LabelToolTip_HighlightTime,
                                    choices =  new() { "1", "2", "3", "4", "5" },
                                    value   = TilePlusConfig.instance.TileHighlightTime.ToString(CultureInfo.InvariantCulture)
                                };

            highlightTime.RegisterValueChangedCallback(evt =>
                                                       {
                                                           if (float.TryParse(evt.newValue, out var result))
                                                               TilePlusConfig.instance.TileHighlightTime = result;
                                                       });

            topSplit.Add(highlightTime);
            
            
            toggle = new TpToggleLeft(ButtonText_PlayUpdate)
                     {
                         tooltip = ToolTip_PlayUpdate,
                         name  = "setting-auto-refresh",
                         value = config.PainterAutoRefresh
                     };
            toggle.RegisterValueChangedCallback(evt => { config.PainterAutoRefresh = evt.newValue; });
            topSplit.Add(toggle);
            updateToggle = toggle;

            //topSplit.Add(new TpSpacer(10, 10));
            topSplit.Add(new TpHelpBox(ToolTip_UiSize,"ui-size-tip"));
            //topSplit.Add(new TpSpacer(10, 10));
            var uiSize = new FloatField(LabelText_UiSize, 2) {isDelayed = true,
                tooltip = ToolTip_UiSize, name = "setting-ui-size", value = config.PainterListItemHeight };
            uiSize.RegisterValueChangedCallback((f) =>
                                                {
                                                    config.PainterListItemHeight = f.newValue;
                                                    TpLib.DelayedCallback(win, 
                                                                          (() => uiSize.value = config.PainterListItemHeight),
                                                                          "T+P settings update list item height ui",1000);
                                                });  
            topSplit.Add(uiSize);
            topSplit.Add(new TpSpacer(10, 10));
            var textField = new TextField(FieldText_MaxNumTiles,
                                          4,
                                          false,
                                          false,
                                          'x')
                            {
                                tooltip =
                                    "Defines maximum number of tiles in any list of tiles. Avoids extremely long lists for performance."
                            };
            textField.SetValueWithoutNotify(config.MaxTilesForViewers.ToString());
            textField.RegisterValueChangedCallback(evt =>
                                                   {
                                                       if (int.TryParse(evt.newValue, out var num))
                                                           config.MaxTilesForViewers = num;
                                                   });
            topSplit.Add(textField);


            /*toggle = new TpToggleLeft(ButtonText_ShowIcons)
                     {
                         tooltip = Tooltip_ShowIcons,
                         name    = "setting-show-icons",
                         value   = config.PainterShowOnlyIcons
                     };
            toggle.RegisterValueChangedCallback(evt =>
                                                {
                                                    config.PainterShowOnlyIcons = evt.newValue;
                                                    win.TabBar.ShowTooltips(evt.newValue); 
                                                    evt.StopImmediatePropagation();
                                                    TpLib.DelayedCallback(win,()=> TilePlusPainterWindow.instance.ReInit(),"Reinit T+P after change icons->tips or vice versa",50);
                                                });
            topSplit.Add(toggle);*/
            
            var box = new TpListBoxItem("chunk-group",Color.yellow){style = {flexDirection = FlexDirection.Column, alignItems = Align.FlexStart, alignContent = Align.Stretch, flexGrow = 0}};
            topSplit.Add(box);
            box.Add(new TpHelpBox("!!Read the Painter user guide before turning the below toggle ON!!","warning"){tooltip = "No kidding!! You'll be confused if you don't!!"});
            
            var authoringOn = config.PainterFabAuthoringMode;
            var snappingToggle = new TpToggleLeft(ToggleText_Snapping)
                                 {
                                     tooltip = ToolTip_SnappingMode,
                                     name    = "setting-fab-authoring",
                                     value   = authoringOn
                                 };
            snappingToggle.RegisterValueChangedCallback(evt =>
                                                {
                                                    config.PainterFabAuthoringMode = evt.newValue;
                                                    evt.StopImmediatePropagation();
                                                });
            box.Add(snappingToggle);

            chunkSize = new IntegerField(FieldText_SnappingChunkSize, 5)
                            {
                                isDelayed = true,
                                tooltip = ToolTip_SnappingChunkSize,
                                name    = "setting-fab-chunksize",
                                value   = config.PainterFabAuthoringChunkSize,
                                style = {flexGrow = 1}
                            };
            chunkSize.RegisterValueChangedCallback(evt =>
                                                   {
                                                       var val     = evt.newValue;
                                                       var refresh = false;
                                                       if (val < 4)
                                                       {
                                                           val     = 4;
                                                           refresh = true;
                                                       }

                                                       if (val % 2 != 0)
                                                       {
                                                           val--;
                                                           refresh          = true;
                                                       }

                                                       config.PainterFabAuthoringChunkSize = val;
                                                       if (refresh)
                                                       {
                                                           TpLib.DelayedCallback(win, () => chunkSize.SetValueWithoutNotify(val),
                                                                                 "TPV+ChunkSizeUpdate", 500);
                                                       }

                                                       evt.StopImmediatePropagation();
                                                   });
            box.Add(chunkSize);
            chunkSize.SetEnabled(!authoringOn);
            

            originField = new Vector3IntField(FieldText_SnappingOrigin) { tooltip = ToolTip_SnappingOrigin, name = "setting-fab-auth-origin", value = config.FabAuthWorldOrigin, style = {flexGrow = .5f} };
            
            originField.RegisterValueChangedCallback(evt =>
                                                     {
                                                         config.FabAuthWorldOrigin = evt.newValue;
                                                         evt.StopImmediatePropagation();
                                                     });
            box.Add(originField);
            originField.SetEnabled(!authoringOn);
            
            topSplit.Add(new TpSpacer(16, 20){style = {flexGrow = 0}});

            var resetButton = new Button(() => TilePlusPainterConfig.instance.Reset())
            {
                tooltip = "Reset Painter configuration to defaults.",
                name = "reset_button", text = "Reset Painter Settings",  style = {alignSelf = Align.FlexStart}
            };
            topSplit.Add(resetButton);
            topSplit.Add(new TpSpacer(4, 20){style = {flexGrow = 0} });
            
            
            splitter.Add(topSplit);

            var imgui = new IMGUIContainer(() =>
                                           {
                                               //ImGuiTileEditor.DrawUILine(4, 4);
                                               //Add(new TpSpacer(4, 20));
                                               EditorGUILayout.HelpBox("Global TilePlus Toolkit Settings", MessageType.None);
                                               //Add(new TpSpacer(2, 20));
                                               TilePlusConfigView.ConfigOnGUI();
                                           }){style = {minHeight = 25f}};
            imgui.style.whiteSpace = new StyleEnum<WhiteSpace>(WhiteSpace.Normal);
            splitter.Add(imgui);
            Add(new TpSpacer(2,20));
            Add(new TpHelpBox("Top section: Painter settings, Bottom: TPToolkit global", "settings-helpbox"));
            Add(new TpSpacer(2, 20));

            
        }


        /// <inheritdoc />
        public void OnSettingsChange(string change, [NotNull] ConfigChangeInfo changes)
        {
            var newValue = changes.m_NewValue;
            if(newValue is not bool b)
                return;
            if (change == TPC_SettingThatChanged.NoOverwriteFromPalette.ToString() && overwriteToggle.value != b)
                    overwriteToggle.value = b;
            else if (change == TPP_SettingThatChanged.SyncSelection.ToString()   && syncToggle.value != b)
                syncToggle.value = TilePlusPainterConfig.instance.TpPainterSyncSelection;
            else if (change == TPP_SettingThatChanged.UpdateInPlay.ToString() && updateToggle.value != b)
                updateToggle.value = TilePlusPainterConfig.instance.PainterAutoRefresh;
            else if (change == TPP_SettingThatChanged.FabAuthoring.ToString())
            {
                chunkSize.SetEnabled(!b);
                originField.SetEnabled(!b);
            }
            
                
        }


       
        /// <summary>
        /// Adjust a splitter in a vertical splitview
        /// </summary>
        /// <param name="evt">The event</param>
        private void SetupSplitterFix([NotNull] GeometryChangedEvent evt)
        {
            var handle = splitter.Q<VisualElement>("unity-dragline-anchor");
            handle.style.width           = style.width;
            handle.style.height          = TilePlusPainterWindow.SplitterSize;
            handle.style.backgroundColor = Color.red;
            evt.StopImmediatePropagation();

        }
        
    }
}
