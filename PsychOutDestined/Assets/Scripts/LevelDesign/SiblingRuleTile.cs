using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TilePlus.Editor;
using TilePlus;

namespace PsychOutDestined
{
    [CreateAssetMenu]
    public class SiblingRuleTile : RuleTile
    {
        public enum SiblingGroup
        {
            None = -1,
            Default = 0,
            Land = 1,
            Indoors1 = 2,
            Indoors2 = 3,
            Water = 4,
        }

        public SiblingGroup siblingGroup;
        public bool hideSpriteInGame = true;

        public override bool RuleMatch(int neighbor, TileBase other)
        {
            if (other is RuleOverrideTile)
                other = (other as RuleOverrideTile).m_InstanceTile;

            switch (neighbor)
            {
                case TilingRule.Neighbor.This:
                    {
                        return other is SiblingRuleTile
                            && (other as SiblingRuleTile).siblingGroup == this.siblingGroup;
                    }
                case TilingRule.Neighbor.NotThis:
                    {
                        return !(other is SiblingRuleTile
                            && (other as SiblingRuleTile).siblingGroup == this.siblingGroup);
                    }
            }

            return base.RuleMatch(neighbor, other);
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
            if(!TpLib.IsTilemapFromPalette(tilemap) && hideSpriteInGame)
                tileData.sprite = null;
        }
    }
}
