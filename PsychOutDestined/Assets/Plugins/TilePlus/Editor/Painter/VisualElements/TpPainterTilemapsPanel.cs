// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 11-05-2022
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 12-31-2022
// ***********************************************************************
// <copyright file="TpPainterTilemapsPanel.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>Panel with list of tilemaps</summary>
// ***********************************************************************
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace TilePlus.Editor.Painter
{

    /// <summary>
    /// TpPainterTilemapsPanel creates the list of tilemaps in the left col of Tile+Painter
    /// Implements the <see cref="VisualElement" />
    /// </summary>
    /// <seealso cref="VisualElement" />
    internal class TpPainterTilemapsPanel : VisualElement
    {
        #region privateFields
        private readonly Label          tilemapsListHeader;
        private readonly Label          tilemapsListSelectionLabel;
        private readonly TpListView     tilemapsListView;
        private readonly TilePlusPainterConfig config;
        private          float          listItemHeight;

        // ReSharper disable once MemberCanBeMadeStatic.Local
        [NotNull]
        private TilePlusPainterWindow Win => TilePlusPainterWindow.instance!;
      

        #endregion

        #region ctor
        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="dataSource">data source for listview.</param>
        /// <param name="viewPanesMinWidth">Minimum width of the view panes.</param>
        internal TpPainterTilemapsPanel(List<TilemapData>          dataSource,
                                        float                       viewPanesMinWidth)
        {
            
            config         = TilePlusPainterConfig.instance;
            listItemHeight = config.PainterListItemHeight;
            
            //listview for Tilemaps is in the leftmost pane
            name           = "tilemap-list-outer-container";
            style.minWidth = viewPanesMinWidth;

            //add a label for the selected tilemap
            Add( tilemapsListHeader =new Label("Tilemaps")
                {
                    style =
                    {
                        alignSelf         = Align.Center, borderLeftWidth = 2, borderRightWidth = 2, borderTopWidth = 2,
                        borderBottomWidth = 2
                    }
                });

            var tilemapLabelText = TilePlusPainterWindow.EmptyFieldLabel;
           

            //this label shows the name of the selected tilemap
            tilemapsListSelectionLabel = new Label(tilemapLabelText)
                                         {
                                             style =
                                             {
                                                 alignSelf         = Align.Center, borderLeftWidth = 4, borderRightWidth = 2, borderTopWidth = 2,
                                                 borderBottomWidth = 2
                                             }
                                         };
            Add(tilemapsListSelectionLabel);


            //this listview has the names of the tilemaps
            tilemapsListView = new TpListView(dataSource,
                                              config.PainterListItemHeight,
                                               true,
                                               MakeTilemapListItem,
                                               BindTilemapsListItem);

            tilemapsListView.itemsChosen                     += OnItemsChosen;
            tilemapsListView.selectionChanged                += OnTilemapListViewSelectionChange;
            tilemapsListView.style.flexGrow                  =  1;
            Add(tilemapsListView);
        }
        
        #endregion

        #region  events


        /// <summary>
        /// In EDIT mode this is called when DBL clicking on a Tile in the center column.
        /// Unlike OnTilesListViewSelectionChange this just checks to see
        /// if the selection is the same as before, and if it is the highlight is shown.
        /// Basically a convenience function
        /// </summary>
        /// <param name="objs">selected items</param>
        private void OnItemsChosen([CanBeNull] IEnumerable<object> objs)
        {
            if (Win.DiscardListSelectionEvents || objs == null)
                return;

            if (objs.First() is not TilemapData ttd)
                return;
            
            SetTarget(ttd);
        }



        private void OnTilemapListViewSelectionChange([CanBeNull] IEnumerable<object> objs)
        {
            if (Win.DiscardListSelectionEvents || objs == null)
                return;

            if (objs.First() is not TilemapData ttd)
                return;

            SetTarget(ttd);

        }

        #endregion

        #region access

        internal void SetTarget(int index)
        {
            SetTarget((TilemapData) tilemapsListView.itemsSource[index]);
        }

        private void SetTarget([NotNull] TilemapData ttd)
        {
            var theSameMap = Win.m_TilemapPaintTarget != null &&
                             Win.m_TilemapPaintTarget.TargetTilemap == ttd.TargetMap;

            if (!Win.SetPaintTarget(ttd.TargetMap) && !theSameMap)
                return;
            if (Win.GlobalMode != GlobalMode.PaintingView)
                Win.SetInspectorTarget(ttd.TargetMap);

            if (!config.TpPainterSyncSelection)
                return;

            // ReSharper disable once InvertIf
            if (ttd.TargetMap != null)   //Win.m_TilemapPaintTarget != null && Win.m_TilemapPaintTarget.TargetTilemap != null)
            {
                var go = ttd.TargetMap.gameObject;
                Selection.SetActiveObjectWithContext(go, go);
            }
        }

        /// <summary>
        /// Sets the tilemaps list header.
        /// </summary>
        /// <param name="text">The text.</param>
        internal void SetTilemapsListHeader(string text)
        {
            tilemapsListHeader.text = text;
        }


        /// <summary>
        /// Sets the selection label.
        /// </summary>
        /// <param name="text">The text.</param>
        internal void SetSelectionLabel(string text)
        {
            tilemapsListSelectionLabel.text = text;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="selectionIndex">Index of the selection.</param>
        internal void SetSelection(int selectionIndex)
        {
            tilemapsListView.SetSelection(new[] { selectionIndex });
        }

        /// <summary>
        /// Sets the selection without notify.
        /// </summary>
        /// <param name="selectionIndex">Index of the selection.</param>
        internal void SetSelectionWithoutNotify(int selectionIndex)
        {
            tilemapsListView.SetSelectionWithoutNotify(new[] { selectionIndex });
        }

        /// <summary>
        /// Updates the tilemaps list.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        internal void UpdateTilemapsList(List<TilemapData> dataSource)
        {
            tilemapsListView.itemsSource = dataSource;
            tilemapsListView.Rebuild();
        }

        
        
        
        /// <summary>
        /// Clears the selection.
        /// </summary>
        internal void ClearSelection()
        {
            tilemapsListView.ClearSelection();
        }



        /// <summary>
        /// Rebuilds the element.
        /// </summary>
        internal void RebuildElement()
        {
            tilemapsListView.Rebuild();
        }

        /// <summary>
        /// Rebind all items
        /// </summary>
        internal void ReBindElement(List<TilemapData> src)
        {
            tilemapsListView.itemsSource = src;
            tilemapsListView.RefreshItems();
        }

        /// <summary>
        /// Gets the data source of the list view.
        /// </summary>
        /// <value>The data source of the list view.</value>
        internal IList DataSource => tilemapsListView.itemsSource;
              
        
        /// <summary>
        /// The current selection index
        /// </summary>
        internal int SelectionIndex => tilemapsListView.selectedIndex;
        
        #endregion

        
        #region makebind
        [NotNull]
        private VisualElement MakeTilemapListItem()
        {
            //used in MakeTilemapListItem
            var imageWidth  = config.PainterListItemHeight * 0.75f;
            //container
            var item = new TpListBoxItem("tilemap-list-item", Color.black); 
           
            //leading Label used for its image
            var leftImage = new Image
                            {   
                                name = "leftimage",
                                tooltip = "Indicates that the Tilemap has TilePlus Tiles",
                                image = TpIconLib.FindIcon(TpIconType.TptIcon),
                                style =
                                {
                                    width = imageWidth, 
                                    height = imageWidth,
                                    minHeight = imageWidth,
                                    minWidth = imageWidth,
                                    marginRight     = 1
                                }
                            };

            var rightImage = new Image
                            {   
                                name    = "rightimage",
                                tooltip = "Indicates that the Tilemap is part of a Prefab: can't be edited unless in a Prefab stage, if that's true then the icon is red.",
                                image   = TpIconLib.FindIcon(TpIconType.PrefabIcon),
                                style =
                                {
                                    width     = imageWidth ,
                                    height    = imageWidth,
                                    minHeight = imageWidth,
                                    minWidth  = imageWidth,
                                    marginRight = 1
                                }
                            };

            item.Add(leftImage);
            item.Add(rightImage);
            
            //Label used for text
            var label                   = new Label { name = "label", style = { marginRight = 2} };
            label.tooltip = "'+NZ' = Tilemap origin != 0\nDblClick = select same Tilemap";
            item.Add(label);
            return item;
        }


        private readonly Color inStageColor = new(1, 0, 0, 0.5f);

        private void BindTilemapsListItem(VisualElement element, int index)
        {
            //get the label element (text) and the icon element
            var labl     = element.Q<Label>("label");
            var leftIcon  = element.Q<Image>("leftimage");  //get the image
            var rightIcon = element.Q<Image>("rightimage"); //get the image
            
            var item      = Win.m_TilemapListViewItems[index];
            var parentMap = item.TargetMap;
            var nScenes   = EditorSceneManager.loadedRootSceneCount;
            var gridName = string.Empty;
            if (TpPainterScanners.MoreThanOneGrid)
            {
                if (TpPainterScanners.GetGridForTilemap(parentMap, out var grid))
                {
                    gridName = $"{grid!.name}.";
                }
            }

            var offset = parentMap.transform.position != Vector3.zero ? "+NZ" : string.Empty;
            labl.text = nScenes == 1 || !TpPainterScanners.MoreThanOneGrid
                            ? $" {gridName}{parentMap.name}{offset}"
                            : $"{item.ParentSceneOfMap.name}.{gridName}{parentMap.name}{offset}";
            
            var inStage = item.InPrefabStage;

            element.style.backgroundColor = inStage
                ? new StyleColor(inStageColor)
                : new StyleColor(StyleKeyword.Null);  //StyleKeyword.Auto);

            
            
            leftIcon.style.opacity  = item.HasTptTilesInMap ? 1f: 0f;
            rightIcon.style.opacity = item.InPrefab || inStage ? 1f : 0f;
            
            rightIcon.style.backgroundColor = inStage
                                                  ? new StyleColor(inStageColor)
                                                  : new StyleColor(StyleKeyword.Null);

        }

        #endregion        
        
    }
}
