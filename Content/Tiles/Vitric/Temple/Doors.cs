using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Core;
using StarlightRiver.Core.Systems;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class DoorVertical : ModTile
    {
        public override string Texture => AssetDirectory.VitricTile + Name;

        public override void SetStaticDefaults()
        {
            MinPick = int.MaxValue;
            TileID.Sets.DrawsWalls[Type] = true;
            (this).QuickSetFurniture(1, 7, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(200, 180, 100), false, true);
        }
    }

    class DoorVerticalItem : QuickTileItem
    {
        public DoorVerticalItem() : base("Vertical Temple Door", "Temple Door, But what if it was vertical?", "DoorVertical", ItemRarityID.Blue, AssetDirectory.Debug, true) { }
    }

    class DoorHorizontal : ModTile
    {
        public override string Texture => AssetDirectory.VitricTile + Name;

        public override void SetStaticDefaults()
        {
            MinPick = int.MaxValue;
            TileID.Sets.DrawsWalls[Type] = true;
            (this).QuickSetFurniture(7, 1, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(200, 180, 100), false, true);
        }
    }

    class DoorHorizontalItem : QuickTileItem
    {
        public DoorHorizontalItem() : base("Horizontal Temple Door", "Temple Door, But what if it was horizontal?", "DoorHorizontal", ItemRarityID.Blue, AssetDirectory.Debug, true) { }
    }

    class DashableDoor : DummyTile
    {
        public override int DummyType => ProjectileType<DashableDoorDummy>();

        public override string Texture => AssetDirectory.Invisible;

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;

		public override void PostSpawnDummy(Projectile dummy)
		{
            dummy.position.X -= 8;
		}

		public override void SetStaticDefaults()
        {
            MinPick = int.MaxValue;
            TileID.Sets.DrawsWalls[Type] = true;
            (this).QuickSetFurniture(2, 13, DustType<Dusts.GlassGravity>(), SoundID.Tink, false, new Color(100, 200, 255));
            Main.tileSolid[Type] = true;
        }
    }

    class DashableDoorDummy : Dummy
    {
        public DashableDoorDummy() : base(TileType<DashableDoor>(), 16 * 3, 16 * 13) { }

        public override void Collision(Player Player)
        {
            if (Player.Hitbox.Intersects(Projectile.Hitbox))
            {
                if (AbilityHelper.CheckDash(Player, Projectile.Hitbox))
                {
                    WorldGen.KillTile(ParentX, ParentY);

                    Player.GetModPlayer<AbilityHandler>().ActiveAbility?.Deactivate();
                    Player.velocity = Vector2.Normalize(Player.velocity) * -10f;
                    Player.velocity.Y -= 5;

                    CameraSystem.Shake += 10;

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Player.Center);
                }
            }
        }

        public override void PostDraw(Color lightColor)
        {
            Main.spriteBatch.Draw(Request<Texture2D>(AssetDirectory.VitricTile + "TutorialDoor2").Value, Projectile.position + Vector2.UnitX * 8 - Main.screenPosition, lightColor);
            Main.spriteBatch.Draw(Request<Texture2D>(AssetDirectory.VitricTile + "TutorialDoor2Glow").Value, Projectile.position + Vector2.UnitX * 8 - Main.screenPosition, Helper.IndicatorColor);
        }
    }

    class DashableDoorItem : QuickTileItem
    {
        public DashableDoorItem() : base("DashableDoor", "Debug Item", "DashableDoor", 1, AssetDirectory.Debug, true) { }
    }
}
