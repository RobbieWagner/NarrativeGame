// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 01-01-2023
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 01-05-2023
// ***********************************************************************
// <copyright file="TpPainterTransforms.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>Plugin for T+Painter</summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TilePlus.Editor.Painter
{
    /// <summary>
    /// A wrapper class for the transform
    /// </summary>
    [Serializable]
    public class TileTransformWrapper
    {
        /// <summary>
        /// transform matrix
        /// </summary>
        [SerializeField]
        public Matrix4x4 m_Matrix;

        /// <summary>
        /// Ctor
        /// </summary>
        public TileTransformWrapper()
        {
            m_Matrix =  Matrix4x4.identity;
        }
    }
    
    /// <summary>
    /// Asset for applying transforms to tiles as they're being painted by T+Painter.
    /// Note that there should only be one of these.
    /// Note that the other
    /// save option is FilePathAttribute.Location.PreferencesFolder which will cause
    /// all sorts of issues if you have multiple Unity instances open at the same time
    /// since they'll all try to ref this at the same time and all sorts of 'stuff' goes
    /// bezerko.
    /// </summary>
    [FilePath("TpConfig/TpPainterTransforms.asset",FilePathAttribute.Location.ProjectFolder)]
    public class TpPainterTransforms : ScriptableSingleton<TpPainterTransforms>
    {
        /// <summary>
        /// A list of transform wrappers
        /// </summary>
        public List<TileTransformWrapper> m_PTransformsList = new (1);
        /// <summary>
        /// Which transform is active
        /// </summary>
        public int                        m_ActiveIndex = -1;

        public void SaveData()
        {
            Save(true);
        }
        

    }

    
    #if UNITY_EDITOR
    /// <summary>
    /// Editor for TpSpawnLink component
    /// </summary>
    [CustomEditor(typeof(TpPainterTransforms))]
    public class TpPainterTransformsEditor : UnityEditor.Editor
    {
        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Please use the Transforms Editor!",MessageType.Warning);

            var tgt = (TpPainterTransforms)target;
            var num = tgt.m_PTransformsList.Count;
            EditorGUILayout.HelpBox($"There are {num} items in this asset, current selection is {tgt.m_ActiveIndex}", MessageType.None);
            
        }
    }
    #endif  
}
