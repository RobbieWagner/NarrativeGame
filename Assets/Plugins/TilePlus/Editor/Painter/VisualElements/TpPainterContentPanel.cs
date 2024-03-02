// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 12-03-2022
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 12-31-2022
// ***********************************************************************
// <copyright file="TpPainterContentPanel.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>Panel for the center and right columns</summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static TilePlus.Editor.TpIconLib;
using Object = UnityEngine.Object;


namespace TilePlus.Editor.Painter
{
    /// <summary>
    /// Builds the UI for the right-half of the outer splitview (ie the center and right columns)
    /// This splitview's left side contains a vertical splitview which shows either the list of
    /// palettes and a control region OR a lis tof tiles and a control region.
    /// The splitview's right side contains a vertical splitview which shows a palette-item in
    /// its top part with a 'brush inspector' in its bottom part (palette mode). In tilemap
    /// mode this top part is collapsed and the bottom part occupies the entire pane: shows the
    /// 'Selection Inspector'
    /// </summary>
    internal class TpPainterContentPanel : VisualElement, ISettingsChangeWatcher
    {
        #region VisualElementReferences
        /// <summary>
        /// The content panel split view
        /// </summary>
        private readonly TpSplitter    contentPanelSplitView;
        /// <summary>
        /// The asset view splitter
        /// </summary>
        private readonly TpSplitter    assetViewSplitter;
        /// <summary>
        /// The palettes ListView
        /// </summary>
        private TpListView    palettesListView;

        //Asset view (center pane)
        //the lists for the center column. Only one is visible, depending on Palette or Tilemaps mode.
        /// <summary>
        /// The Edit mode asset ListView
        /// </summary>
        private TpListView editModeAssetListView;
       
        /// <summary>
        /// The asset view data panel (Center pane)
        /// </summary>
        private VisualElement assetViewDataPanel;
        /// <summary>
        /// The asset view tile options panel
        /// </summary>
        private VisualElement assetViewTileOptions;
        /// <summary>
        /// The asset view palette options panel
        /// </summary>
        private VisualElement assetViewPaletteOptions;
        /// <summary>
        /// The asset view header label
        /// </summary>
        private Label         assetViewHeaderLabel;
        /// <summary>
        /// The asset view selection label
        /// </summary>
        private Label         assetViewSelectionLabel;
        /// <summary>
        /// The asset view controls scroll view
        /// </summary>
        private ScrollView    assetViewControlsScrollView;
        /// <summary>
        /// The tilemaps list selection needed help box
        /// </summary>
        private TpHelpBox assetViewMapSelectionNeededHelpBox;

        
        /// brush inspector view (right pane)
        /// <summary>
        /// container for Paint mode rightmost column
        /// </summary>
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private VisualElement paintModeTileView;

        
        
        /// <summary>
        /// The inspector list selection label
        /// </summary>
        private Label         brushInspectorListSelectionLabel;
       
        /// <summary>
        /// The tiles information container
        /// </summary>
        private VisualElement    brushInpectorTileInfoContainer;
        /// <summary>
        /// The inspector view splitter
        /// </summary>
        private TpSplitter    brushInspectorViewSplitter;
        /// <summary>
        /// The Paint mode tiles ListView
        /// </summary>
        private TpListView brushInspectorTilesListView;
        /// <summary>
        /// The tile display GUI
        /// </summary>
        private IMGUIContainer brushInspectorGui;
        
        /// <summary>
        /// Edit mode view
        /// </summary>
        private readonly VisualElement editModeTileView;
       
        /// <summary>
        /// The tile display GUI
        /// </summary>
        private IMGUIContainer selectionInspectorGui;

        /// <summary>
        /// Grid selections GUI
        /// </summary>
        private readonly TpPainterGridSelPanel gridSelectionPanel;
        
        
        #endregion

        #region privateFieldsProperties
        /// <summary>
        /// This window
        /// </summary>
        // ReSharper disable once MemberCanBeMadeStatic.Local
        [NotNull]
        private  TilePlusPainterWindow PainterWindow => TilePlusPainterWindow.instance!;
        /// <summary>
        /// The list item height
        /// </summary>
        private readonly float listItemHeight;
        /// <summary>
        /// The view panes minimum width
        /// </summary>
        private readonly float                 viewPanesMinWidth;
        /// <summary>
        /// The search field's textfield outline radius
        /// </summary>
        private const    float                 SearchFieldRadius = 4;

        /// <summary>
        /// Get a size for the palette target image.
        /// </summary>
        /// <value>The size of the palette target image.</value>
        private float PaletteTargetImageSize => 30f + PaletteTargetItemBorderWidth;
        /// <summary>
        /// The palette target item border width
        /// </summary>
        private const float PaletteTargetItemBorderWidth = 4;

        //a list of palettes
        /// <summary>
        /// The palettes
        /// </summary>
        private readonly List<PaletteListItem> palettes = new();


        /// <summary>
        /// The history palette item
        /// </summary>
        /// <remarks>place-holder for the HistoryStack 'palette' : note that this use of the parameterless ctor makes this a history Palette.</remarks>
        private readonly PaletteListItem historyPaletteItem = new();

        /// <summary>
        /// Ref for systtem configuration.
        /// </summary>
        /// <value>The configuration.</value>
        private TilePlusPainterConfig Config => TilePlusPainterConfig.instance;
        #endregion
        
        #region ctor

        /// <summary>
        /// this is a panel with all the UI for the right-hand side of the main splitview.
        /// </summary>
        /// <param name="viewPanesMinWidth">Minimum width dimension for view panes</param>
        internal TpPainterContentPanel(float viewPanesMinWidth)
        {
            listItemHeight    = Config.PainterListItemHeight;
            this.viewPanesMinWidth = viewPanesMinWidth;

            gridSelectionPanel = new TpPainterGridSelPanel {style = {display = DisplayStyle.None}};
            Add(gridSelectionPanel);
            
            contentPanelSplitView = new TpSplitter("painter-splitview-inner",
                         "TPT.TPPAINTER.SPLITVIEW.RIGHT", 100, TwoPaneSplitViewOrientation.Horizontal, 0);
            
            var splitterHandle = contentPanelSplitView.Q<VisualElement>("unity-dragline-anchor");
            splitterHandle.style.backgroundColor = Color.red;
            Add(contentPanelSplitView);

            /*left side of this splitview also a splitView: AssetView.
            
            This has two major panels: Palette/Tilemap (PTV) View and Tile View (TV) 
            
            PTV has two panels: one for each of the two Global modes:  PaintingView and EditingView
            
            PaintingView is a list of palettes/tilefabs|Chunks/History with an area for options at the bottom
            this one has a vertical orientation.
            the fixed pane is a container. 
               label - title
               label - context
               listView with names of palettes, tilefabg/chunks, or History
            EditingView
            a list of tiles in a tilemap selected from column 1.
                label - title
                label - context
                list view with tiles
            */


            //the vertical split view gets added to this container.
            assetViewSplitter = new TpSplitter("painter-splitview-assetinfo",
                                                 "TPT.TPPAINTER.SPLITVIEW.ASSETS",
                                                 100,
                                                 TwoPaneSplitViewOrientation.Vertical,
                                                 1, SourceSplitterFix) { style = { minWidth = viewPanesMinWidth } };

            //this is the top part of this split
            //in PAINT mode this displays the list of Palettes, Tilefabs|Chunks, and the History List
            //in EDIT mode this displays the list of tiles from the tilemap chosen in the left column of the window.
            //Paint mode is what appears when this window opens.
            assetViewSplitter.Add(BuildAssetViewDataPanel()); //add first panel to palette view splitter

            //the bottom part of the palette view is a small options panel
            //this has two different types of info: for palettes view or for tilemaps view
            assetViewSplitter.Add(BuildAssetViewControls());
            
            contentPanelSplitView.Add(assetViewSplitter); //add source view controls to inner splitview

            //The rightmost panel holds two 'Inspector' subpanels,
            //only one of which is available at a time, depending on Global mode
            var inspectorPanel = new VisualElement {name = "inspector-panel", style = { flexGrow = 1 } }; 
            contentPanelSplitView.Add(inspectorPanel);
            //For PAINT mode, right side of InnerSplitView is itself a splitview, this one has a vertical orientation.
            //the fixed pane is a container 
            //   label - title
            //   label - context
            //   listView (palette content display)
            // bottom of split is 'brush' inspector
            inspectorPanel.Add( paintModeTileView = BuildBrushInspector());
            //in EDIT mode, right side of InnerSplitView is a SelectionInspector
            // label - content
            // 'Selection' Inspector
            editModeTileView = BuildSelectionInspector();
            editModeTileView.style.display = DisplayStyle.None; //initially OFF
            inspectorPanel.Add(editModeTileView);
        }
        #endregion
        
        #region access

        /// <summary>
        /// Set the content panel for Tilemaps or Palette
        /// </summary>
        /// <param name="mode">Palette or Tilemaps mode from GlobalMode enum</param>
        internal void SetDisplayState(GlobalMode mode)
        {
            if (mode == GlobalMode.GridSelView)
            {
                contentPanelSplitView.style.display = DisplayStyle.None;
                gridSelectionPanel.style.display  = DisplayStyle.Flex;
                return;
            }

            gridSelectionPanel.style.display  = DisplayStyle.None;
            
            if (mode == GlobalMode.EditingView)
            {
                paintModeTileView.style.display = DisplayStyle.None;
                ShowPalettesListView(false);
                ShowSourceViewPaletteOptions(false);
                ShowTilesListView(true);
                ShowAssetViewTileOptions(true);
                SetAssetViewHeaderLabel("Tiles");
                ShowTilemapsListSelectionNeededHelpBox(false);
                SetAssetViewSelectionLabel(TilePlusPainterWindow.EmptyFieldLabel);
                editModeTileView.style.display = DisplayStyle.Flex;
            }
            else
            {
                editModeTileView.style.display = DisplayStyle.None;
                ShowPalettesListView(true);
                ShowSourceViewPaletteOptions(true);
                ShowTilesListView(false);
                ShowAssetViewTileOptions(false);
                ShowTilemapsListSelectionNeededHelpBox(false);
                SetAssetViewHeaderLabel("Painting Source");
                SetAssetViewSelectionLabel(PainterWindow.m_PaletteTarget != null
                    ? PainterWindow.m_PaletteTarget.ItemName
                    : TilePlusPainterWindow.EmptyFieldLabel);
                paintModeTileView.style.display = DisplayStyle.Flex;
                
                
                //if the tiles list has a selection, use that for the brush inspector target. Otherwise, null this.
                if(brushInspectorTilesListView.selectedItem == null)
                    PainterWindow.TileTarget = null;
                else
                    OnBrushInspectorTilesListViewSelectionChange(new[]{ brushInspectorTilesListView.selectedItem});

            }
            contentPanelSplitView.style.display = DisplayStyle.Flex;


        }
        

        /// <summary>
        /// used to enable or disable the entire content panel when the MOVE tabbar function is selected.
        /// </summary>
        /// <param name="enable">true/false to enable/disable</param>
        internal void EnableContentPanelSplitView(bool enable)
        {
            contentPanelSplitView.SetEnabled(enable);
        }

        /// <summary>
        /// used to hide or unhide the list of palettes
        /// </summary>
        /// <param name="show">true/false to unhide/hide</param>
        private void ShowPalettesListView(bool show)
        {
            palettesListView.style.display = show
                                                ? DisplayStyle.Flex
                                                : DisplayStyle.None;
        }
        
        /// <summary>
        /// used to hide or unhide the list of palette options
        /// </summary>
        /// <param name="show">true/false to unhide/hide</param>
        private void ShowSourceViewPaletteOptions(bool show)
        {
            assetViewPaletteOptions.style.display = show
                                                ? DisplayStyle.Flex
                                                : DisplayStyle.None;
        }

        /// <summary>
        /// used to show or unhide the tiles list view in TileMap mode.
        /// </summary>
        /// <param name="show"></param>
        private void ShowTilesListView( bool show)
        {
            editModeAssetListView.style.display = show
                                                ? DisplayStyle.Flex
                                                : DisplayStyle.None;
        }

        /// <summary>
        /// Set the selection for the Tiles list
        /// </summary>
        /// <param name="index">selection index</param>
        internal void TilesListViewSetSelection(int index)
        {
            editModeAssetListView.SetSelection(index);
        }
        
        /// <summary>
        /// Rebuild the tiles list view
        /// </summary>
        internal void RebuildTilesListView()
        {
            editModeAssetListView.ClearSelection();
            editModeAssetListView.Rebuild();

            

        }

        

        /// <summary>
        /// Shows or hides the asset view tile options panel.
        /// </summary>
        /// <param name="show">The show.</param>
        private void ShowAssetViewTileOptions( bool show)
        {
            assetViewTileOptions.style.display = show
                                                                     ? DisplayStyle.Flex
                                                                     : DisplayStyle.None;
        }

        /// <summary>
        /// Rebuild the palettes list view
        /// </summary>
        internal void RebuildPalettesListView()
        {
            palettes.Clear();
            palettes.Add(historyPaletteItem); 
            palettes.AddRange(TpPainterScanners.CurrentPalettes);
            assetViewSelectionLabel.text = TilePlusPainterWindow.EmptyFieldLabel;
            palettesListView.Rebuild();

            brushInspectorTilesListView.SetSelectionWithoutNotify(new[] { -1 });
            brushInspectorListSelectionLabel.text = TilePlusPainterWindow.EmptyFieldLabel;
            
        }


        /// <summary>
        /// refresh the Palettes list in Palettes mode
        /// </summary>
        internal void RefreshPalettesListView()
        {
            palettes.Clear();
            palettes.Add(historyPaletteItem); 
            palettes.AddRange(TpPainterScanners.CurrentPalettes);
            palettesListView.Rebuild();
        }

        /// <summary>
        /// Refresh the Tiles list for Palette mode.
        /// </summary>
        internal void RefreshPaletteModeTilesListView()
        {
            brushInspectorTilesListView.Rebuild();
        }

        /// <summary>
        /// Show a message that the user needs to make a tilemap selection in the leftmost column.
        /// </summary>
        /// <param name="show">true to show or false to hide</param>
        internal void ShowTilemapsListSelectionNeededHelpBox(bool show)
        {
            if (PainterWindow.GlobalMode == GlobalMode.PaintingView)
                show = false;
            assetViewMapSelectionNeededHelpBox.style.display = show
                                                                   ? DisplayStyle.Flex
                                                                   : DisplayStyle.None;
        }

        /// <summary>
        /// Sets the asset view (center column) header label.
        /// </summary>
        /// <param name="text">text to display.</param>
        private void SetAssetViewHeaderLabel(string text)
        {
            assetViewHeaderLabel.text = text;
        }

        
        /// <summary>
        /// Set the selection label for the center column
        /// </summary>
        /// <param name="text">text to display</param>
        internal void SetAssetViewSelectionLabel(string text)
        {
            assetViewSelectionLabel.text = text;
        }

        /// <summary>
        /// Sets the Virtualization mode for the Paint Inspector (RH column top element in paint mode)
        ///  </summary>
        /// <param name="fixedHeight">if true, use fixed height. Must have been set in constructor</param>
        internal void SetVirtualizationForPaintInspector(bool fixedHeight)
        {
            brushInspectorTilesListView.SetVirtualizationMethod(fixedHeight);
        }

        internal void SetScrollModeForPaintInspector(bool h, bool v)
        {
            brushInspectorTilesListView.ScrollerControl(h,v);
        }
        
        
        /// <summary>
        /// Repaint the IMGUI window
        /// </summary>
        internal void RepaintImgui(GlobalMode mode)
        {
            if(mode == GlobalMode.PaintingView)
                brushInspectorGui.MarkDirtyRepaint();
            else
                selectionInspectorGui.MarkDirtyRepaint();
        }

        public void AddGridSelection(BoundsInt bounds, bool silent = false)
        {
            gridSelectionPanel.AddGridSelection(bounds,silent);
        }


        public void AddGridSelection(Object target, BoundsInt bounds)
        {
            gridSelectionPanel.AddGridSelection(target,bounds);
        }
        
        public void DeselectGridSelection()
        {
            gridSelectionPanel.Deselect();
        }

        [CanBeNull]
        public SelectionElement GridSelectionElement => gridSelectionPanel.m_ActiveGridSelection;

        /// <summary>
        /// Is this set of UIElements properly set up!
        /// </summary>
        public bool Valid =>
            assetViewSplitter != null
            && contentPanelSplitView != null
            && brushInspectorViewSplitter != null
            && assetViewSplitter.Valid
            && contentPanelSplitView.Valid
            && brushInspectorViewSplitter.Valid;
        
        /// <summary>
        /// property to obtain the selected item from the tiles list
        /// </summary>
        internal object GetTilesListViewSelectionObject => editModeAssetListView.selectedItem;


        /// <summary>
        /// The current selection index for the tiles list in center column in Edit mode
        /// </summary>
        internal int EditModeAssetSelectionIndex => editModeAssetListView.selectedIndex;     
        /// <summary>
        /// The current selection index for the source (palette)  list in center column in Psint mode
        /// </summary>
        internal int PaintModeAssetSelectionIndex => palettesListView.selectedIndex;

        internal int PaintModeTilesListViewSelectionIndex => brushInspectorTilesListView.selectedIndex;
        
        
        #endregion
        
        #region events
        
        /// <inheritdoc />
        public void OnSettingsChange(string change, ConfigChangeInfo _)
        {
            if (PainterWindow.GlobalMode == GlobalMode.EditingView &&  change == TPP_SettingThatChanged.MaxTilesInViewer.ToString())
            {
                if (PainterWindow.m_TilemapPaintTarget is { Valid: true } && PainterWindow.m_TilemapPaintTarget.TargetTilemap != null)
                {
                    PainterWindow.SetInspectorTarget(PainterWindow.m_TilemapPaintTarget.TargetTilemap);
                    RebuildTilesListView();
                }
                else
                {
                    PainterWindow.SetInspectorTarget(null);
                    RebuildTilesListView();
                }

            }
            
                
        }

        /// <summary>
        ///only used in PAINT global mode. Entered after a click in the RIGHT column in PAINT mode.
        /// </summary>
        /// <param name="objs">selected item from list</param>
        private void OnBrushInspectorTilesListViewSelectionChange([CanBeNull] IEnumerable<object> objs)
        {
            if (PainterWindow.DiscardListSelectionEvents || objs == null)
                return;

            using var enumerator = objs.GetEnumerator();
            if (!enumerator.MoveNext())
                return;

            if (enumerator.Current is not TargetTileData ptd)
                return;

            SelectBrushInspectorTarget(ptd);

        }

        /// <summary>
        /// Set the inspector target (Rightmost column) in paint mode.
        /// </summary>
        /// <param name="itemIndex">index of the item</param>
        internal void SelectBrushInspectorTarget(int itemIndex)
        {
            if(itemIndex < brushInspectorTilesListView.itemsSource.Count)
                SelectBrushInspectorTarget((TargetTileData)brushInspectorTilesListView.itemsSource[itemIndex],false);
        }
        
        
        private void SelectBrushInspectorTarget(TargetTileData ptd, bool changeTool = true)
        {
            PainterWindow.TileTarget = ptd; //the tile we want to paint
            
            //note that this has to be prior to changing the tab bar picked tile image or the handler OnMainToolbarChanged will reset the image.
            if (changeTool && PainterWindow is { GlobalMode: GlobalMode.PaintingView, PaintingAllowed: true } )
                PainterWindow.TabBar.ActivateToolbarButton(TpPainterTool.Paint, true); //change to paint tool with notification so context changes.  

            switch (ptd.ItemVariety)
            {
                case TargetTileData.Variety.TileItem when ptd.Tile != null: 
                    
                    PainterWindow.TabBar.SetPickedTileImage(ptd.Valid ? ptd.Tile : null, TpPickedTileType.Tile);
                    
                    brushInspectorListSelectionLabel.text = ptd.Valid ? ptd.Tile.name : "NULL tile";
                    break;
                
                case TargetTileData.Variety.BundleItem when ptd.Bundle != null:
                    PainterWindow.TabBar.SetPickedTileImage(null, TpPickedTileType.Bundle,  true);
                    brushInspectorListSelectionLabel.text = ptd.Bundle.name;
                    break;
                
                case TargetTileData.Variety.TileFabItem when ptd.TileFab != null:
                    PainterWindow.TabBar.SetPickedTileImage( null, TpPickedTileType.TileFab, true);
                    brushInspectorListSelectionLabel.text = ptd.TileFab.name;
                    break;
            }

        }

        /// <summary>
        /// Selection change for Palette list view.
        /// This is the Center column in PAINT mode
        /// </summary>
        /// <param name="objs">The objs.</param>
        private void OnPaletteListViewSelectionChange([CanBeNull] IEnumerable<object> objs)
        {
            if (PainterWindow.DiscardListSelectionEvents || objs == null)
                return;

            using var enumerator = objs.GetEnumerator();
            if (!enumerator.MoveNext())
                return;

            if (enumerator.Current is not PaletteListItem item)
                return;
            
            SelectPaletteOrOtherSource(item);
        }

        internal void SelectPaletteOrOtherSource(int itemIndex)
        {
            if(itemIndex < palettesListView.itemsSource.Count)
                SelectPaletteOrOtherSource((PaletteListItem)palettesListView.itemsSource[itemIndex]);
        }
        
        internal void SelectPaletteOrOtherSource(PaletteListItem item)
        {
            PainterWindow.m_PaletteTarget = item; //the palette we want to use

            assetViewSelectionLabel.text = item.ItemName;

            if (item.ItemType is TpPaletteListItemType.Palette or TpPaletteListItemType.History)
            {
                brushInspectorTilesListView.SetSelectionWithoutNotify(new[] { -1 });
                brushInspectorListSelectionLabel.text = TilePlusPainterWindow.EmptyFieldLabel;
                PainterWindow.ClearClipboard();
                PainterWindow.TileSourceAssetScanner();
            }
            else
            {
                PainterWindow.ClearClipboard();
                PainterWindow.TileSourceAssetScanner();
                //in 2022.2 ListView.SetSelection no longer invokes selection-changed callback when selecting the same
                //item. So now have to add SetSelection(-1).
                brushInspectorTilesListView.SetSelection(-1);
                brushInspectorTilesListView.SetSelection(0);
            }

            brushInspectorTilesListView.RefreshItems();
        }


        
        /// <summary>
        /// In EDIT mode this is called when DBL clicking on a Tile in the center column.
        /// Unlike OnTilesListViewSelectionChange this just checks to see
        /// if the selection is the same as before, and if it is the highlight is shown.
        /// Basically a convenience function
        /// </summary>
        /// <param name="objs">selected items</param>
        private void EditModeAssetListViewOnitemsChosen([CanBeNull] IEnumerable<object> objs)
        {
            var target = PainterWindow.TileTarget;
            if (objs == null)
                return;
            var l = objs.ToArray();
            if(l.Length == 0     
                    || target is not { Valid: true }  //invalid current tile
                || target.ItemVariety != TargetTileData.Variety.TileItem //or not a tile (ie it's a fab or bundle)
                || !target.IsTilePlusBase //or not a TilePlusBase
                || PainterWindow.DiscardListSelectionEvents)
                return;

            var tile = l[0] as TilePlusBase;
            if (tile == null)
                return;
            if (tile == target.Tile)
                TpEditorUtilities.MakeHighlight(tile, TilePlusConfig.instance.TileHighlightTime);
        }


        /// <summary>
        ///in EDIT mode, this is called when you click on a tile in the center column. 
        ///NOTE that here we know implicitly what the tilemap is as it must have been selected in the left column.
        /// /// </summary>
        /// <param name="objs">The selected item.</param>
        private void OnTilesListViewSelectionChange([CanBeNull] IEnumerable<object> objs)
        {
            if (PainterWindow.DiscardListSelectionEvents || objs == null)
                return;

            using var enumerator = objs.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                assetViewSelectionLabel.text = TilePlusPainterWindow.EmptyFieldLabel;
                PainterWindow.TileTarget = null;
                return;
            }

            var current = enumerator.Current;
            if (current == null)
            {
                assetViewSelectionLabel.text = TilePlusPainterWindow.EmptyFieldLabel;
                PainterWindow.TileTarget = null;
                return;
            }

            UseTileSelection(current);
        }


        internal void SelectTile(int index)
        {
            if(index < editModeAssetListView.itemsSource.Count)
                UseTileSelection(editModeAssetListView.itemsSource[index]);
        }
        internal void UseTileSelection(object selection)
        {
            TpLibEditor.OnSelectionChanged(); //this cancels any marquees and/or lines
            if (selection is TilePlusBase tpb)
            {
                if (tpb.ParentTilemap == null)
                    return;
                var tname = tpb.TileName;
                var dex = tname.IndexOf('(');
                if (dex != -1)
                    tname = tname.Substring(0, dex);
                assetViewSelectionLabel.text = tname;
                PainterWindow.TileTarget = new TargetTileData(tpb, tpb.TileGridPosition, tpb.ParentTilemap);
                TpEditorUtilities.MakeHighlight(tpb, TilePlusConfig.instance.TileHighlightTime);
            }
            else if (selection is Tile tile && PainterWindow.m_TilemapPaintTarget != null)
            {
                assetViewSelectionLabel.text = tile.name;
                var map = PainterWindow.m_TilemapPaintTarget.TargetTilemap;
                PainterWindow.TileTarget = new TargetTileData(tile, Vector3Int.zero, map);
            }
            //must be TileBase  NOTE that anything subclassed from TileBase needs a TpPainterPlugin
            else if (selection is TileBase ti && PainterWindow.m_TilemapPaintTarget != null)
            {
                assetViewSelectionLabel.text = ti.name;
                var map = PainterWindow.m_TilemapPaintTarget.TargetTilemap;
                PainterWindow.TileTarget = new TargetTileData(ti, Vector3Int.zero, map);
            }
        }
        
        #endregion
        
        #region builders
        
        /// <summary>
        /// the center column display has a list of tiles or palettes
        /// </summary>
        /// <returns></returns>
        [NotNull]
        private VisualElement BuildAssetViewDataPanel()
        {
            
            palettes.Clear();
            palettes.Add(historyPaletteItem); 
            palettes.AddRange(TpPainterScanners.CurrentPalettes);
            //in PALETTE mode this displays the list of Palettes, Chunks/TileFabs, and History
            palettesListView = new TpListView(palettes,
                                               listItemHeight,
                                               true,
                                               MakePaletteListItem,
                                               BindPaletteListItem);

            palettesListView.itemsChosen      += OnPaletteListViewSelectionChange;
            palettesListView.selectionChanged += OnPaletteListViewSelectionChange;
            palettesListView.style.flexGrow   =  1;
            palettesListView.style.minWidth   =  viewPanesMinWidth;
            palettesListView.name             =  "palette-list-view";

            //Palettes View


            //at the top of the list is a label for Palettes or Tiles, and a second label for the selection's name.
            assetViewDataPanel = new VisualElement { name = "source-view-outer-container", style = { minWidth = viewPanesMinWidth } };

            assetViewHeaderLabel = new Label("Painting Source")
                                    {
                                        style =
                                        {
                                            alignSelf         = Align.Center, borderLeftWidth = 4, borderRightWidth = 2, borderTopWidth = 2,
                                            borderBottomWidth = 2
                                        }
                                    };

            assetViewDataPanel.Add(assetViewHeaderLabel);
            assetViewSelectionLabel = new Label(TilePlusPainterWindow.EmptyFieldLabel)
                                       {
                                           style =
                                           {
                                               alignSelf         = Align.Center, borderLeftWidth = 4, borderRightWidth = 2, borderTopWidth = 2,
                                               borderBottomWidth = 2
                                           }
                                       };

            assetViewDataPanel.Add(assetViewSelectionLabel);

            assetViewMapSelectionNeededHelpBox =
                new TpHelpBox("No selection. Choose a tilemap from the list in the left column",
                               "tilemap-list-needs-selection",
                               HelpBoxMessageType.Info) { style = { display = DisplayStyle.None }, visible = false };
            assetViewDataPanel.Add(assetViewMapSelectionNeededHelpBox);
            assetViewDataPanel.Add(palettesListView);

            //tiles view

            //the second part of this panel is the Tiles view panel. This is used when in TILEMAP
            //mode, and displays a list of tiles from the selected tilemap (leftmost pane)
            editModeAssetListView = new TpListView(PainterWindow.m_CurrentTileList,
                                            listItemHeight, 
                                            true,
                                            MakeTileListItem,
                                            BindTileListItem);

            editModeAssetListView.itemsChosen += EditModeAssetListViewOnitemsChosen;
            
            editModeAssetListView.selectionChanged  += OnTilesListViewSelectionChange;
           
            editModeAssetListView.style.flexGrow    =  1;
            editModeAssetListView.style.minWidth    =  viewPanesMinWidth;
            editModeAssetListView.name              =  "tiles-list-view";

            editModeAssetListView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            assetViewDataPanel.Add(editModeAssetListView);
            EditModeAssetListViewOrigColor =  editModeAssetListView.style.color;
            return assetViewDataPanel;
        }

        private StyleColor EditModeAssetListViewOrigColor { get; set; }


        /// <summary>
        /// Control Panels for Palette lists or Tile Lists (center column)
        /// </summary>
        /// <returns>Visual Element</returns>
        private VisualElement BuildAssetViewControls()
        {
            assetViewControlsScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal)
                                           {
                                               name = "asset-view-control-container", style =
                                               {
                                                   minWidth            = viewPanesMinWidth, minHeight = 80, borderTopWidth        = 6, borderLeftWidth         = 2,
                                                   borderBottomWidth   = 4, borderRightWidth          = 2, borderBottomLeftRadius = 3, borderBottomRightRadius = 3,
                                                   borderTopLeftRadius = 3, borderTopRightRadius      = 3, paddingBottom          = 2, paddingLeft             = 4,
                                                   paddingTop          = 2, paddingRight              = 2, marginLeft             = 4
                                               }
                                           };

            //want same look as scrollviews inside the list-views.
            assetViewControlsScrollView.AddToClassList("unity-collection-view--with-border");
            assetViewControlsScrollView.AddToClassList("unity-collection-view__scroll-view");
            assetViewControlsScrollView.Q("unity-content-container").style.flexGrow = 1;

            assetViewControlsScrollView.Add(BuildPaletteOptions());
            assetViewControlsScrollView.Add(BuildTileOptions());
            return assetViewControlsScrollView;

            [NotNull]
            VisualElement BuildTileOptions()
            {
                assetViewTileOptions = new VisualElement {name = "tile-options",
                                                              style =
                                                              {
                                                                  display = DisplayStyle.None
                                                              }
                                                          }; //a container so we can switch visible/invisible

                assetViewTileOptions.Add(new Label("Options") { style = { alignSelf = Align.Center, marginBottom = 2 } });

                var toggle = new Toggle("Show IDs")
                         {
                             tooltip = "Show instance IDs next to tile position",
                             name    = "setting-show-iid",
                             value   = Config.TpPainterShowIid
                         };
                toggle.RegisterValueChangedCallback(evt =>
                                                    {
                                                        Config.TpPainterShowIid = evt.newValue;
                                                        PainterWindow.RefreshTilesView();
                                                    });
                assetViewTileOptions.Add(toggle);
                
                //note that choices match enum TpTileSorting
                var choices = new List<string>
                              {
                                  "Unsorted", "Type", "IID"
                              };
                var radioGroup = new RadioButtonGroup("Tile Sorting",
                                                      choices)
                                 {
                                     style =
                                     {
                                         flexDirection           = FlexDirection.Column,
                                         borderBottomColor       = Color.red,
                                         borderTopColor          = Color.red,
                                         borderLeftColor         = Color.red,
                                         borderRightColor        = Color.red,
                                         borderBottomWidth       = 1,
                                         borderTopWidth          = 1,
                                         borderLeftWidth         = 1,
                                         borderRightWidth        = 1,
                                         borderBottomLeftRadius  = 4,
                                         borderTopLeftRadius     = 4,
                                         borderTopRightRadius    = 4,
                                         borderBottomRightRadius = 4,
                                     }
                                 };
                radioGroup.SetValueWithoutNotify((int)Config.TpPainterTileSorting);

                radioGroup.RegisterValueChangedCallback(evt =>
                                                        {
                                                            Config.TpPainterTileSorting = (TpTileSorting)evt.newValue;
                                                            PainterWindow.RefreshTilesView();
                                                        });
                assetViewTileOptions.Add(radioGroup);

                assetViewTileOptions.Add(new TpSpacer(10,10));
                
                assetViewTileOptions.Add(CreateFilterGui());
                

                return assetViewTileOptions;
            }

            [NotNull]
            VisualElement BuildPaletteOptions()
            {
                assetViewPaletteOptions = new VisualElement{name = "palette-options"}; //just a container so we can switch it visible/invisible

                var searchFieldContainer = new VisualElement { name = "search-field-container", style = { flexGrow = 0, borderBottomWidth = 2, borderBottomColor = Color.black, marginBottom = 2 } };

                var searchInnerContainer = new VisualElement { name = "search-field-inner-container", style = { flexGrow = 1, flexDirection = FlexDirection.Row } };

                
                var sf = new TextField(16,
                                       false,
                                       false,
                                       ' ')
                         {
                             style =
                             {
                                 flexGrow                = 1,
                                 borderBottomRightRadius = SearchFieldRadius,
                                 borderBottomLeftRadius  = SearchFieldRadius,
                                 borderTopRightRadius    = SearchFieldRadius,
                                 borderTopLeftRadius     = SearchFieldRadius
                             }
                         };

                sf.RegisterValueChangedCallback(evt =>
                                                {
                                                    PainterWindow.m_CurrentPaletteSearchString = evt.newValue;
                                                    PainterWindow.RebuildPaletteListIfChanged();
                                                });
                searchInnerContainer.Add(sf);
                searchInnerContainer.Add(new TpSpacer(4, 4));
                var clearTextButton = new Button(() => sf.value = string.Empty) { style = { backgroundImage = FindIcon(TpIconType.UnityXIcon) } };
                searchInnerContainer.Add(clearTextButton);

                searchFieldContainer.Add(searchInnerContainer);
                searchFieldContainer.Add(new Label("Search is case-insensitive") { style = { scale = new StyleScale(new Vector2(0.8f, 0.8f)), alignSelf = Align.Center, unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Italic) } });
                assetViewPaletteOptions.Add(searchFieldContainer);


                assetViewPaletteOptions.Add(new Label("Options") { style = { alignSelf = Align.Center, marginBottom = 2 } });
                
                var toggle = new Toggle("Show Palettes") { name = "setting-show-palettes", value = Config.TpPainterShowPalettes };
                toggle.RegisterValueChangedCallback(evt =>
                                                    {
                                                        Config.TpPainterShowPalettes = evt.newValue;
                                                        PainterWindow.RebuildPaletteListIfChanged();
                                                        PainterWindow.ClearClipboard();
                                                        PainterWindow.ResetSelections(false);
                                                        brushInspectorTilesListView.Rebuild();
                                                    });
                assetViewPaletteOptions.Add(toggle);
                
                
                toggle = new Toggle("Show TileFabs") { name = "setting-show-tilefabs", value = Config.TpPainterShowTilefabs };
                toggle.RegisterValueChangedCallback(evt =>
                                                    {
                                                        Config.TpPainterShowTilefabs = evt.newValue;
                                                        PainterWindow.RebuildPaletteListIfChanged();
                                                        PainterWindow.ClearClipboard();
                                                        PainterWindow.ResetSelections(false);
                                                        brushInspectorTilesListView.Rebuild();
                                                    });
                assetViewPaletteOptions.Add(toggle);
                
                toggle = new Toggle("Show Tile Bundles") { name = "setting-show-combined-tiles", value = Config.TpPainterShowCombinedTiles };
                toggle.RegisterValueChangedCallback(evt =>
                                                    {
                                                        Config.TpPainterShowCombinedTiles = evt.newValue;
                                                        PainterWindow.RebuildPaletteListIfChanged();
                                                        PainterWindow.ClearClipboard();
                                                        PainterWindow.ResetSelections(false);
                                                        brushInspectorTilesListView.Rebuild();
                                                    });
                assetViewPaletteOptions.Add(toggle);

                
                return assetViewPaletteOptions;
            }
        }


        /// <summary>
        /// Builds the inspector UI (RIGHTmost column)
        /// for palette items (Paint mode)
        /// </summary>
        /// <returns>Visual element</returns>
        [NotNull]
        private VisualElement BuildBrushInspector()
        {
            // a container for this mess
            var ve = new VisualElement
                                    {
                                        name = "paint-inspector-container",
                                        style =
                                        {
                                            flexGrow = 1,flexShrink = 1,
                                            minWidth = viewPanesMinWidth
                                        }
                                    };

            //the vertical split view gets added to this container.
            brushInspectorViewSplitter = new TpSplitter("painter-splitview-tilesinfo",
                                                    "TPT.TPPAINTER.SPLITVIEW.BRUSHINSPECTOR",
                                                    150,
                                                    TwoPaneSplitViewOrientation.Vertical,
                                                    0, BrushInspectorSplitterFix);

            ve.Add(brushInspectorViewSplitter); //split view added to the container for this pane (which itself is the RH side of a splitview )

            //the top element is a container with two labels and a list
            var inspectorListContainer = new VisualElement
                                        {
                                            name = "inspector-list-container",
                                            style =
                                            {
                                                minWidth  = viewPanesMinWidth,
                                                minHeight = 150,
                                                flexGrow  = 1
                                            }
                                        };
            
            brushInspectorViewSplitter.Add(inspectorListContainer); //added to splitview as fixed-pane
            
            //here's a container for the bottom (fixed) part of the split view
            //and the palette list container is not displayed.
            brushInpectorTileInfoContainer = new VisualElement
                                                      {
                                                          name = "paint-inspector-scrollview-container",
                                                          style =
                                                          {
                                                              minWidth = viewPanesMinWidth,
                                                              minHeight = 100,
                                                              flexGrow = 1
                                                          }
                                                      };
                
                
           //the container includes a scrollview     
           var brushInspectorScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal)
                                             {
                                                 name = "tiles-info-container", style =
                                                 {
                                                     flexGrow = 1,
                                                     minWidth                = viewPanesMinWidth,
                                                     minHeight               = 80,
                                                     borderTopWidth          = 6,
                                                     borderLeftWidth         = 2,
                                                     borderBottomWidth       = 4,
                                                     borderRightWidth        = 2,
                                                     borderBottomLeftRadius  = 3,
                                                     borderBottomRightRadius = 3,
                                                     borderTopLeftRadius     = 3,
                                                     borderTopRightRadius    = 3,
                                                     paddingBottom           = 2,
                                                     paddingLeft             = 4,
                                                     paddingTop              = 2,
                                                     paddingRight            = 2,
                                                     marginLeft              = 4,
                                                     marginBottom            = 8
                                                 }
                                             };
            //want same look as scrollviews inside the list-views.
            brushInspectorScrollView.AddToClassList("unity-collection-view--with-border");
            brushInspectorScrollView.AddToClassList("unity-collection-view__scroll-view");
            brushInpectorTileInfoContainer.Add(brushInspectorScrollView);
            brushInspectorViewSplitter.Add(brushInpectorTileInfoContainer); //container w bottom portion of splitview added to splitview


            //now build the List view which is the upper portion of the splitview

            //header label
            inspectorListContainer.Add(new Label("Tiles")
                                      {
                                          
                                          style =
                                          {
                                              alignSelf         = Align.Center,
                                              borderLeftWidth   = 4,
                                              borderRightWidth  = 2,
                                              borderTopWidth    = 2,
                                              borderBottomWidth = 2
                                          }
                                      });


            //a label for the name of the selected tile
            brushInspectorListSelectionLabel = new Label(TilePlusPainterWindow.EmptyFieldLabel)
                                           {
                                               style =
                                               {
                                                   alignSelf         = Align.Center,
                                                   borderLeftWidth   = 4,
                                                   borderRightWidth  = 2,
                                                   borderTopWidth    = 2,
                                                   borderBottomWidth = 2
                                               }
                                           };

            inspectorListContainer.Add(brushInspectorListSelectionLabel);

            //the list-view for the tiles/fabs/chunks -> RH column
            brushInspectorTilesListView = new TpListView(PainterWindow.TargetPaletteListViewItems,
                                                         listItemHeight,
                                                  true,
                                                  MakePaintModeInspectorListItem,
                                                  BindPaintModeInspectorListItem)
                                  {
                                      name = "palette-item-list-view", style = { flexGrow = 1 }
                                  };
            
            brushInspectorTilesListView.selectionChanged  += OnBrushInspectorTilesListViewSelectionChange;
            
            brushInspectorTilesListView.Q<VisualElement>("unity-content-container").style.flexGrow =  1;

            inspectorListContainer.Add(brushInspectorTilesListView);   //add to container

            
           

            //the container contains an IMGUI view
            brushInspectorGui                     = new IMGUIContainer(TpPainterDataGUI.ImitateBrushInspector);
            brushInspectorGui.cullingEnabled      = true;
            brushInspectorGui.style.paddingBottom = 10;
            brushInspectorGui.style.minHeight     = 75;

            //tilesInfoContainer.Add(new TpSpacer(5, 4));
            brushInspectorScrollView.Add(brushInspectorGui); //add to container

            return ve;
        }

        
       
        /// <summary>
        /// Builds the inspector UI (RIGHTmost column)
        /// for Tilemap selections (EDIT mode)
        /// </summary>
        /// <returns>Visual element</returns>
        [NotNull]
        private VisualElement BuildSelectionInspector()
        {
            // a container for this mess
            var ve = new VisualElement
                                         {
                                             name = "tile-inspector-container",
                                             style =
                                             {
                                                 flexGrow = 1,
                                                 minWidth = viewPanesMinWidth
                                             }
                                         };

            
            //here's a container for the bottom (fixed) part of the split view
            //and the palette list container is not displayed.
            var scroller = new ScrollView(ScrollViewMode.VerticalAndHorizontal)
                                             {
                                                name = "tiles-info-container", style =
                                                 {
                                                     
                                                     flexGrow = 1,
                                                     minWidth                = viewPanesMinWidth,
                                                     minHeight               = 80,
                                                     borderTopWidth          = 6,
                                                     borderLeftWidth         = 2,
                                                     borderBottomWidth       = 4,
                                                     borderRightWidth        = 2,
                                                     borderBottomLeftRadius  = 3,
                                                     borderBottomRightRadius = 3,
                                                     borderTopLeftRadius     = 3,
                                                     borderTopRightRadius    = 3,
                                                     paddingBottom           = 2,
                                                     paddingLeft             = 4,
                                                     paddingTop              = 2,
                                                     paddingRight            = 2,
                                                     marginLeft              = 4,
                                                     marginBottom            = 8
                                                 }
                                             };

            //want same look as scrollviews inside the list-views.
            scroller.AddToClassList("unity-collection-view--with-border");
            scroller.AddToClassList("unity-collection-view__scroll-view");
            
        
            //the container contains an IMGUI view
            selectionInspectorGui                     = new(TpPainterDataGUI.ImitateSelectionInspector);
            selectionInspectorGui.style.paddingBottom = 10;
            //selectionInspectorGui.style.flexGrow      = 1;
            selectionInspectorGui.cullingEnabled      = true;

            scroller.Add(selectionInspectorGui);
            ve.Add(scroller); //add to container

            return ve;
        }
        
        
        
        
        #endregion

        #region tilefilters
        //for the type filter
        /// <summary>
        ///used when computing type filter
        /// </summary>
        private List<Type>               typeList       = new(16);
        /// <summary>
        /// used when computing type filter
        /// </summary>
        private readonly Dictionary<string, Type> typeFilterDict = new(16); //used when computing type filter

        //for the tag filter
        /// <summary>
        ///used when computing tag filter
        /// </summary>
        private List<string> validTags;              

        /// <summary>
        /// Recomputes the tag filter.
        /// </summary>
        internal void RecomputeTagFilter()
        {
            PainterWindow.m_FilterTag = TpLib.ReservedTag;
            ComputeTagFilter();
            PainterWindow.ForceRefreshTilesList();
        }

        /// <summary>
        /// Recomputes the type filter.
        /// </summary>
        internal void RecomputeTypeFilter()
        {
            PainterWindow.m_FilterType = typeof(TileBase);
            ComputeTypeFilter();
            PainterWindow.ForceRefreshTilesList();
        }

        /// <summary>
        /// Resets the Type and Tag filters.
        /// </summary>
        internal void ResetFilters()
        {
            PainterWindow.m_FilterType            = typeof(TileBase);
            PainterWindow.m_FilterTag             = TpLib.ReservedTag;
            ComputeTagFilter();
            ComputeTypeFilter();
            PainterWindow.ForceRefreshTilesList();
        }

        /// <summary>
        /// Actual work of creating the Type filter
        /// </summary>
        private void ComputeTypeFilter()
        {
            typeFilterDict.Clear();

            typeFilterDict.Add(nameof(TileBase), typeof(TileBase));  //ie, everything, including Rule tiles (need plugins)
            typeFilterDict.Add(nameof(Tile),         typeof(Tile)); //ie only normal Unity Tiles
            
            foreach(var typ in TpPreviewUtility.AllPlugins)
                typeFilterDict.Add(typ.GetTargetTileType.Name,  typ.GetTargetTileType);

            TpLib.GetAllTypesInDb(ref typeList);
            foreach (var item in typeList)
                typeFilterDict.TryAdd(GetShortTypeName(item), item);

            if (typeDropDown == null)
                return;

            typeDropDown.choices = typeFilterDict.Keys.ToList();

            if (!typeFilterDict.ContainsValue(PainterWindow.m_FilterType))
                PainterWindow.m_FilterType = typeof(TileBase);
            typeDropDown.value = GetShortTypeName(PainterWindow.m_FilterType);


        }

        /// <summary>
        /// Gets the short name of the type.
        /// </summary>
        /// <param name="item">Type to get the short name of</param>
        /// <returns>string</returns>
        private string GetShortTypeName([NotNull] Type item)
        {
            var substrings = item.ToString().Split('.');
            var len        = substrings.Length;
            return len == 0
                       ? string.Empty
                       : substrings[len - 1]; //string name of the type
        }


        /// <summary>
        /// Computes the tag filter.
        /// </summary>
        private void ComputeTagFilter()
        {
            validTags = TpLib.GetAllTagsInDb.ToList();
            validTags.Insert(0, TpLib.ReservedTag);

            if (tagDropDown == null)
                return;

            tagDropDown.choices = validTags;
            tagDropDown.value = !validTags.Contains(PainterWindow.m_FilterTag)
                                    ? TpLib.ReservedTag
                                    : PainterWindow.m_FilterTag;

        }

        /// <summary>
        /// The type drop down UI Element
        /// </summary>
        private DropdownField typeDropDown;
        /// <summary>
        /// The tag drop down UI Element
        /// </summary>
        private DropdownField tagDropDown;

        /// <summary>
        /// Creates the filter GUI.
        /// </summary>
        /// <returns>UnityEngine.UIElements.VisualElement.</returns>
        [NotNull]
        private VisualElement CreateFilterGui()
        {
            //filters
            
            ComputeTypeFilter();
            ComputeTagFilter();

            var container = new VisualElement{ style              =
                                             {
                                                 flexGrow = 1, marginBottom = 2,
                                                 borderBottomColor = Color.red,
                                                 borderTopColor = Color.red,
                                                 borderLeftColor = Color.red,
                                                 borderRightColor = Color.red,
                                                 borderBottomWidth = 1,
                                                 borderTopWidth = 1,
                                                 borderLeftWidth = 1,
                                                 borderRightWidth = 1,
                                                 borderBottomLeftRadius = 4,
                                                 borderTopLeftRadius = 4,
                                                 borderTopRightRadius = 4,
                                                 borderBottomRightRadius = 4,
                                                 
                                             }, name = "filter-container" };
            var label     = new Label("Type/Tag filters") { style = { alignSelf = Align.Center, unityFontStyleAndWeight = FontStyle.Bold} };
            container.Add(label);

            container.Add(new TpSpacer(4, 10));
            
            //reset button
            var button = new Button(ResetFilters)
                         {
                             name = "reset-filter-button",
                             text = "Reset filters",
                             style =
                             {
                                 flexGrow = 0
                             }
                         };
            container.Add(button);
            
            container.Add(new TpSpacer(4,10));
            
            //add type-filter
            typeDropDown = new DropdownField
                               {
                                   label   = Filter_Type_Dropdown_Label,
                                   tooltip = Filter_Type_Dropdown_Tip,
                                   choices = typeFilterDict.Keys.ToList(),
                                   value   = GetShortTypeName(PainterWindow.m_FilterType)
                               };
            typeDropDown.Q<Label>().style.minWidth = 30;
            
            typeDropDown.RegisterValueChangedCallback(evt =>
                                                      {
                                                          typeFilterDict.TryGetValue(evt.newValue, out PainterWindow.m_FilterType);

                                                          PainterWindow.ForceRefreshTilesList();
                                                      });
            container.Add(typeDropDown);
            container.Add( new TpSpacer(4, 20));
            container.Add(new Label("Tags are case-insensitive\nand only apply to TilePlus tiles.")
                          {
                              style =
                              {
                                  color                   = Color.red,
                                  overflow = new StyleEnum<Overflow>(Overflow.Hidden),
                                  textOverflow            = TextOverflow.Clip,
                                  alignSelf               = Align.Center,
                                  unityFontStyleAndWeight = FontStyle.Bold,
                                  scale                   = new StyleScale(new Vector2(0.8f,0.8f))
                              }
                          });
            //add tag-filter
            tagDropDown = new DropdownField
                              {
                                  
                                  label = Filter_Tag_Dropdown_Label,
                                  tooltip = $"'{TpLib.ReservedTag}' {Filter_Tag_Dropdown_Tip}"
                              };
            if (validTags != null && validTags.Count != 0)
            {
                tagDropDown.choices = validTags;
                tagDropDown.value   = validTags[0];
            }

            tagDropDown.Q<Label>().style.minWidth = 30;
            
            tagDropDown.RegisterValueChangedCallback(evt =>
                                                     {
                                                             PainterWindow.m_FilterTag = evt.newValue;
                                                             PainterWindow.ForceRefreshTilesList();

                                                     });
            
            container.Add(tagDropDown);
            return container;
        }
        
        #endregion
        
        #region makeBind

        /// <summary>
        /// This is used when in EDIT mode: a list item for list of tiles (in center pane) from the selected tilemap in leftmost pane.
        /// </summary>
        /// <returns>Visual Element</returns>
        [NotNull]
        private VisualElement MakeTileListItem()
        {
            //container
            var item = new TpListBoxItem("imageWithLabelContainer", Color.black); 
            var iconHeight = listItemHeight + 6;
            item.Add(new Image {name = "imageL", style = {paddingLeft = 8, width = iconHeight, height = iconHeight, flexShrink = 0}});
            item.Add(new Image {name = "imageR", style = {paddingLeft = 8, width = iconHeight, height = iconHeight, flexShrink = 0}});

            //Label used for text
            item.Add(new Label
            {
                tooltip = "Click to inspect",
                name = "label", style = { paddingRight = 8, paddingLeft = 2, height = listItemHeight }
            });
            
            return item;
        }

        /// <summary>
        /// This is used when in TILEMAP mode: a list item for list of tiles from the selected tilemap (as chosen from leftmost pane)
        /// </summary>
        private void BindTileListItem(VisualElement element, int index)
        {
            //get the label element (text) and the icon element
            var labl = element.Q<Label>("label");
            var imgL = element.Q<Image>("imageL");
            var imgR = element.Q<Image>("imageR");
            
            var item = PainterWindow.m_CurrentTileList[index];
            labl.style.color = PainterWindow.TilemapPaintTargetCount > Config.MaxTilesForViewers
                                   ? new StyleColor(Color.yellow)
                                   : EditModeAssetListViewOrigColor;


            if (item is TilePlusBase tpb) 
            {
                var tname = tpb.TileName;
                var dex = tname.IndexOf('(');
                if (dex != -1)
                    tname = tname.Substring(0, dex);

                labl.text =
                    $"{tname} {(Config.TpPainterShowIid ? $"{tpb.TileGridPosition.ToString()} [id: {tpb.Id.ToString()}] " : tpb.TileGridPosition.ToString())}";
                
                imgL.image  = FindIcon(TpIconType.TptIcon);
                imgR.sprite = tpb.sprite;
            }
            else
            {
                labl.text = Config.TpPainterShowIid
                                ? $"{item.name} [id: {item.GetInstanceID().ToString()}]"
                                : item.name;
                imgL.image = FindIcon(TpIconType.TileIcon);
                if (item is Tile t)
                    imgR.sprite = t.sprite;
                else if (item is { } tb)
                {
                    imgR.sprite = TpPreviewUtility.TryGetPlugin(tb, out var plug) && plug != null
                            ? plug.GetSpriteForTile(tb)
                            : SpriteFromTexture(FindIcon(TpIconType.UnityToolbarMinusIcon));
                    
                }
                else
                    imgR.sprite = null;
            }

        }

        
        /// <summary>
        /// Make a list item to be used in the Rightmost column (inspector) when displaying items from a
        /// palette, chunk, tilefab or FavoritesList
        /// </summary>
        /// <returns>Visual item</returns>
        [NotNull]
        private VisualElement MakePaintModeInspectorListItem()
        {
            var height = Config.PainterListItemHeight;
            var outer = new VisualElement { 
                                              name = "palette-inspector-list-item",
                                              style =
                                              {
                                                  flexGrow    = 1, alignContent = Align.FlexStart,
                                                  paddingLeft = 2, paddingRight  = 2, paddingBottom = 1, 
                                                  paddingTop = 1,
                                                  
                                                  
                                              } };
            
           
            var container = new TpListBoxItem("palette-inspector-list-item-item", Color.black); 

            
            outer.Add(container);
            var image = new Image
                        {
                            name = "palette-item-image",
                            style =
                            {
                                minWidth          = PaletteTargetImageSize - PaletteTargetItemBorderWidth,
                                minHeight         = PaletteTargetImageSize - PaletteTargetItemBorderWidth,
                                maxWidth          = height,
                                maxHeight         = height,
                                borderBottomWidth = PaletteTargetItemBorderWidth,
                                borderTopWidth    = PaletteTargetItemBorderWidth,
                                borderRightWidth  = PaletteTargetItemBorderWidth,
                                borderLeftWidth   = PaletteTargetItemBorderWidth,
                            }
                        };

            container.Add(image);
            //container.Add(new TpSpacer(10,4));
            var label = new Label { name = "palette-item-label",style = {whiteSpace  = WhiteSpace.Normal, flexGrow = 0/*, unityFontStyleAndWeight = FontStyle.Bold*/} };
            container.Add(label);
            container.Add(new TpSpacer(10, 4));
            var imgSize = PaletteTargetImageSize / 4;

            container.Add(new Image { name = "palette-item-tpt", image = FindIcon(TpIconType.TptIcon),
                                        style = { flexGrow = 0, height = imgSize, width = imgSize } });
            //outer.Add(new TpSpacer(20,10){style={flexGrow = 1}});

            var textField = new TextField
                            {
                                focusable = false,
                                multiline = true,
                                name = "palette-item-textfield",
                                maxLength = 8192,
                                style =
                                {
                                    whiteSpace              = WhiteSpace.Normal,
                                    flexGrow                = 1,
                                    unityFontStyleAndWeight = FontStyle.Bold,
                                    display                 = DisplayStyle.None
                                    
                                }
                            };
            textField.Q<TextElement>().enableRichText = true;
            var sv = new ScrollView(){style={flexGrow = 1}};
            sv.contentContainer.style.flexGrow = 1;
            sv.Add(textField);
            outer.Add(sv);
            return outer;
        }

        /// <summary>
        /// Bind a list item to be used in the Rightmost column (inspector) when displaying items from a
        /// palette, chunk, or history list
        /// </summary>
        private void BindPaintModeInspectorListItem(VisualElement element, int index)
        {

            [CanBeNull]
            Sprite GetSprite([NotNull] TileBase tile)
            {
                return TpPreviewUtility.TryGetPlugin(tile, out var plug) && plug != null
                           ? plug.GetSpriteForTile(tile)
                           : SpriteFromTexture(FindIcon(TpIconType.UnityToolbarMinusIcon));
            }
            
            var item      = PainterWindow.m_TilesToDisplay[index];
            var outer     = element.Q<VisualElement>("palette-inspector-list-item");
            var container = element.Q<VisualElement>("palette-inspector-list-item-item");            
            outer.style.flexGrow = 1; //something resets this! WT!#@^%$
            var textfield = element.Q<TextField>("palette-item-textfield");
            var label     = element.Q<Label>("palette-item-label");
            var img       = element.Q<Image>("palette-item-image");
            var tptImg    = element.Q<Image>("palette-item-tpt");

            if (item.ItemVariety is TargetTileData.Variety.TileItem && item.Tile != null) 
            {
                outer.style.flexGrow = 0; //something resets this! WT!#@^%$
                var sprite = SpriteFromTexture(FindIcon(TpIconType.HelpIcon));
                if (item.IsTile) //Tile or TilePlusBase
                    sprite = ((Tile)item.Tile).sprite;
                else if (item.Tile != null) //must be a TileBase
                    sprite = GetSprite(item.Tile);
                
                var isTptTile = item.Tile is ITilePlus;

                img.sprite = sprite;
                tptImg.visible = isTptTile;
                                          
                if (item.Valid)
                {
                    var itemName = item.Tile.name.Trim();
                    if (sprite == null) 
                        label.text = itemName;
                    else
                    {
                        var locked = isTptTile && ((ITilePlus)item.Tile).IsLocked
                                         ? "<i><b> Locked</b></i>"
                                         : "";
                        var extents = sprite.bounds.extents;
                        var height  = extents.y * 2;
                        var width   = extents.x * 2;
                        label.text = $"{itemName}{locked} [{height:N2} X {width:N2}]"; 
                    }
                }
                else
                    label.text = "Null tile in Palette!"; 

                //textfield.style.flexGrow = 0;
                textfield.style.display = DisplayStyle.None;

            }
            //note never should be null tiles in a chunk but it doesn't matter here anyway.
            else if (item.ItemVariety == TargetTileData.Variety.BundleItem)
            {
                outer.style.flexGrow     = 1; //something resets this! WT!#@^%$
                container.style.flexGrow = .2f;
                img.sprite               = SpriteFromTexture( FindIcon(TpIconType.CombinedTilesIcon));
                tptImg.style.display = DisplayStyle.Flex;
                textfield.style.display = DisplayStyle.Flex;
                textfield.style.flexGrow = .8f;
                var bundle     = item.Bundle;
                if (bundle == null)
                {
                    textfield.value = "Invalid or Null Bundle!!";
                }
                else
                {
                    var numTpTiles = bundle.m_TilePlusTiles.Count;
                    var numUtiles  = bundle.m_UnityTiles.Count;
                    var numPrefabs = bundle.m_Prefabs.Count;
                    var size       = bundle.m_TilemapBoundsInt.size;
                    var bPath      = AssetDatabase.GetAssetPath(bundle);
                    var sel = bundle.m_FromGridSelection
                                  ? $"[From Selection, Center:{bundle.m_TilemapBoundsInt.center}]"
                                  : $"[Center:{bundle.m_TilemapBoundsInt.center}]";

                    label.text = $"TpTileBundle asset: {bundle.name}";
                    textfield.value = $"Size:{size.x}X{size.y}, Variety: {(bundle.m_FromGridSelection ? "From Grid Selection" : "Entire Tilemap")}.\n"
                                      + $"GUID: {bundle.AssetGuidString}\n"
                                      + $"\n{numTpTiles} TPT tiles, {numUtiles} Unity tiles, {numPrefabs} prefabs. {sel}\n\n"
                                      + $"-------------------------------------------------------------------------------------------------\n"
                                      + "\n1. Please note that errors are NORMAL if any TPT tiles in the asset refer to a Tilemap"
                                      + " by name but that tilemap can't be located."
                                      + "\nFor example: 'No gameobject named SomeName'"
                                      + "\n2. <color=red>Painting this asset will overwrite tiles.</color>"
                                      + $"\n\n<i>Path: {bPath}</i>\n";
                }
            }
            else if (item.ItemVariety == TargetTileData.Variety.TileFabItem)
            {
                outer.style.flexGrow     = 1; //something resets this! WT!#@^%$
                container.style.flexGrow = .2f;
                img.sprite               = SpriteFromTexture( FindIcon(TpIconType.TileFabIcon));
                tptImg.style.display = DisplayStyle.Flex;
                textfield.style.display = DisplayStyle.Flex;
                textfield.style.flexGrow = .8f;

                var fab       = item.TileFab;
                if (fab == null)
                {
                    textfield.value = "Invalid or Null TileFab!!";

                }
                else
                {
                    var numChunks = fab.m_TileAssets!.Count;
                    var size      = fab.LargestBounds.size;
                    var fPath     = AssetDatabase.GetAssetPath(fab);

                    var requiredMaps = fab.m_TileAssets.Select(assetSpec => assetSpec.m_TilemapName);
                    var requiredTags = fab.m_TileAssets.Select(assetSpec => assetSpec.m_TilemapTag);

                    var mapsMessage = string.Join(',', requiredMaps);
                    var tagsMessage = string.Join(',', requiredTags);

                    label.text = $"TpTileFab asset: {fab.name}";
                    textfield.value = $"Size:{size.x}X{size.y}, Variety: {(fab.m_FromGridSelection ? "CHUNK" : "TILEFAB")}.\n"
                                      + $"GUID: {fab.AssetGuidString}\n"
                                      + $"{numChunks} TileBundle Assets in this TileFab. \n\n"
                                      + $"-------------------------------------------------------------------------------------------------\n"
                                      + "<b><color=red>If the named Tilemaps or tags can't be found THEN this TileFab can't be painted or previewed.</color></b>\n"
                                      + "\n1. Please note that errors are NORMAL if any TPT tiles in the asset refer to a Tilemap"
                                      + " by name but that tilemap can't be located."
                                      + "\nFor example: 'No gameobject named SomeName'"
                                      + "\n2. <color=red>Painting this asset will overwrite tiles.</color>\n\n"
                                      + "NOTE: the following named Tilemaps and/or tags must be present to Paint this.\n"
                                      + $"Tilemaps: {mapsMessage}\nTags: {tagsMessage}\n\n"
                                      + "You can edit the TileFab asset to change these values.\n"
                                      + "'Untagged' means that the Tilemap that the tiles were extracted from didn't have a tag. "
                                      + $"\n\n<i>Path: {fPath}</i>\n";
                }
            }
            
        }



        /// <summary>
        /// The palette list item default color
        /// </summary>
        private StyleColor paletteListItemDefaultColor;
        /// <summary>
        /// Make the list item to display information about a palette/chunk/favorites/tilefab for the center column 
        /// </summary>
        /// <returns>Visual Element</returns>
        [NotNull]
        private VisualElement MakePaletteListItem()
        {
            var item = new TpListBoxItem("imageWithLabelContainer", Color.black); 

            //container
            /*var item = new VisualElement
                       {
                           name = "imageWithLabelContainer",
                           style =
                           {
                               //cause child elements to be arranged horizontally
                               flexDirection           = FlexDirection.Row,
                               alignItems              = Align.Center,
                               borderBottomWidth       = 1,
                               borderTopWidth          = 1,
                               borderRightWidth        = 1,
                               borderLeftWidth         = 1,
                               borderBottomColor       = Color.black,
                               borderTopColor          = Color.black,
                               borderLeftColor         = Color.black,
                               borderRightColor        = Color.black,
                               borderBottomLeftRadius  = 4,
                               borderBottomRightRadius = 4,
                               borderTopLeftRadius     = 4,
                               borderTopRightRadius    = 4
                           }
                       };*/

            var iconHeight = listItemHeight * 1.1f;
            item.Add(new Image {name = "image", style = {paddingLeft = 8, width = iconHeight, height = iconHeight, flexShrink = 0}});
            //Label used for text
            var label = new Label
            {
                tooltip = "Click to inspect",
                name = "label", style = { paddingRight = 8, paddingLeft = 2, height = listItemHeight }
            };
            paletteListItemDefaultColor = label.style.color;
            item.Add(label);
            return item;
        }


        /// <summary>
        /// Bind the palette list item
        /// </summary>
        /// <param name="element"></param>
        /// <param name="index"></param>
        private void BindPaletteListItem(VisualElement element, int index)
        {
            //get the label element (text) and the icon element
            var labl      = element.Q<Label>("label");
            labl.style.color = paletteListItemDefaultColor;
            var img       = element.Q<Image>("image");
            var container = element.Q<VisualElement>("imageWithLabelContainer");
            container.tooltip               = string.Empty;


            if(index < 0 || index >= palettes.Count)
            {
                labl.text = "Index of list item out of array range!";
                return;
            }
            
            var item = palettes[index]; 

            if (item == null)
            {
                labl.text = "?";
                return;
            }

            var num = item.Count;
            
            switch (item.ItemType)
            {
                case TpPaletteListItemType.Palette:
                {
                    var palette = item.Palette;
                    img.image = FindIcon(TpIconType.PaletteIcon);
                    labl.text = palette == null
                                    ? "Invalid or null Palette!"
                                    : $"{palette.name}: {num} tile{(num!=1 ? "s" : string.Empty)}";
                    break;
                }
                //tilefab chunk.
                case TpPaletteListItemType.Bundle:
                    labl.text = $"{item.ItemName}: {num} tile{(num !=1 ? "s  total" : string.Empty)}" ;
                    img.image = FindIcon(TpIconType.CombinedTilesIcon);
                    break;
                case TpPaletteListItemType.TileFab:
                    labl.text = $"{item.ItemName}: {num} Bundle{(num !=1 ? "s" : string.Empty)}" ;
                    img.image = FindIcon(TpIconType.TileFabIcon);
                    break;

                case TpPaletteListItemType.History:
                    container.tooltip = "History list is always available. \nAdd to it with:\n\n1. CTRL-CLICK on the tile Clipboard (Upper right of window) when there's a tile in it.\n\n2. Hold down CTRL when clicking on a tile with the PICK tool.\n\n3. Select one or more tile assets in the Project folder and use the Assets or right-click context menu item 'Add To Painter History' \n\nThe Refresh button at the lower-left corner clears History";
                    labl.style.color  = Color.red;
                    num               = PainterWindow.HistoryStackSize;
                    labl.text         = $"{item.ItemName}: {num} tile{(num !=1 ? "s" : string.Empty)}";
                    img.image         = FindIcon(TpIconType.ClipboardIcon);
                    break;
            }
            
        }
        #endregion

        #region splitterGeometry
        /// <summary>
        /// adjust a splitter in a vertical splitview
        /// </summary>
        /// <param name="evt">The event</param>
        private void SourceSplitterFix([NotNull] GeometryChangedEvent evt)
        {
            var handle = assetViewSplitter.Q<VisualElement>("unity-dragline-anchor");
            handle.style.width           = assetViewControlsScrollView.style.width;
            handle.style.height          = TilePlusPainterWindow.SplitterSize;
            handle.style.backgroundColor = Color.red;
            evt.StopPropagation();

        }

        /// <summary>
        /// Adjust a splitter in a vertical splitview
        /// </summary>
        /// <param name="evt">The event.</param>
        
        private void BrushInspectorSplitterFix([NotNull] GeometryChangedEvent evt)
        {
            var handle = brushInspectorViewSplitter.Q<VisualElement>("unity-dragline-anchor");
            
            handle.style.width           = brushInpectorTileInfoContainer.style.width;
            handle.style.height          = TilePlusPainterWindow.SplitterSize;
            handle.style.backgroundColor = Color.red;
            evt.StopImmediatePropagation(); //needed to preserve splitter pos when changing global modes. LEAVE AS IS!
            //evt.StopPropagation();
        }
        #endregion




        /// <summary>
        /// The filter type dropdown label
        /// </summary>
        private const string Filter_Type_Dropdown_Label = "Type Filter";
        /// <summary>
        /// The filter type dropdown tooltip
        /// </summary>
        private const string Filter_Type_Dropdown_Tip   = "Select TileBase to show all Tiles.\nSelect any other type to filter what is displayed in the Tilemaps foldouts.\nNote: this setting isn't persistent and it ANDs with the Tag filter setting.";
        /// <summary>
        /// The filter tag dropdown label
        /// </summary>
        private const string Filter_Tag_Dropdown_Label  = "Tag Filter";
        /// <summary>
        /// The filter tag dropdown tooltip
        /// </summary>
        private const string Filter_Tag_Dropdown_Tip    = "shows all Tiles.\nSelect any other tag to filter (TilePlus tiles only) what is displayed in the Tilemaps foldouts.\nNote: this setting isn't persistent and it ANDs with the Type filter setting.";

       
    }
}
