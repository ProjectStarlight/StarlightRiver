using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class VitricOre : DummyTile
    {
        public override int DummyType => ProjectileType<VitricOreDummy>();

        public override string Texture => AssetDirectory.VitricTile + Name;
    
        public override void SetStaticDefaults()
        {
            TileObjectData.newTile.DrawYOffset = 2;
            MinPick = int.MaxValue;
            TileID.Sets.Ore[Type] = true;
            //chest = "Vitric Crystal";//this makes the game think this is a chest, and prevents the tiles below from being broken (as well as meteors avoiding it)

            var bottomAnchor = new Terraria.DataStructures.AnchorData(Terraria.Enums.AnchorType.SolidTile, 2, 0);
            (this).QuickSetFurniture(2, 3, DustType<Dusts.Air>(), SoundID.Shatter, new Color(200, 255, 230), 16, false, false, "Vitric Ore", bottomAnchor);        
        }

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
            int item = Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ItemType<Items.Vitric.VitricOre>(), 12);

            // Sync the drop for multiPlayer
            if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0)
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
        }

        public override void SafeNearbyEffects(int i, int j, bool closer)
        {
            if (Main.rand.Next(50) == 0)
            {
                Vector2 pos = new Vector2(i * 16 + Main.rand.Next(16), j * 16 + Main.rand.Next(16));
                if (Main.rand.NextBool())
                    Dust.NewDustPerfect(pos, ModContent.DustType<CrystalSparkle>(), Vector2.Zero);
                else
                    Dust.NewDustPerfect(pos, ModContent.DustType<CrystalSparkle2>(), Vector2.Zero);
            }
            base.SafeNearbyEffects(i, j, closer);
        }
    }

    internal class VitricOreFloat : DummyTile
    {
        public override int DummyType => ProjectileType<VitricOreFloatDummy>();

        public override string Texture => AssetDirectory.VitricTile + Name;

        public override void SetStaticDefaults()
        {
            (this).QuickSetFurniture(2, 2, DustType<Content.Dusts.Air>(), SoundID.Shatter, false, new Color(200, 255, 230), false, false, "Vitric Ore");
            MinPick = int.MaxValue;
            TileID.Sets.Ore[Type] = true;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            int item = Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ItemType<Items.Vitric.VitricOre>(), 6);

            // Sync the drop for multiPlayer
            if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0)
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
        }
    }

    internal class VitricOreDummy : Dummy
    {
        public override string Texture => AssetDirectory.VitricTile + "VitricOreGlow";

        public VitricOreDummy() : base(TileType<VitricOre>(), 32, 48) { }

        public override void Collision(Player Player)
        {
            if (AbilityHelper.CheckDash(Player, Projectile.Hitbox))
            {
                if (Main.myPlayer == Player.whoAmI)
                {
                    WorldGen.KillTile((int)(Projectile.position.X / 16f), (int)(Projectile.position.Y / 16f));
                    NetMessage.SendTileSquare(Player.whoAmI, (int)(Projectile.position.X / 16f), (int)(Projectile.position.Y / 16f), 2, 3, TileChangeType.None);
                } 
                else
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);

                for (int k = 0; k <= 10; k++)
                {
                    Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.3f);
                    Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Air>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 0.8f);
                }
            }
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D tex = Request<Texture2D>(AssetDirectory.VitricTile + "VitricOreGlow").Value;
            Color color = Helper.IndicatorColorProximity(150, 300, Projectile.Center);

            Main.spriteBatch.Draw(tex, Projectile.position - new Vector2(1, -1) - Main.screenPosition, color);
        }
    }

    internal class VitricOreFloatDummy : Dummy
    {
        public override string Texture => AssetDirectory.VitricTile + "VitricOreFloatGlow";

        public VitricOreFloatDummy() : base(TileType<VitricOreFloat>(), 32, 32) { }

        public override void Collision(Player Player)
        {
            if (AbilityHelper.CheckDash(Player, Projectile.Hitbox))
            {
                if (Main.myPlayer == Player.whoAmI)
                {
                    WorldGen.KillTile((int)(Projectile.position.X / 16f), (int)(Projectile.position.Y / 16f));
                    NetMessage.SendTileSquare(Player.whoAmI, (int)(Projectile.position.X / 16f), (int)(Projectile.position.Y / 16f), 2, 2, TileChangeType.None);
                }
                else
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);

                for (int k = 0; k <= 10; k++)
                {
                    Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.3f);
                    Dust.NewDustPerfect(Projectile.Center, DustType<Content.Dusts.Air>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 0.8f);
                }
            }
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D tex = Request<Texture2D>(AssetDirectory.VitricTile + "VitricOreFloatGlow").Value;
            Color color = Helper.IndicatorColorProximity(150, 300, Projectile.Center);

            Main.spriteBatch.Draw(tex, Projectile.position - Vector2.One - Main.screenPosition, color);
        }
    }

    class VitricOreItem : QuickTileItem
    {
        public VitricOreItem() : base("Vitric Ore Crystal Item", "", "VitricOre", 1, AssetDirectory.VitricTile, false) { }
    }

    class VitricOreFloatItem : QuickTileItem
    {
        public VitricOreFloatItem() : base("Floating Vitric Ore Crystal Item", "", "VitricOreFloat", 1, AssetDirectory.VitricTile, false) { }
    }
}