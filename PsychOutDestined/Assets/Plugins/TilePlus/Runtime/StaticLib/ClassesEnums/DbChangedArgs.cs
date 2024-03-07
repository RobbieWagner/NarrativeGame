// ***********************************************************************
// Assembly         : TilePlus
// Author           : Jeff Sasmor
// Created          : 07-29-2021
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 05-23-2021
// ***********************************************************************
// <copyright file="DbChangedArgs.cs" company="Jeff Sasmor">
//     Copyright (c) 2021 Jeff Sasmor. All rights reserved.
// </copyright>
// ***********************************************************************

using UnityEngine;
using UnityEngine.Tilemaps;

namespace TilePlus
{
    /// <summary>
    /// Contains information about what has
    /// changed in the TpLib tile data.
    /// </summary>
    public class DbChangedArgs
    {
        /// <summary>
        /// The type of change that occurred
        /// </summary>
        public enum ChangeType
        {
            /// <summary>
            /// Something was added
            /// </summary>
            Added,
            /// <summary>
            /// Something was added to a previously-empty tilemap
            /// </summary>
            AddedToEmptyMap,

            /// <summary>
            /// Something was deleted
            /// </summary>
            Deleted,
            /// <summary>
            /// Something was modified
            /// </summary>
            Modified,
            /// <summary>
            /// Something was either modified or added.
            /// ONLY for Tile or TileBase class instances
            /// Note that position will be invalid - use 
            /// </summary>
            ModifiedOrAdded,
            /// <summary>
            /// Tags were modified 
            /// </summary>
            TagsModified
        }

        /// <summary>
        /// True for an addition, false for a deletion
        /// </summary>
        public ChangeType m_ChangeType;


        /// <summary>
        /// Is this part of a group? In editor, you might want to
        /// cache this until OnTilemapDbChangedGroup fires. Note:
        /// this is only true in Editor sessions.
        /// </summary>
        public readonly bool m_IsPartOfGroup;

        /// <summary>
        /// The Tile location
        /// </summary>
        public  Vector3Int m_GridPosition = TilePlusBase.ImpossibleGridPosition;

        /// <summary>
        /// The tilemap where changes occurred
        /// </summary>
        public  Tilemap m_Tilemap;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="changeType">Value of ChangeType enum</param>
        /// <param name="isPartOfGroup">part of group or a singleton</param>
        /// <param name="gridPosition">The position affected</param>
        /// <param name="map">The tilemap affected: NOTE can be null, eg when entire tilemaps are deleted.</param>
        public DbChangedArgs(ChangeType changeType, bool isPartOfGroup, Vector3Int gridPosition, Tilemap map)
        {
            m_ChangeType = changeType;
            #if UNITY_EDITOR
            m_IsPartOfGroup = isPartOfGroup;
            #else
            m_IsPartOfGroup = false;
            #endif
            m_GridPosition = gridPosition;
            m_Tilemap      = map;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public DbChangedArgs() {}
        
    }
}
