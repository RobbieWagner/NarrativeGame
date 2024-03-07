// ***********************************************************************
// Assembly         : TilePlus.Editor
// Author           : Jeff Sasmor
// Created          : 11-09-2022
//
// Last Modified By : Jeff Sasmor
// Last Modified On : 12-31-2022
// ***********************************************************************
// <copyright file="TpPainterClasses.cs" company="Jeff Sasmor">
//     Copyright (c) Jeff Sasmor. All rights reserved.
// </copyright>
// <summary>Data structures for Tile+Painter</summary>
// ***********************************************************************
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
#nullable enable
namespace TilePlus.Editor.Painter 
{
    /// <summary>
    /// Describes what type of thing to display after a palette or tile is selected
    /// </summary>
    [Serializable]
    internal class TargetTileData
    {
        public enum Variety
        {
            
            /// <summary>
            /// This instance is a Unity or TPT tile
            /// </summary>
            TileItem,
            /// <summary>
            /// This instance is a TpTileBundle asset
            /// </summary>
            BundleItem,
            /// <summary>
            /// This instance is a TileFabItem
            /// </summary>
            TileFabItem,
            /// <summary>
            /// Represents an empty field. 
            /// </summary>
            EmptyItem
            
            
        }
        /// <summary>
        /// A Tile reference. May be either a Unity or TPT tile
        /// </summary>
        public TileBase? Tile { get; }  
        /// <summary>
        /// the Type of the tile. Only valid when Tile != null. In
        /// that case the type will always be TileBase
        /// </summary>
        public Type? TileType { get; } 
        /// <summary>
        /// The position, only used when variety is TileItem. 
        /// </summary>
        public Vector3Int Position      { get; }
        
        
        /// <summary>
        /// What sort of thing is this?
        /// </summary>
        public Variety ItemVariety { get; }

        /// <summary>
        /// Is this data instance valid?
        /// </summary>
        public bool Valid
        {
            get
            {
                if (ItemVariety == Variety.BundleItem)
                    return Bundle != null;
                if (ItemVariety == Variety.TileFabItem)
                    return TileFab != null;
                
                //here, test for validity of tile items
                if (Tile == null) //tile being null is fail for any source
                    return false;

                //non-null tile but invalid grid position means History stack is where this tile came from.
                if (Position == TilePlusBase.ImpossibleGridPosition)  
                    return true;

                //any other source: tile or map can't be null and position needs to be valid.
                return SourceTilemap != null && Position != TilePlusBase.ImpossibleGridPosition;
            }
        }

        /// <summary>
        /// Instance Id of the contents, if applicable
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// Get a TpTileBundle instance 
        /// </summary>
        public TpTileBundle? Bundle { get; }
        /// <summary>
        /// Get a TpTileFab instance
        /// </summary>
        public TpTileFab? TileFab { get; }
        /// <summary>
        /// Get the tilemap for the contents, if applicable 
        /// 
        /// </summary>
        public Tilemap? SourceTilemap { get; }
        
        /// <summary>
        /// Is this a Tile or subclass? This will also be true for when IsTilePlusBase is true. 
        /// </summary>
        public bool IsTile { get; }
        /// <summary>
        /// Is this a TileBase class tile? Only true if NOT a Tile or TilePlusBase
        /// </summary>
        public bool IsTileBase { get; }
        /// <summary>
        /// Is this a TilePlusBase class. NOT true if TileBase class.
        /// </summary>
        public bool IsTilePlusBase { get; }
        /// <summary>
        /// Is this a clone of a TilePlusBase
        /// </summary>
        public bool IsClonedTilePlusBase { get; }
        
        /// <summary>
        /// TRUE when this is NOT a TilePlus tile
        /// </summary>
        public bool IsNotTilePlusBase { get; }
        
        /// <summary>
        /// TRUE if this item is from the History Stack.
        /// </summary>
        public bool IsFromHistory { get; }
        
        /// <summary>
        /// TRUE if the tile is a TPT or Tile AND the gameObject
        /// field isn't null. Note that this is NOT the
        /// instantiated GameObject that may be instantiated
        /// by the tilemap after calling GetTileData.
        /// </summary>
        public bool HasGameObject { get; }
        
        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// The transform for this tile, if appropriate
        /// </summary>
        public Matrix4x4 transform { get; private set; }


        private readonly Matrix4x4 originalTransform;

        /// <summary>
        /// Indicates that the transform has been modified
        /// </summary>
        [FormerlySerializedAs("transformModified")]
        [SerializeField]
        private bool m_TransformModified;

        /// <summary>
        /// Is the transform modified? For TileItem variety only.
        /// </summary>
        public bool TransformModified
        {
            get => m_TransformModified;
            private set
            {
                m_TransformModified = value;
                if (TilePlusPainterWindow.RawInstance != null)
                    TilePlusPainterWindow.RawInstance.TabBar.SetTabTilePickedIconAsTransformModified(value, this);
            }
        }

        [SerializeField]
        private bool m_WasPickedTile;

        /// <summary>
        /// Was this a picked tile?
        /// </summary>
        public bool WasPickedTile => m_WasPickedTile;

        /// <summary>
        /// A standard tile item
        /// </summary>
        /// <param name="aTile"></param>
        /// <param name="aPosition">Tile position : if ImpossibleGridPositon then source can be null</param>
        /// <param name = "source" >Source tilemap or null for history (for example)</param>
        /// <param name="wasPickedTile">was this a picked tile?</param>
        /// <remarks>If aPostion is ImpossibleGridPosition then this tile is treated as coming from the History
        /// list, where the tilemap source isn't available.</remarks>
        public TargetTileData(TileBase? aTile, Vector3Int aPosition, Tilemap? source, bool wasPickedTile = false) 
        {
            switch (aPosition == TilePlusBase.ImpossibleGridPosition) //this pos means its from History stack.
            {
                case false when source == null:
                    TpLib.TpLogError("NULL tilemap passed to TileTargetData Ctor");
                    break;
                case true:
                    IsFromHistory = true;
                    wasPickedTile = false; //ensures use of tile's transform for items from History Stack.
                    break;
            }

            Tile          = aTile;
            
            if (aTile != null && (source != null || IsFromHistory))
            {
                Id       = aTile.GetInstanceID();
                TileType = aTile.GetType();
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (aTile is ITilePlus itp)
                {
                    IsClonedTilePlusBase = itp.IsClone; 
                    IsTilePlusBase       = true;
                    IsTile               = true;
                    HasGameObject        = ((Tile)aTile).gameObject != null;
                    transform            = wasPickedTile && source!=null ? source.GetTransformMatrix(aPosition) : (((aTile as Tile)!)).transform;
                }
                else if (aTile is Tile tile)
                {
                    IsTile            = true;
                    IsNotTilePlusBase = true;
                    HasGameObject     = tile.gameObject != null;
                    transform         = wasPickedTile && source!= null ? source.GetTransformMatrix(aPosition) :  tile.transform;
                }
                else
                {
                    IsTileBase        = true; //note TileBase has no transform NOR color, sprite etc.
                    IsNotTilePlusBase = true;
                }
            }
            else
                TileType = typeof(TileBase);
            
            Position          = aPosition;
            m_WasPickedTile     = wasPickedTile;
            SourceTilemap     = source;
            ItemVariety       = Variety.TileItem;
            originalTransform = transform; //saved for use when reverting from custom transform.
        }

        public TargetTileData(TpTileBundle? bundle)
        {
            Bundle       = bundle;
            ItemVariety = Variety.BundleItem;
        }
        
        public TargetTileData(TpTileFab? fab)
        {
            TileFab       = fab;
            ItemVariety = Variety.TileFabItem;
        }

        public TargetTileData()
        {
            ItemVariety = Variety.EmptyItem;
        }
        

        public void Rotate(bool ccw = false)
        {
            if (ItemVariety != Variety.TileItem || IsTileBase)
                return;
            transform         *= TileUtil.RotatationMatixZ(ccw ? 90 : -90);
            TransformModified =  true;
        }

        public void Flip(bool flipX = false)
        {
            if (ItemVariety != Variety.TileItem || IsTileBase)
                return;
            transform *= TileUtil.ScaleMatrix(flipX ? new Vector3(-1, 1,  1)
                                                  : new Vector3(1,    -1, 1),
                                              Vector3Int.zero);
            TransformModified =  true;
        }
        
        public void Reset()
        {
            if (ItemVariety != Variety.TileItem || IsTileBase)
                return;
            transform         = Matrix4x4.identity;
            TransformModified = false;
        }

        public void Apply(Matrix4x4 newTransform)
        {
            if (ItemVariety != Variety.TileItem || IsTileBase)
                return;
            transform         = newTransform;
            TransformModified = true;
        }

        public void Restore()
        {
            if (!TransformModified || ItemVariety != Variety.TileItem || IsTileBase)
                return;
            transform         = originalTransform;
            TransformModified = false;
            m_WasPickedTile = false;
        }

    }

    /// <summary>
    /// A data structure for items in the palette list (Center column)
    /// </summary>
    [Serializable]
    internal class PaletteListItem
    {
        /// <summary>
        /// Reference to a Palette, if appropriate
        /// </summary>
        public GameObject? Palette { get; }
        /// <summary>
        /// Reference to a TpTileBundle asset, if appropriate
        /// </summary>
        public TpTileBundle? Bundle { get; }
        /// <summary>
        /// Reference to a TileFab, if appropriate.
        /// </summary>
        public TpTileFab? TileFab { get; }
        /// <summary>
        /// What's in this instance? 
        /// </summary>
        public TpPaletteListItemType ItemType { get; }
        /// <summary>
        /// Name of this item
        /// </summary>
        public string ItemName { get; }

        [SerializeField]
        // ReSharper disable once InconsistentNaming
        private int count;
        
        /// <summary>
        /// Get a count appropriate for the contents
        /// </summary>
        public int Count
        {
            get
            {
                switch (ItemType)
                {
                    case TpPaletteListItemType.History:
                    {
                        var instance = TilePlusPainterWindow.RawInstance;
                        return instance != null
                                   ? instance.HistoryStackSize
                                   : 0;
                    }
                    case TpPaletteListItemType.Bundle:
                        return Bundle != null ? Bundle.m_UnityTiles.Count + Bundle.m_TilePlusTiles.Count : 0;
                    case TpPaletteListItemType.TileFab:
                        return TileFab != null  ?  TileFab.m_TileAssets!.Count : 0;
                    default:
                        return count;
                }
            }
            private set => count = value;
        }

        /// <summary>
        /// how many items in the asset described by this instance?
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        // ReSharper disable once UnusedMember.Local
        private int CurrentCount
        {
            get
            {
                switch (ItemType)
                {
                    case TpPaletteListItemType.Palette: return Palette != null ? Palette.GetComponentInChildren<Tilemap>().GetUsedTilesCount() : 0;
                    case TpPaletteListItemType.TileFab: return TileFab != null ?  TileFab.m_TileAssets!.Count :0 ;
                    case TpPaletteListItemType.Bundle:  return Bundle != null ? Bundle.m_UnityTiles.Count + Bundle.m_TilePlusTiles.Count : 0;
                    case TpPaletteListItemType.History:
                        var instance = TilePlusPainterWindow.instance;
                        return instance != null
                                   ? instance.HistoryStackSize
                                   : 0;
                    case TpPaletteListItemType.None:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Ctor when this instance is describing a Unity palette
        /// </summary>
        /// <param name="palette">a Palette ref</param>
        public PaletteListItem(GameObject palette)
        {
            Palette  = palette;
            ItemType = TpPaletteListItemType.Palette;
            ItemName = palette.name;
            Count    = palette.GetComponentInChildren<Tilemap>().GetUsedTilesCount();
        }

        /// <summary>
        /// Ctor when this instance is describing a TpTileBundle asset
        /// </summary>
        /// <param name="bundle">a TpTileBundle asset ref</param>
        public PaletteListItem(TpTileBundle bundle)
        {
            Bundle    = bundle;
            ItemType = TpPaletteListItemType.Bundle;
            ItemName = bundle.name;
            Count    = bundle.m_UnityTiles.Count + bundle.m_TilePlusTiles.Count;
        }

        /// <summary>
        /// Ctor when this instance is describing the History List.
        /// </summary>
        public PaletteListItem()
        {
            
            ItemType    = TpPaletteListItemType.History;
            ItemName    = "History List";
            Count       = -1;
        }

        /// <summary>
        /// Ctor when this instance is describing a TileFab asset
        /// </summary>
        /// <param name="tileFab">TileFab asset ref</param>
        public PaletteListItem(TpTileFab tileFab)
        {
            TileFab  = tileFab;
            ItemType = TpPaletteListItemType.TileFab;
            ItemName = tileFab.name;
            Count    = tileFab.m_TileAssets!.Count;
        }
        
        
    }

    /// <summary>
    /// describes a tilemap we can paint on
    /// </summary>
    [Serializable]
    internal class PaintTarget
    {
        /// <summary>
        /// The tilemap that gets painted on
        /// </summary>
        public Tilemap? TargetTilemap { get; }
        /// <summary>
        /// The Grid Layout of the paintable tilemap
        /// </summary>
        public GridLayout? TargetTilemapGridLayout { get; }
        //the Transform of the paintable tilemap
        // ReSharper disable once MemberCanBePrivate.Global
        public Transform? TargetTilemapTransform { get; }
        /// <summary>
        /// The Transform of the parent grid.
        /// </summary>
        public Transform? ParentGridTransform { get; }
        /// <summary>
        /// The name of the tilemap
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// is this data strucure valid?
        /// </summary>
        public bool Valid { get; }
        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="map">tilemap</param>
        public PaintTarget(Tilemap? map)
        {
            TargetTilemap = map;
            if (map == null)
            {
                Debug.LogError("Passed NULL tilemap to PaintTarget ctor");
                return;
            }

            Name                    = map.name;
            TargetTilemapGridLayout = map.GetComponent<GridLayout>();
            TargetTilemapTransform  = map.transform;
            ParentGridTransform     = GetParentGridTransform(TargetTilemapTransform);

            if (ParentGridTransform == null)
            {
                Debug.LogError("Could not find tilemap's parent grid in PaintTarget ctor");
                return;
            }

            if (TargetTilemapTransform != null && TargetTilemapGridLayout != null)
                Valid = true;
            else
                Debug.LogError("Invalid data in PaintTarget ctor");

            //local method
            Transform? GetParentGridTransform(Transform current)
            {
                //perhaps the current transform has a Grid
                if (current.TryGetComponent<Grid>(out var output))
                    return output.transform;
                //otherwise, look at its parent. If == null then
                //current is a root transform.
                while ((current = current.parent) != null) 
                {
                    // ReSharper disable once RedundantTypeArgumentsOfMethod
                    if (current.TryGetComponent<Grid>(out output))
                        return output.transform;
                }
                return null;
            }
        }
    }
    

    /// <summary>
    /// Data used by the UIElements list. Describes a Tilemap
    /// </summary>
    [Serializable]
    internal class TilemapData
    {
        /// <summary>
        /// What's being described by this instance? A tile, a map, etc.
        /// </summary>

        /// <summary>
        /// Parent tilemap for tile
        /// </summary>
        public Tilemap TargetMap { get; }

        /// <summary>
        /// Is this tile or tilemap part of a prefab?
        /// </summary>
        public bool InPrefab { get; }
        
        /// <summary>
        /// Is this tile or tilemap being shown in a prefab stage
        /// </summary>
        public bool InPrefabStage { get; }

        /// <summary>
        /// The tilemap's parent scene
        /// </summary>
        public Scene ParentSceneOfMap { get; }

        /// <summary>
        /// TRUE if this map has TPT tiles in it.
        /// </summary>
        public bool HasTptTilesInMap => TpLib.IsTilemapRegistered(TargetMap);

        /// <summary>
        /// Constructor when desiring to display info for a tilemap
        /// </summary>
        /// <param name="mapRef">the map</param>
        /// <param name="inPrefab">is it in a prefab?</param>
        /// <param name = "inPrefabStage" >is it in a prefab STAGE (Editor)</param>
        public TilemapData(Tilemap mapRef, bool inPrefab, bool inPrefabStage)
        {
            TargetMap        = mapRef;
            InPrefab         = inPrefab;
            InPrefabStage    = inPrefabStage;
            ParentSceneOfMap = mapRef.gameObject.scene;
        }

    }
   
}
