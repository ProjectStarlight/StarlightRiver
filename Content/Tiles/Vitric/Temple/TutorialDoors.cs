using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class TutorialDoor1 : DummyTile
    {
        public override int DummyType => ProjectileType<TutorialDoor1Dummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Invisible;
            return true;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;

        public override void SetDefaults()
        {
            //minPick = int.MaxValue;
            TileID.Sets.DrawsWalls[Type] = true;
            (this).QuickSetFurniture(1, 7, DustType<Content.Dusts.Air>(), SoundID.Tink, false, new Color(100, 200, 255));
        }
    }

    class TutorialDoor1Dummy : Dummy
    {
        public TutorialDoor1Dummy() : base(TileType<TutorialDoor1>(), 16, 16 * 7) { }

        public override void Collision(Player player)
        {
            if (player.GetModPlayer<StarlightPlayer>().inTutorial && player.Hitbox.Intersects(projectile.Hitbox))
                player.velocity.X = 1;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Player player = Main.LocalPlayer;
            if (!player.GetModPlayer<StarlightPlayer>().inTutorial && player.GetHandler().Unlocked<Abilities.ForbiddenWinds.Dash>()) 
                return;

            spriteBatch.Draw(GetTexture(AssetDirectory.VitricTile + "TutorialDoor1"), projectile.position - Main.screenPosition, lightColor);
        }
    }

    class TutorialDoor2 : DummyTile
    {
        public override int DummyType => ProjectileType<TutorialDoor2Dummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Invisible;
            return true;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;

        public override void SetDefaults()
        {
            //minPick = int.MaxValue;
            TileID.Sets.DrawsWalls[Type] = true;
            (this).QuickSetFurniture(2, 7, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(100, 200, 255));
        }
    }

    class TutorialDoor2Dummy : Dummy
    {
        public TutorialDoor2Dummy() : base(TileType<TutorialDoor2>(), 16 * 2, 16 * 7) { }

        public override void Collision(Player player)
        {
            if ((player.GetModPlayer<StarlightPlayer>().inTutorial || !player.GetHandler().Unlocked<Abilities.ForbiddenWinds.Dash>()) && player.Hitbox.Intersects(projectile.Hitbox))
                if (AbilityHelper.CheckDash(player, projectile.Hitbox))
                {
                    player.GetModPlayer<StarlightPlayer>().inTutorial = false;
                    Main.PlaySound(SoundID.Shatter, player.Center);

                    for (int k = 0; k < 50; k++)
                        Dust.NewDustPerfect(player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20), DustType<Dusts.GlassGravity>());
                }
                else player.velocity.X = -1;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Player player = Main.LocalPlayer;

            if (!player.GetModPlayer<StarlightPlayer>().inTutorial && player.GetHandler().Unlocked<Abilities.ForbiddenWinds.Dash>())
                return;

            spriteBatch.Draw(GetTexture(AssetDirectory.VitricTile + "TutorialDoor2"), projectile.position - Main.screenPosition, lightColor);
            spriteBatch.Draw(GetTexture(AssetDirectory.VitricTile + "TutorialDoor2Glow"), projectile.position - Main.screenPosition, Helper.IndicatorColor);
        }
    }
}
