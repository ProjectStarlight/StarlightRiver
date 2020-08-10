using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Abilities;
using StarlightRiver.Items;
using StarlightRiver.Projectiles.Dummies;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Interactive
{
    internal class Bouncer : DummyTile
    {
        public override int DummyType => ProjectileType<BouncerDummy>();
        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 1, 1, DustType<Dusts.Glass3>(), SoundID.Shatter, false, new Color(115, 182, 158));
        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<BouncerItem>());
    }

    internal class BouncerItem : QuickTileItem { public BouncerItem() : base("Vitric Bouncer", "Dash into this to go flying!\nResets jump accessories", TileType<Bouncer>(), 8) { } }

    internal class BouncerDummy : Dummy
    {
        public BouncerDummy() : base(TileType<Bouncer>(), 16, 16) { }

        public override void Collision(Player player)
        {
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();

            if (AbilityHelper.CheckDash(player, projectile.Hitbox))
            {
                mp.dash.Active = false;

                if (player.velocity.Length() != 0)
                {
                    player.velocity = Vector2.Normalize(player.velocity) * -18f;
                    player.wingTime = player.wingTimeMax;
                    player.rocketTime = player.rocketTimeMax;
                    player.jumpAgainCloud = true;
                    player.jumpAgainBlizzard = true;
                    player.jumpAgainSandstorm = true;
                    player.jumpAgainFart = true;
                    player.jumpAgainSail = true;
                }

                Main.PlaySound(SoundID.Shatter, projectile.Center);
                for (int k = 0; k <= 30; k++)
                {
                    int dus = Dust.NewDust(projectile.position, 48, 32, mod.DustType("Glass"), Main.rand.Next(-16, 15), Main.rand.Next(-16, 15), 0, default, 1.3f);
                    Main.dust[dus].customData = projectile.Center;
                }
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = GetTexture("StarlightRiver/Tiles/Interactive/BouncerGlow");
            Color color = Color.White * (float)Math.Sin(StarlightWorld.rottime);
            spriteBatch.Draw(tex, projectile.position - Vector2.One - Main.screenPosition, color);
        }
    }
}