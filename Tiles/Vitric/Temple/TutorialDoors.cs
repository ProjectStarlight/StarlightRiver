using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Terraria.ModLoader.ModContent;
using Terraria;
using Terraria.ID;
using StarlightRiver.Projectiles.Dummies;
using StarlightRiver.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Abilities;

namespace StarlightRiver.Tiles.Vitric.Temple
{
    class TutorialDoor1 : DummyTile
    {
        public override int DummyType => ProjectileType<TutorialDoor1Dummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Invisible";
            return true;
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 1, 7, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(100, 200, 255));
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
            if (!player.GetModPlayer<StarlightPlayer>().inTutorial) return;
            spriteBatch.Draw(GetTexture("StarlightRiver/Tiles/Vitric/Temple/TutorialDoor1"), projectile.position - Main.screenPosition, lightColor);
        }
    }

    class TutorialDoor1Item : QuickTileItem
    {
        public override string Texture => "StarlightRiver/MarioCumming";

        public TutorialDoor1Item() : base("TutorialDoor1", "Titties", TileType<TutorialDoor1>(), 1) { }
    }

    class TutorialDoor2 : DummyTile
    {
        public override int DummyType => ProjectileType<TutorialDoor2Dummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Invisible";
            return true;
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 2, 7, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(100, 200, 255));
        }
    }

    class TutorialDoor2Dummy : Dummy
    {
        public TutorialDoor2Dummy() : base(TileType<TutorialDoor2>(), 16 * 2, 16 * 7) { }

        public override void Collision(Player player)
        {
            if (player.GetModPlayer<StarlightPlayer>().inTutorial && player.Hitbox.Intersects(projectile.Hitbox))
            {
                if (AbilityHelper.CheckDash(player, projectile.Hitbox))
                {
                    player.GetModPlayer<StarlightPlayer>().inTutorial = false;
                    Main.PlaySound(SoundID.Shatter, player.Center);

                    for(int k = 0; k < 50; k++)
                        Dust.NewDustPerfect(player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20), DustType<Dusts.Glass2>());
                }
                else player.velocity.X = -1;
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Player player = Main.LocalPlayer;
            if (!player.GetModPlayer<StarlightPlayer>().inTutorial) return;
            spriteBatch.Draw(GetTexture("StarlightRiver/Tiles/Vitric/Temple/TutorialDoor2"), projectile.position - Main.screenPosition, lightColor);
            spriteBatch.Draw(GetTexture("StarlightRiver/Tiles/Vitric/Temple/TutorialDoor2Glow"), projectile.position - Main.screenPosition, Color.White * (float)Math.Sin(StarlightWorld.rottime));
        }
    }

    class TutorialDoor2Item : QuickTileItem
    {
        public override string Texture => "StarlightRiver/MarioCumming";

        public TutorialDoor2Item() : base("TutorialDoor2", "Titties", TileType<TutorialDoor2>(), 1) { }
    }
}
