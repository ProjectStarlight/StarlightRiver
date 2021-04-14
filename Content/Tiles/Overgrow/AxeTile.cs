using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
    class AxeTile : DummyTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.OvergrowTile + "BrickOvergrow";
            return true;
        }

        public override int DummyType => ProjectileType<AxeTileDummy>();

        public override void SetDefaults() => QuickBlock.QuickSet(this, 210, DustType<Dusts.Stone>(), SoundID.Tink, new Color(79, 76, 71), -1);
    }

    class AxeTileDummy : Dummy
    {
        public AxeTileDummy() : base(TileType<AxeTile>(), 16, 16) { }

        float Angle => (float)((Math.PI * (6 / 8f)) + Math.Sin(projectile.ai[0] / 200f * (Math.PI * 2) + Math.PI / 4) * 1.5f);
        Vector2 Center => projectile.Center + new Vector2(1, -1).RotatedBy(Angle) * 90;

        public override void SafeSetDefaults() => projectile.hide = true;

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }

        public override void Update()
        {
            projectile.ai[0]++;

            Rectangle hitbox = new Rectangle((int)Center.X - 32, (int)Center.Y - 32, 64, 64);

            for (int k = 0; k < Main.maxPlayers; k++)
            {
                Player player = Main.player[k];
                if (player.active && !player.immune && player.Hitbox.Intersects(hitbox))
                {
                    player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " got axed..."), 80, 0, false, false, true);
                    player.AddBuff(BuffID.Bleeding, 1800);
                    player.velocity.X += projectile.ai[0] % 200 < 100 ? 12 : -12;
                }
            }

            if (projectile.ai[0] % 100 == 75) Main.PlaySound(SoundID.Item71.SoundId, (int)Center.X, (int)Center.Y, 71, 1, -0.8f);
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = GetTexture(AssetDirectory.OvergrowItem + "ExecutionersAxe");
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Lighting.GetColor((int)Center.X / 16, (int)Center.Y / 16), Angle, new Vector2(0, tex.Height), 1.5f, 0, 0);
        }
    }

    class AxeTileItem : QuickTileItem
    {
        public override string Texture => AssetDirectory.Debug;

        public AxeTileItem() : base("Axe Trap", "Titties", TileType<AxeTile>(), 1) { }
    }
}
