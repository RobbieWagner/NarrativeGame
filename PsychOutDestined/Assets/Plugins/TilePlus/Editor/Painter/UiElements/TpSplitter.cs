// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 01-01-2023
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 12-26-2022
// ***********************************************************************
// <copyright file="TpSplitter.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>Custom TwoPaneSplitView</summary>
// ***********************************************************************

using JetBrains.Annotations;
using TilePlus.Editor.Painter;
using UnityEngine.UIElements;

namespace TilePlus.Editor
{
    
    /// <summary>
    /// Custom TwoPaneSplitView
    /// </summary>
    public class TpSplitter : TwoPaneSplitView
    {
        private int lastSizeX; 
        private int lastSizeY;
       
        //need this for inheritance.
        public TpSplitter() { }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="elementName">name of this element</param>
        /// <param name = "viewDataKeyString" >persistence data string</param>
        /// <param name = "initialDimensionForFixedPanel" >initial dim for fixed panel</param>
        /// <param name = "splitViewOrientation" >orientation of SV</param>
        /// <param name="fixedPaneIndexNumber">which panel is fixed?</param>
        /// <param name = "geometryChangedCallback" >Callback for Geometry change</param>
        /// <returns></returns>
        public TpSplitter(string                               elementName,
                          string                               viewDataKeyString,
                          float                                initialDimensionForFixedPanel,
                          TwoPaneSplitViewOrientation          splitViewOrientation    = TwoPaneSplitViewOrientation.Horizontal,
                          int                                  fixedPaneIndexNumber    = 1,
                          [CanBeNull] EventCallback<GeometryChangedEvent> geometryChangedCallback = null)
        {
            fixedPaneInitialDimension = initialDimensionForFixedPanel;
            fixedPaneIndex            = fixedPaneIndexNumber;
            orientation               = splitViewOrientation;
            viewDataKey               = viewDataKeyString;
            name                      = elementName;
            style.minWidth            = 100;
            style.minHeight           = 100;

            if(geometryChangedCallback != null) 
                RegisterCallback(geometryChangedCallback);
            
            var splitter = this.Q("unity-dragline-anchor");
            splitter.style.width = TilePlusPainterWindow.SplitterSize;
            
        }


        /// <summary>
        /// Returns true when this is properly set up
        /// </summary>
        public bool Valid => fixedPane != null && flexedPane != null;
        

        /// <summary>
        /// Is one of the panes collapsed? The bool in TwoPaneSplitView is private
        /// for some reason (be nice to have a property).
        /// </summary>
        public bool IsCollapsed =>
            (fixedPane.style.display == DisplayStyle.None) ||
            (flexedPane.style.display == DisplayStyle.None);
    }
}
