using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Interactive
{
	internal class Bouncer : DummyTile
    {
        public override int DummyType => ProjectileType<BouncerDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.InteractiveTile + name;
            return true;
        }

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 1, 1, DustType<Content.Dusts.GlassNoGravity>(), SoundID.Shatter, false, new Color(115, 182, 158));

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<BouncerItem>());
    }

    internal class BouncerItem : QuickTileItem 
    {
        public BouncerItem() : base("Vitric Bouncer", "Dash into this to go flying!\nResets jump accessories", TileType<Bouncer>(), 8, AssetDirectory.InteractiveTile) { }
    }

    internal class BouncerDummy : Dummy
    {
        public BouncerDummy() : base(TileType<Bouncer>(), 16, 16) { }

        public override void Collision(Player Player)
        {
            AbilityHandler mp = Player.GetHandler();

            if (AbilityHelper.CheckDash(Player, Projectile.Hitbox))
            {
                mp.ActiveAbility?.Deactivate();

                if (Player.velocity.Length() != 0)
                {
                    Player.velocity = Vector2.Normalize(Player.velocity) * -18f;
                    Player.wingTime = Player.wingTimeMax;
                    Player.rocketTime = Player.rocketTimeMax;
                    Player.jumpAgainCloud = true;
                    Player.jumpAgainBlizzard = true;
                    Player.jumpAgainSandstorm = true;
                    Player.jumpAgainFart = true;
                    Player.jumpAgainSail = true;
                }

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
                for (int k = 0; k <= 30; k++)
                {
                    int dus = Dust.NewDust(Projectile.position, 48, 32, Terraria.ModLoader.ModContent.DustType<Dusts.GlassAttracted>(), Main.rand.Next(-16, 15), Main.rand.Next(-16, 15), 0, default, 1.3f);
                    Main.dust[dus].customData = Projectile.Center;
                }
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Tiles/Interactive/BouncerGlow").Value;
            Color color = Helper.IndicatorColorProximity(150, 300, Projectile.Center);
            spriteBatch.Draw(tex, Projectile.position - Vector2.One - Main.screenPosition, color);
        }
    }
}