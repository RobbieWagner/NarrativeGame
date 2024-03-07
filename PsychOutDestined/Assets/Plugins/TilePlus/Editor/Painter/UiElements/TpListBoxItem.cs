// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 01-01-2023
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 01-22-2023
// ***********************************************************************
// <copyright file="TpListBoxItem.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>Custom list box item</summary>
// ***********************************************************************

using UnityEngine;
using UnityEngine.UIElements;

namespace TilePlus.Editor
{

    /// <summary>
    /// A list box item for the Tile+Painter
    /// </summary>
    public class TpListBoxItem : VisualElement
    {
        /// <summary>
        /// Create a list box item
        /// </summary>
        /// <param name = "elementName" >Name of this element</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width (1)</param>
        /// <param name="radius">Border radius (4)</param>
        public TpListBoxItem(string elementName,  Color borderColor,  float borderWidth = 1f, float radius = 4f )
        {
            name                          = elementName;
            style.flexDirection           = FlexDirection.Row;
            style.alignItems              = Align.Center;
            style.borderBottomWidth       = borderWidth;
            style.borderTopWidth          = borderWidth;
            style.borderRightWidth        = borderWidth;
            style.borderLeftWidth         = borderWidth;
            style.borderBottomColor       = borderColor;
            style.borderTopColor          = borderColor;
            style.borderLeftColor         = borderColor;
            style.borderRightColor        = borderColor;
            style.borderBottomLeftRadius  = radius;
            style.borderBottomRightRadius = radius;
            style.borderTopLeftRadius     = radius;
            style.borderTopRightRadius    = radius;
        }
          
    }
    
    
}
