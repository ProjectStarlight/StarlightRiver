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

        float Angle => (float)((Math.PI * (6 / 8f)) + Math.Sin(Projectile.ai[0] / 200f * (Math.PI * 2) + Math.PI / 4) * 1.5f);
        Vector2 Center => Projectile.Center + new Vector2(1, -1).RotatedBy(Angle) * 90;

        public override void SafeSetDefaults() => Projectile.hide = true;

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }

        public override void Update()
        {
            Projectile.ai[0]++;

            Rectangle hitbox = new Rectangle((int)Center.X - 32, (int)Center.Y - 32, 64, 64);

            for (int k = 0; k < Main.maxPlayers; k++)
            {
                Player Player = Main.player[k];
                if (Player.active && !Player.immune && Player.Hitbox.Intersects(hitbox))
                {
                    Player.Hurt(PlayerDeathReason.ByCustomReason(Player.name + " got axed..."), 80, 0, false, false, true);
                    Player.AddBuff(BuffID.Bleeding, 1800);
                    Player.velocity.X += Projectile.ai[0] % 200 < 100 ? 12 : -12;
                }
            }

            if (Projectile.ai[0] % 100 == 75) Terraria.Audio.SoundEngine.PlaySound(SoundID.Item71.SoundId, (int)Center.X, (int)Center.Y, 71, 1, -0.8f);
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = Request<Texture2D>(AssetDirectory.OvergrowItem + "ExecutionersAxe").Value;
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Lighting.GetColor((int)Center.X / 16, (int)Center.Y / 16), Angle, new Vector2(0, tex.Height), 1.5f, 0, 0);
        }
    }

    class AxeTileItem : QuickTileItem
    {
        public override string Texture => AssetDirectory.Debug;

        public AxeTileItem() : base("Axe Trap", "Debug Item", TileType<AxeTile>(), 1) { }
    }
}
