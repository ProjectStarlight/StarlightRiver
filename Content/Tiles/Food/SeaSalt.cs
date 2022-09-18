using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Food
{
	class SeaSalt : ModTile
    {
        public override string Texture => AssetDirectory.FoodTile + Name;

        public override void SetStaticDefaults()
        {
            Main.tileMerge[Type][ModContent.TileType<Salt>()] = true;
            Main.tileMerge[ModContent.TileType<Salt>()][Type] = true;
            this.QuickSet(0, DustID.Web, SoundID.Dig, new Color(1f, 0.8f, 0.8f), ItemType<Items.Food.SeaSalt>());
            Main.tileSolid[Type] = false;
            //Main.tileBlockLight[Type] = true;
            TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true; // Allows Sandshark enemies to "swim" in this sand.
            TileID.Sets.Falling[Type] = true;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) => Salt.SandFall(i, j, ModContent.ProjectileType<SeaSaltProjectile>());

        public override bool IsTileSpelunkable(int i, int j) => true;
    }

    public class SeaSaltProjectile : SaltProjectile
    {
        protected override int GetTileType => ModContent.TileType<SeaSalt>();
        protected override string GetName => "Sea Salt Ball";
    }
}
