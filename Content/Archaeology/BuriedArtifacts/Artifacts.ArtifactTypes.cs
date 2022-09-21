using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Archaeology;
using StarlightRiver.Content.Items.BuriedArtifacts;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Archaeology.BuriedArtifacts //using empty classes instead of interfaces so I can easily read them via Mod.GetContent in generation
{
    public abstract class OceanArtifact : Artifact { } 

    public abstract class LavaArtifact : Artifact { }

    public abstract class DesertArtifact : Artifact { }

    public abstract class UndergroundArtifact : Artifact { }
    
    public abstract class JungleArtifact : Artifact 
    {
        public override bool CanGenerate(int i, int j) //Make sure it doesn't generate near mushroom grass
        {
            bool ret = false;

            for (int x = -15; x < (Size.X / 16) + 15; x++)
                for (int y = -15; y < (Size.Y / 16) + 15; y++)
                {
                    Tile testTile = Main.tile[x + i, y + j];
                    if (testTile.HasTile && testTile.TileType == TileID.MushroomGrass)
                        return false;

                    if (testTile.HasTile && testTile.TileType == TileID.JungleGrass)
                        ret = true;

                }

            return ret;
        }
    }
}