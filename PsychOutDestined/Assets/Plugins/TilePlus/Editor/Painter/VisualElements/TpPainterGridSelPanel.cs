// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 04-24-2023
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 04-21-2023
// ***********************************************************************
// <copyright file="TpPainterGridSelPanel.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
#nullable enable
namespace TilePlus.Editor.Painter
{
    /// <summary>
    /// information about what is selected in the list.
    /// </summary>
    [Serializable]
    public class SelectionElement
    {
        /// <summary>
        /// The Selection Bounds
        /// </summary>
        [SerializeField]
        public BoundsInt m_BoundsInt = new(new Vector3Int(0,0,0), new Vector3Int(1,1,1)); //size will be 1,1,1 if default ctor used

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="boundsInt">the selection bounds</param>
        public SelectionElement(BoundsInt boundsInt)
        {
            m_BoundsInt = boundsInt;
            var size = m_BoundsInt.size;
            if(size.z <= 0)
                size.z           = 1; 
            m_BoundsInt.size = size;
        }

        /// <summary>
        /// Base Ctor
        /// </summary>
        public SelectionElement()
        {
        }

    }
    
    
    
    /// <summary>
    /// Visual Element for the Grid Selection panel 
    /// </summary>
    public class TpPainterGridSelPanel : VisualElement
    {
        #region privateFields
        //UiElements refs
        private readonly TpListView listView;
        private readonly Button     overlayButton;
        private readonly Button     gridSelButton;
        private readonly Button     mapClearButton;
        private readonly Button     makeFabButton;
        private readonly Button     clearButton;
        private readonly Button     deselectButton;

        private List<SelectionElement>   selectionElements = new();
        private bool                     selecting;

         
        private readonly string basicHelpText = "Create and manage Grid Selections and Marquees. Click the button for more information";
        
        #endregion
        
        #region privateProperties
       
        private string ExpandedHelpText
        {
            get
            {
                var binding = TpPainterShortCuts.MarqueeDragTooltip;  //ShortcutManager.instance.GetShortcutBinding("TilePlus/Painter/MarqueeDrag [C]");
                return $"GridSelection management. When this panel is visible, select a Tilemap, then add a GridSelection with the Palette or hold down the shortcut key ({binding}) while dragging.\n\n"
                       + "To use, select a BoundsInt below, then click OVERLAY to just show the bounds, or GRID SELECTION to also create a new Grid Selection.\n"
                       + "\nIt's active until DESELECT is clicked, the Painter isn't the active Tool, or something else creates a Grid Selection."
                       + "\n\nIf you create a Grid Selection using the GRID SELECTION button or if there's an active one (say, created using the Palette) then the\n"
                       + "Clear Map and Create Chunk buttons will be active. Clear map uses the active Grid Selection to invoke the Clear Selected Tilemaps menu function.\n"
                       + "Create Chunk emulates the Bundle Tilemaps menu function. These two are conveniences only.\n\n" +
                       "This tool works best when the Tilemap has an origin of (0,0,0) or integer offsets from zero such as (1,1,0).";        
            }
        }
        
        #endregion

        #region publicFields
        /// <summary>
        /// The active grid selection. 
        /// </summary>
        internal SelectionElement? m_ActiveGridSelection;
        #endregion
        
        #region Ctor
        #pragma warning disable CS8618
        /// <summary>
        /// Ctor for panel
        /// </summary>
        public TpPainterGridSelPanel()
            #pragma warning restore CS8618
        {
            m_ActiveGridSelection = null;
           
            selectionElements.Clear();
            foreach(var item in TpPainterGridSelections.instance.m_GridSelectionWrappers)
                selectionElements.Add(new SelectionElement(item.m_BoundsInt));
                
            style.flexGrow          = 1;
            
            style.borderLeftWidth   = 4;
            style.borderRightWidth  = 4;
            style.borderBottomWidth = 2;
            style.borderTopWidth    = 2;
            style.borderBottomColor = Color.black;
            style.borderLeftColor = Color.black;
            style.borderRightColor = Color.black;
            style.borderTopColor = Color.black;
            
            style.paddingBottom           = 2;
            style.paddingLeft             = 2;
            style.paddingTop              = 2;
            style.paddingRight            = 2;
            

            var helpContainer = new VisualElement() { style = { flexDirection = FlexDirection.Row } };
            
            var helpLabel = new Label(basicHelpText)
                            {
                                userData = false,
                                style    = {whiteSpace = WhiteSpace.Normal, flexGrow = 1, flexShrink = 0.25f}
                            };
            
           
            
            var iconSize = TilePlusConfig.instance.SelInspectorButtonSize;
            helpContainer.Add(new Button((() =>
                                          {
                                              var expanded = (bool)helpLabel.userData;
                                              helpLabel.text = expanded
                                                                   ? basicHelpText
                                                                   : ExpandedHelpText;
                                              helpLabel.userData = !expanded;
                                          })){style =
                                             {
                                                 backgroundImage = TpIconLib.FindIcon(TpIconType.InfoIcon),
                                                 flexGrow        = 1,
                                                 height = iconSize,
                                                 width = iconSize,
                                                 maxHeight = iconSize,
                                                 maxWidth = iconSize
                                                 
                                             }});
            helpContainer.Add(helpLabel);
            Add(helpContainer);
            Add(new TpSpacer(8, 20));

            Add(listView = new TpListView(selectionElements,
                                          32,
                                          false,
                                          MakeItem,
                                          BindItem,
                                          AlternatingRowBackground.ContentOnly));
            listView.reorderable                   = true;
            listView.showAddRemoveFooter           = true;
            listView.selectionType                 = SelectionType.Single; 

            var scrollView = this.Q<ScrollView>(null, "unity-scroll-view");
            if(scrollView != null)
                scrollView.style.borderBottomWidth = 2;
            
            //next 2 lines handy for debug only
            /*listView.showFoldoutHeader             = true; 
            listView.showBoundCollectionSize       = true;*/
            
            listView.itemsRemoved     += ListViewOnitemsRemoved;
            listView.itemIndexChanged += ListViewOnitemIndexChanged;
            
            listView.Q<Button>("unity-list-view__add-button").visible =  false;
            
            Add(new TpSpacer(20,10));
            var buttonContainer = new VisualElement() { style = {minHeight  = 30,flexGrow = 0, flexDirection = FlexDirection.Row } };
            Add(buttonContainer);
            clearButton= new Button(() =>
                            {
                                TpLib.DelayedCallback(TilePlusPainterWindow.RawInstance, () =>
                                {
                                    if (!EditorUtility.DisplayDialog("Confirm", "Please confirm clearing this list", "YEP", "NOPE!"))
                                        return;
                                    Deselect();
                                    TpPainterGridSelections.instance.m_GridSelectionWrappers.Clear();
                                    TpPainterGridSelections.instance.SaveData();
                                    selectionElements.Clear();
                                    listView.Rebuild();

                                },"T+P:GridSel-Clear");
                            }){style = {flexGrow = 0}, text = "Clear", tooltip = "Delete all the items above. Confirmation required."};
            buttonContainer.Add(clearButton);
            deselectButton = new Button(Deselect){style = {flexGrow = 0}, text = "Deselect", tooltip = "Clear the overlay or Grid Selection"};
            buttonContainer.Add(deselectButton);

            var schItem = buttonContainer.schedule.Execute(UpdateEvent);
            schItem.Every(500);
            overlayButton = new Button(Overlay){ text = "Overlay", style        = { flexGrow = 0, }, tooltip = "Show as an overlay, do not create Grid Selection."};
            gridSelButton = new Button(GridSel){ text = "Grid Selection", style = { flexGrow = 0, }, tooltip =  "Show as an overlay and create a Grid Selection."};
            mapClearButton = new Button(MapClear) { text  = "Clear map", style = { flexGrow = 0, }, tooltip = "Erase the area defined by the active Grid Selection." };
            makeFabButton  =  new Button(MakeChunk) { text = "Create Chunk", style = { flexGrow = 0, } ,tooltip = "Create a TileFab Chunk from the area defined by the active Grid Selection."};
            
            buttonContainer.Add(overlayButton);
            buttonContainer.Add(gridSelButton);
            buttonContainer.Add(mapClearButton);
            buttonContainer.Add(makeFabButton);

        }

        private void MapClear()
        {
            var win         = TilePlusPainterWindow.RawInstance;
            var paintTarget = win!.TilemapPaintTarget;
            if(paintTarget != null && paintTarget.ParentGridTransform != null)
                TpEditorUtilities.ClearSelectedTilemaps(paintTarget.ParentGridTransform.gameObject,GridSelection.position);
        }
        private void MakeChunk()
        {
            TpPrefabUtilities.Bundle();
        }


        private void UpdateEvent()
        {
            var win = TilePlusPainterWindow.RawInstance;
            if (win == null || win.GlobalMode != GlobalMode.GridSelView)
                return;
            
            
            var selected  = listView.selectedItem;
            var selection = selected != null;
            
            
            var paintTarget       = win.TilemapPaintTarget;
            var showActionButtons = paintTarget is { Valid: true } && selection;
            overlayButton.SetEnabled(showActionButtons);
            gridSelButton.SetEnabled(showActionButtons);
            mapClearButton.SetEnabled(showActionButtons && GridSelection.active);
            makeFabButton.SetEnabled(showActionButtons && GridSelection.active);
            clearButton.SetEnabled(listView.itemsSource.Count != 0);
            deselectButton.SetEnabled(m_ActiveGridSelection != null);
        }

        private void GridSel()
        {
            var win         = TilePlusPainterWindow.RawInstance;
            if(win == null)
                return;
            var paintTarget = win.TilemapPaintTarget;
            if(paintTarget is not { Valid: true })
                return;

            if(listView.selectedItem is not SelectionElement item)
                return;
            
            //don't want to add another GridSelection frivolously.
            //Actually this creates other issues, eg click Overlay then this code fails... 
            /*if(m_ActiveGridSelection !=null && m_ActiveGridSelection.m_BoundsInt  == item.m_BoundsInt) 
                return;*/
            Tilemap? map;
            if ((map = paintTarget.TargetTilemap) != null)
            {
                Selecting = true;
                var bounds = item.m_BoundsInt;
                var gridLayout  = map.layoutGrid;
                bounds.position += gridLayout.LocalToCell(map.transform.localPosition);
                GridSelection.Select(map.gameObject, bounds);
                Selecting = false;
            }

             
            m_ActiveGridSelection = item;

        }

        private void Overlay()
        {
            var win  = TilePlusPainterWindow.RawInstance;
            if(win == null)
                return;
            var paintTarget = win.TilemapPaintTarget;
            if(paintTarget is not { Valid: true })
                return;

            if(listView.selectedItem is not SelectionElement item)
                return;
             
            m_ActiveGridSelection = item;
            
        }

        #endregion
       
        #region control
        internal void Deselect()
        {
            //Debug.Log("Desel");
            if(selectionElements.Count != 0)
                listView.SetSelectionWithoutNotify(new []{-1});
            m_ActiveGridSelection = null;
            Selecting = true; //interlock to prevent false messages like "duplicate selection"
            GridSelection.Clear();
            Selecting = false;
        }

        private void ListViewOnitemIndexChanged(int idBeingMoved, int destinationId)
        {
            TpPainterGridSelections.instance!.m_GridSelectionWrappers = selectionElements
                                                      .Select(se => new GridSelectionWrapper(se.m_BoundsInt))
                                                      .ToList();
            TpPainterGridSelections.instance.SaveData();
            listView.Rebuild();
        }

       

        private void ListViewOnitemsRemoved(IEnumerable<int> objs)
        {
            //var index = objs.First();
            selectionElements                       = (List<SelectionElement>)listView.itemsSource;
            TpPainterGridSelections.instance!.m_GridSelectionWrappers = selectionElements.Select((se) => new GridSelectionWrapper(se.m_BoundsInt)).ToList();
            TpPainterGridSelections.instance.SaveData();
        }

        private bool Selecting { get; set; }
        internal void AddGridSelection(Object target, BoundsInt boundsInt)
        {
            Selecting = true;
            AddGridSelection(boundsInt);
            //todo GridSelection.Select(target, boundsInt);
            Selecting = false;
        }

        internal void AddGridSelection(BoundsInt boundsInt, bool silent = false)
        {
            if (Selecting)
            {
                //Debug.Log("Selection in prog");
                Selecting = false;
                return;
            }
            

            if (TpPainterGridSelections.instance!.m_GridSelectionWrappers.Count >= 64)
            {
                
                TpLib.DelayedCallback(TilePlusPainterWindow.RawInstance, () =>
                                                                         {
                                                                             EditorUtility.DisplayDialog("This one cannot comply!", "Too many items in Grid Selection list. Remove some....", "Continue", "");
                                                                         },"T+P:Gridsel-too-many-items");
                return;
            }

            if (boundsInt.size == Vector3Int.zero || boundsInt.size == new Vector3Int(0,0,1))
            {
                if(!silent)
                    TpLib.DelayedCallback(TilePlusPainterWindow.RawInstance, () =>
                                                                         {
                                                                             EditorUtility.DisplayDialog("Bad GridSelection!", "Grid Selection size was zero.", "Continue", "");
                                                                         },"T+P:Gridsel-zero-size");
                return;
            }

            //no dupes
            if (TpPainterGridSelections.instance.m_GridSelectionWrappers.Any(gs => gs.m_BoundsInt == boundsInt))
            {
                if(!silent)
                    TpLib.DelayedCallback(TilePlusPainterWindow.RawInstance, () =>
                                                                         {
                                                                             EditorUtility.DisplayDialog("No duplicates!", "This Grid Selection exists already.", "Continue", "");
                                                                         },"T+P:Gridsel-too-many-items");
                return;
            }
            
            TpPainterGridSelections.instance.m_GridSelectionWrappers.Add(new GridSelectionWrapper(boundsInt));
            TpPainterGridSelections.instance.SaveData();
            selectionElements.Clear();
            foreach(var item in TpPainterGridSelections.instance.m_GridSelectionWrappers)
                selectionElements.Add(new SelectionElement(item.m_BoundsInt));

            listView.Rebuild();
        }


        
        
        #endregion
        
        
        #region make_bind

        private VisualElement MakeItem()
        {
            var container = new TpListBoxItem("selection-element", Color.black){style = 
                                                                               { 
                                                                                   marginTop = 2, 
                                                                                   marginBottom = 2}
                                                                               };
            container.Add(new Label(){name="field-label", style = {flexGrow = 0}});
            var field = new BoundsIntField()
                        {
                            focusable = false,
                            style =
                            {
                                flexGrow = 1
                            },
                            name  = "boundsint-field",
                            value = new BoundsInt(Vector3Int.zero, Vector3Int.forward)
                        }; //v3i.forward makes the size = 0,0,1
            
            container.Add(field); 
            return container;
        }

        

        private void BindItem(VisualElement ve, int index)
        {
            var field = ve.Q<BoundsIntField>("boundsint-field");
            var label = ve.Q<Label>("field-label");
            label.text  = (index + 1).ToString();
            field.SetValueWithoutNotify(selectionElements[index].m_BoundsInt);
        }

        #endregion
    
    }
}
