// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 01-01-2023
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 01-05-2023
// ***********************************************************************
// <copyright file="TpPainterRuleOverrideTilePlugIn.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>Plugin for Rule Override tiles </summary>
// ***********************************************************************
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TilePlus.Editor.Painter
{
    /// <summary>
    /// Plug-in to support Rule Override Tiles in Tile+Painter
    /// </summary>
    public class TpPainterRuleOverrideTilePlugIn : TpPainterPluginBase
    {
       /// <inheritdoc />
        public override Type GetTargetTileType => typeof(RuleOverrideTile);

        /// <inheritdoc />
        public override Sprite GetSpriteForTile(TileBase tileBase)
        {
            return tileBase == null || tileBase is not RuleOverrideTile rt
                       ? null
                       :  rt.m_Sprites[0].m_OriginalSprite; //hard to say what's right here...  
        }
    }
}
