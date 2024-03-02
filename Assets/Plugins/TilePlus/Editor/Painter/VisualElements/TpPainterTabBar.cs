// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 11-03-2022
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 12-31-2022
// ***********************************************************************
// <copyright file="TpPainterTabBar.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>Creates the buttons bar at the top of the Tile+Painter window</summary>
// ***********************************************************************
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static TilePlus.Editor.TpIconLib;
using static TilePlus.Editor.Painter.TpPainterShortCuts;

namespace TilePlus.Editor.Painter
{
    /// <summary>
    /// TpPainterTabBar creates the buttons bar at the top of the Tile+Painter window
    /// Implements the <see cref="VisualElement" />
    /// </summary>
    /// <seealso cref="VisualElement" />
    internal class TpPainterTabBar : VisualElement
    {
        /// <summary>
        /// The tab bar tile picked tile image
        /// </summary>
        private readonly Image                    tabBarTilePickedTileImage;
        /// <summary>
        /// The main toolbar
        /// </summary>
        private MutuallyExclusiveToolbar mainToolbar;
        /// <summary>
        /// The mode toolbar
        /// </summary>
        private MutuallyExclusiveToolbar modeToolbar;
        /// <summary>
        /// The tab bar tile picked icon
        /// </summary>
        private readonly Image                    tabBarTilePickedIcon;
        /// <summary>
        /// The picked tile
        /// </summary>
        private TileBase                 pickedTile;
        /// <summary>
        /// The picked tile
        /// </summary>
        private TpPickedTileType         pickedTileType;


        /// <summary>
        /// The parent window
        /// </summary>
        // ReSharper disable once MemberCanBeMadeStatic.Local
        [NotNull]
        private TilePlusPainterWindow ParentWindow => TilePlusPainterWindow.instance!;
        
        

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="toolbarContainerHeight">Height of the toolbar container.</param>
        /// <param name="modeBarMinWidth">Minimum width of the mode bar.</param>
        internal TpPainterTabBar(float toolbarContainerHeight, float modeBarMinWidth)
        {

            name = "tab-bar-outer container";
            style.borderBottomColor = EditorGUIUtility.isProSkin
                                          ? Color.white
                                          : Color.gray;
            style.borderBottomWidth = 2;
            style.paddingBottom     = 1;
            style.flexShrink        = 0;
            style.height            = new StyleLength(StyleKeyword.Auto);

            //main tab bar
            var tabBar = CreateTabBar(toolbarContainerHeight);
            Add(tabBar);

            //need the tab bar's container so we can add things to it
            var tabBarContainer = tabBar.Q<VisualElement>("main-toolbar-container");
            //add the mode bar toggle
            tabBarContainer.Insert(0, CreateModeBar(toolbarContainerHeight));

            //add a spacer
            tabBarContainer.Add(new TpSpacer(10, 10) { style = { minWidth = 10 } });
            
            var dim = toolbarContainerHeight * 0.9f;
            
            tabBarTilePickedIcon = new Image
                                   {
                                       name = "tile-type-icon",
                                       tooltip =
                                           "X means that the Clipboard is empty. Other icons indicate the type of item in the Clipboard",
                                       style = //note: matches TpImageToggle
                                       {
                                           alignSelf         = new StyleEnum<Align>(Align.Center),
                                           height            = dim,
                                           width             = dim,
                                           minHeight         = dim,
                                           minWidth          = dim,
                                           paddingBottom     = 1,
                                           borderBottomWidth = 1,
                                           paddingTop        = 1,
                                           paddingLeft       = 1,
                                           paddingRight      = 1
                                       }
                                   };
            tabBarContainer.Add(tabBarTilePickedIcon);
            
            tabBarContainer.Add(new TpSpacer(10, 10) { style = { minWidth = 10 } });
            
            dim = toolbarContainerHeight * 1.25f;
            const float borderWidth = 1;
            tabBarTilePickedTileImage = new Image
                                        {
                                            name = "toolbar-picked-tile",
                                            tooltip =
                                                "This is the Clipboard. It containes the tile picked from the palette or from the scene.\nClick to clear OR CTRL+Click to save the picked tile to the History list. \nOutline: \nBLACK = empty\nWHITE = tile picked from Palette column\nYELLOW = TileFabItem\nRED = tile picked from scene.",
                                            style =
                                            {
                                                borderBottomColor = Color.black,
                                                borderTopColor    = Color.black,
                                                borderLeftColor   = Color.black,
                                                borderRightColor  = Color.black,
                                                borderBottomWidth = borderWidth,
                                                borderLeftWidth   = borderWidth,
                                                borderRightWidth  = borderWidth,
                                                borderTopWidth    = borderWidth,
                                                marginTop         = 2,
                                                marginRight       = 4,
                                                alignSelf         = new StyleEnum<Align>(Align.Auto),
                                                height            = dim,
                                                width             = dim,
                                                minHeight         = dim,
                                                minWidth          = dim
                                            },
                                            sprite = SpriteFromTexture(FindIcon(TpIconType.UnityToolbarMinusIcon))
                                        };

            tabBarTilePickedTileImage.RegisterCallback<ClickEvent>(PickedTileImageClickHandler);
            
            tabBarContainer.Add(tabBarTilePickedTileImage);


            //local methods----------------------------------------------

            //HANDLER for this callback as local method (saves a closure)
            void PickedTileImageClickHandler(ClickEvent evt)
            {
                if (pickedTileType is not TpPickedTileType.Tile)
                {
                    if(pickedTile == null && evt.ctrlKey)
                        PickedTileImageClickHandlerCtrlKeyHandler();
                    return;
                }

                if (evt.ctrlKey)
                {
                    if (pickedTile == null) //redundant?
                    {
                        PickedTileImageClickHandlerCtrlKeyHandler();
                        return;
                    }

                    TpLib.DelayedCallback(ParentWindow,() =>
                                                  {
                                                      ParentWindow.AddToHistory(pickedTile);
                                                  }, "T+P: picked tile -> history", 20);

                    return;
                }
                
                if (ParentWindow.CurrentTool == TpPainterTool.Paint)
                    TpLib.DelayedCallback(ParentWindow,() => mainToolbar.SetButtonActive((int)TpPainterTool.None,
                                                                                         true),
                                          "T+V: ClipboardImageClick-ModeChangeToNone",
                                          10);
                ParentWindow.ClearClipboard();
            }

            void PickedTileImageClickHandlerCtrlKeyHandler()
            {
                TpLib.DelayedCallback(ParentWindow,() =>
                                                   {
                                                       EditorUtility.DisplayDialog("Nope", "No picked tile to save to history.", "Continue");

                                                   },"T+P:null picked tile on picked tile ctrl+click");
            }




            [NotNull]
            VisualElement CreateModeBar(float height)
            {
                var modeSpecs = new List<ToolbarItemSpec>
                                {
                                    new((int)GlobalMode.PaintingView,
                                        $"Painting {GetModeButtonTooltip()}",
                                        GetModeButtonAbbreviatedTooltip(),
                                        FindIcon(TpIconType.UnityPaintIcon)),
                                    new((int)GlobalMode.EditingView,
                                        $"Editing {GetModeButtonTooltip()}",
                                        GetModeButtonAbbreviatedTooltip(),
                                        FindIcon(TpIconType.TilemapIcon)),
                                    new((int)GlobalMode.GridSelView, 
                                    $"Grid Selection {GetModeButtonTooltip()}",
                                    GetModeButtonAbbreviatedTooltip(),
                                    FindIcon(TpIconType.UnityGridIcon))
                                };

                var modeBar = TpToolbar.CreateMutuallyExclusiveToolbar(modeSpecs, ParentWindow.OnModeBarChanged, height);
                modeToolbar                = modeBar.Q<MutuallyExclusiveToolbar>("muex_toolbar");
                modeToolbar.style.minWidth = modeBarMinWidth;
                return modeBar;
            }


            [NotNull]
            VisualElement CreateTabBar(float height)
            {
                
                var binding = ShortcutManager.instance.GetShortcutBinding("TilePlus/Painter/MarqueeDrag [C]");
                var marqSc = $"({binding.ToString()})";
                
                var spec =
                    new List<ToolbarItemSpec>() //note these need to be in the same order as in the enum TpPainterTool
                    {
                        new((int)TpPainterTool.None,
                            $"Inactivate the Tile+Painter. {GetToolTipForTool(TpPainterTool.None)}", 
                            GetAbbreviatedToolTipForTool(TpPainterTool.None), //NB GetToolTipForTool is in TpPainterShortcuts
                            FindIcon(TpIconType.UnityToolbarMinusIcon)),
                        new((int)TpPainterTool.Paint,
                            $"Painting requires choices in all columns below. Hold SHIFT to drag-paint. Hold CTRL to drag-paint in a row or column. Hold ALT when releasing the mouse to paint the Tile's GameObject. Hold {marqSc} to drag an area to Paint. {GetToolTipForTool(TpPainterTool.Paint)} ",
                            GetAbbreviatedToolTipForTool(TpPainterTool.Paint),
                            FindIcon(TpIconType.UnityPaintIcon)),
                        new((int)TpPainterTool.Erase,
                            $"Erasing requires the selection of a tilemap. Hold SHIFT to drag-erase. Hold CTRL to drag-erase in a row or column. NOTE: both types of 'drag-erase' ignore the 'confirm-deletions' setting! {GetToolTipForTool(TpPainterTool.Erase)}",
                            GetAbbreviatedToolTipForTool(TpPainterTool.Erase),
                            FindIcon(TpIconType.UnityEraseIcon)),
                        new((int)TpPainterTool.Pick,
                            $"Pick a tile in a map and put it in the clipboard (upper-right of this window). Hold CTRL to bypass the clipboard and add the pick to History, SHIFT to override automatic mode change to PAINT after pick. See also the 'pin' mini button in the lower button-bar. {GetToolTipForTool(TpPainterTool.Pick)}",
                            GetAbbreviatedToolTipForTool(TpPainterTool.Pick),
                            FindIcon(TpIconType.UnityPickIcon)),
                        new((int)TpPainterTool.Move,
                            $"Pick a tile, then click again to move it. You can change the Tilemap selection after you pick the tile if you want to move the tile to a different map {GetToolTipForTool(TpPainterTool.Move)}",
                            GetAbbreviatedToolTipForTool(TpPainterTool.Move),
                            FindIcon(TpIconType.UnityMoveIcon)),
                        new((int)TpPainterTool.RotateCw,
                            $"Click on a tile to rotate CW -or- rotate CW while Painting {GetToolTipForTool(TpPainterTool.RotateCw)}",
                            GetAbbreviatedToolTipForTool(TpPainterTool.RotateCw),
                            FindIcon(TpIconType.UnityRotateCwIcon)),
                        new((int)TpPainterTool.RotateCcw,
                            $"Click on a tile to rotate CCW -or- rotate CCW while Painting {GetToolTipForTool(TpPainterTool.RotateCcw)}",
                            GetAbbreviatedToolTipForTool(TpPainterTool.RotateCcw),
                            FindIcon(TpIconType.UnityRotateCcwIcon)),
                        new((int)TpPainterTool.FlipX,
                            $"Click on a tile to Flip X -or- Flip X while Painting {GetToolTipForTool(TpPainterTool.FlipX)}",
                            GetAbbreviatedToolTipForTool(TpPainterTool.FlipX),
                            FindIcon(TpIconType.UnityFlipXIcon)),
                        new((int)TpPainterTool.FlipY,
                            $"Click on a tile to Flip Y  -or- Flip Y while Painting {GetToolTipForTool(TpPainterTool.FlipY)}",
                            GetAbbreviatedToolTipForTool(TpPainterTool.FlipY),
                            FindIcon(TpIconType.UnityFlipYIcon)),
                        new((int)TpPainterTool.ResetTransform,
                            $"Click on a tile to reset its transform -or- reset modified transform while Painting {GetToolTipForTool(TpPainterTool.ResetTransform)}",
                            GetAbbreviatedToolTipForTool(TpPainterTool.ResetTransform),
                            FindIcon(TpIconType.UnityXIcon)),
                        new((int)TpPainterTool.Help,
                            "What is this thing? Click here!",
                            string.Empty,
                            FindIcon(TpIconType.HelpIcon)),
                        new((int)TpPainterTool.Settings,
                            "Settings",
                            string.Empty,
                            FindIcon(TpIconType.SettingsIcon))
                    };
                var tabbar = TpToolbar.CreateMutuallyExclusiveToolbar(spec,
                                                                      ParentWindow.OnMainToolbarChanged,
                                                                      height);
                mainToolbar = tabbar.Q<MutuallyExclusiveToolbar>("muex_toolbar");
                return tabbar;
            }
        }



        
        

        /// <summary>
        /// Enables a toolbar button.
        /// </summary>
        /// <param name="tool">which tool?</param>
        /// <param name="enable">enable/disable = true/false</param>
        internal void EnableToolbarButton(TpPainterTool tool, bool enable)
        {
            mainToolbar.SetButtonEnabled((int)tool, enable);
        }


        /// <summary>
        /// Activates the toolbar button.
        /// </summary>
        /// <param name="tool">which tool?</param>
        /// <param name="withNotify">notification or not.</param>
        internal void ActivateToolbarButton(TpPainterTool tool, bool withNotify)
        {
            mainToolbar.SetButtonActive((int)tool, withNotify);
        }

        /// <summary>
        /// make a toolbar button visible or invisible
        /// </summary>
        /// <param name="tool">which tool?</param>
        /// <param name="show">show or hide</param>
        internal void ShowToolbarButton(TpPainterTool tool, bool show)
        {
            mainToolbar.SetButtonVisibility(tool, show);
        }

        /// <summary>
        /// Enables the mode bar button.
        /// </summary>
        /// <param name="mode">Global mode.</param>
        /// <param name="enable">enable/disable = true/false.</param>
        internal void EnableModeBarButton(GlobalMode mode, bool enable)
        {
            modeToolbar.SetButtonEnabled((int)mode, enable);
        }

        /// <summary>
        /// Activates the mode bar button.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="notify">The notify.</param>
        internal void ActivateModeBarButton(GlobalMode mode, bool notify)
        {
            modeToolbar.SetButtonActive((int)mode, notify);
        }

        /// <summary>
        /// Sets the color of the picked tile image border.
        /// </summary>
        /// <param name="c">The color</param>
        private void SetPickedTileImageBorderColor(Color c)
        {
            if (!ParentWindow.GuiInitialized)
                return;
            var sc = (StyleColor)c;
            tabBarTilePickedTileImage.style.borderBottomColor = sc;
            tabBarTilePickedTileImage.style.borderTopColor    = sc;
            tabBarTilePickedTileImage.style.borderLeftColor   = sc;
            tabBarTilePickedTileImage.style.borderRightColor  = sc;

        }

        /// <summary>
        /// Sets the picked tile image.
        /// </summary>
        /// <param name="tile">tile.</param>
        /// <param name="pickType">Type of the pick.</param>
        /// <param name="wasPickedTile">was this picked?</param>
        internal void SetPickedTileImage([CanBeNull] TileBase tile, TpPickedTileType pickType = TpPickedTileType.None, bool wasPickedTile = false)
        {
            [CanBeNull]
            Sprite GetSprite([NotNull] TileBase t)
            {
                return (TpPreviewUtility.TryGetPlugin(t, out var plug) && plug != null)
                           ? plug.GetSpriteForTile(t)
                           : SpriteFromTexture(FindIcon(TpIconType.UnityToolbarMinusIcon));

            }
            
            switch (pickType)
            {
                case TpPickedTileType.None:
                    tabBarTilePickedTileImage.sprite = SpriteFromTexture(FindIcon(TpIconType.UnityToolbarMinusIcon));
                    break;
                case TpPickedTileType.Bundle:
                    tabBarTilePickedTileImage.sprite = SpriteFromTexture(FindIcon(TpIconType.CombinedTilesIcon));
                    break;
                case TpPickedTileType.TileFab:
                    tabBarTilePickedTileImage.sprite = SpriteFromTexture(FindIcon(TpIconType.TileFabIcon));
                    break;
                case TpPickedTileType.Tile:
                {
                    var sprite = SpriteFromTexture(FindIcon(TpIconType.HelpIcon));  //these icons are cached
                    if (tile is Tile t) //Tile or TilePlusBase
                        sprite = t.sprite;
                    else if (tile != null) //TileBase
                        sprite = GetSprite(tile);
                    tabBarTilePickedTileImage.sprite = sprite;
                    break;
                }
            }
            
            pickedTile                       = tile;
            pickedTileType              = pickType;

            if (tile == null ||  pickType is TpPickedTileType.Bundle or TpPickedTileType.TileFab )
            {
                SetPickedTileImageBorderColor(pickType is TpPickedTileType.Bundle or TpPickedTileType.TileFab
                                                  ? Color.yellow
                                                  : Color.black);
            }
            else
            {
                SetPickedTileImageBorderColor(wasPickedTile
                                                  ? Color.red
                                                  : Color.white);
            }

            switch (pickType)
            {
                case TpPickedTileType.Bundle:
                    tabBarTilePickedIcon.sprite = SpriteFromTexture(FindIcon(TpIconType.CombinedTilesIcon));
                    break;
                case TpPickedTileType.TileFab:
                    tabBarTilePickedIcon.sprite = SpriteFromTexture(FindIcon(TpIconType.TileFabIcon));
                    break;
                default:
                {
                    if (tile == null)
                        tabBarTilePickedIcon.sprite = SpriteFromTexture(FindIcon(TpIconType.UnityXIcon));
                    else
                        tabBarTilePickedIcon.sprite = SpriteFromTexture(tile is ITilePlus
                                                                            ? FindIcon(TpIconType.TptIcon)
                                                                            : FindIcon(TpIconType.TileIcon));

                    break;
                }
            }

        }
        
        /// <summary>
        /// change the picked tile icon to show that the transform was changed OR NOT
        /// </summary>
        internal void SetTabTilePickedIconAsTransformModified(bool wasModified, TargetTileData ttd)
        {
            if(wasModified)
                tabBarTilePickedIcon.sprite = SpriteFromTexture(FindIcon(TpIconType.UnityTransformIcon));
            else 
                SetPickedTileImage(ttd.Tile, TpPickedTileType.Tile, ttd.WasPickedTile );
        }


       
        
    }
}
