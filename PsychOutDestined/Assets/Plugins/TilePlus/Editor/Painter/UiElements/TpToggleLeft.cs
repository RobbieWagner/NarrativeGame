// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 01-01-2023
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 12-22-2022
// ***********************************************************************
// <copyright file="TpToggleLeft.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>Custom Toggle</summary>
// ***********************************************************************

using UnityEngine.UIElements;

namespace TilePlus.Editor
{
    public class TpToggleLeft : Toggle
    {
        public TpToggleLeft() { }

        public TpToggleLeft(string label) : base(label)
        {
            style.flexDirection                                                = FlexDirection.RowReverse;
            style.alignSelf                                                    = Align.FlexStart;
            this.Q<VisualElement>("", "unity-toggle__input").style.marginRight = 4;
        }
    }
}
