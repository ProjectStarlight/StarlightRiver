using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Items.Breacher;
using StarlightRiver.Content.Items.SpaceEvent;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Enums;
using System;
using System.Collections.Generic;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.CrashTech
{
    class CrashPod : DummyTile
    {
        public override string Texture => "StarlightRiver/Assets/Tiles/CrashTech/CrashPod";

        public override int DummyType => ProjectileType<CrashPodDummy>();

        public override void SetStaticDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 2, 4, DustID.Lava, SoundID.Shatter, false, new Color(255, 200, 40), false, false, "Crashed Pod", new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 2, 0));
            MinPick = 999;
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            Lighting.AddLight(new Vector2(i, j) * 16, new Vector3(1, 0.5f, 0.2f) * 0.3f);
            //if (Main.rand.Next(4) == 0) Dust.NewDustPerfect(new Vector2(i + Main.rand.NextFloat(), j + Main.rand.NextFloat()) * 16, DustType<Content.Dusts.Stamina>(), new Vector2(0, -Main.rand.NextFloat()));
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (Main.tile[i, j].TileFrameX == 0 && Main.tile[i, j].TileFrameY == 0)
            {
                var dummy = Dummy(i, j);

                if (dummy is null)
                    return;

                Texture2D tex = Request<Texture2D>(Texture + "_Glow").Value;
                Texture2D tex2 = Request<Texture2D>(Texture + "_Glow2").Value;

                spriteBatch.Draw(tex, (Helper.TileAdj + new Vector2(i, j)) * 16 - Main.screenPosition, null, Color.White);
                spriteBatch.Draw(tex2, (Helper.TileAdj + new Vector2(i, j)) * 16 + new Vector2(-1, 0) - Main.screenPosition, null, Helper.IndicatorColorProximity(150, 300, dummy.Center));

            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ModContent.ItemType<Astroscrap>(), Main.rand.Next(10,20));
        }

        public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;

        public override bool CanExplode(int i, int j) => false;
    }
    internal class CrashPodDummy : Dummy
    {
        public override void Load()
        {
            for (int k = 1; k < 6; k++)
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "StarlightRiver/Assets/Tiles/CrashTech/CrashPodGore" + k);
        }

        public CrashPodDummy() : base(TileType<CrashPod>(), 32, 48) { }

        public override void Collision(Player Player)
        {
            if (AbilityHelper.CheckDash(Player, Projectile.Hitbox))
            {
                WorldGen.KillTile(ParentX, ParentY);
                NetMessage.SendTileSquare(Player.whoAmI, (int)(Projectile.position.X / 16f), (int)(Projectile.position.Y / 16f), 2, 3, TileChangeType.None);

                Core.Systems.CameraSystem.Shake += 4;

                for (int k = 1; k <= 5; k++)
                    Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position + new Vector2(Main.rand.Next(Projectile.width), Main.rand.Next(Projectile.height)), (Vector2.Normalize(Player.velocity) * Main.rand.NextFloat(4)).RotatedByRandom(MathHelper.ToRadians(35f)), Mod.Find<ModGore>("CrashPodGore" + k).Type);

                for (int i = 0; i < 17; i++)
                {
                    //for some reason the BuzzSpark dust spawns super offset 
                    Dust.NewDustPerfect(Projectile.Center + new Vector2(0f, 28f) + Main.rand.NextVector2Circular(15,25), ModContent.DustType<Dusts.BuzzSpark>(), (Vector2.Normalize(Player.velocity) * Main.rand.NextFloat(9.5f)).RotatedByRandom(MathHelper.ToRadians(5f)), 0, new Color(255, 255, 60) * 0.8f, 1.15f);

                    Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15, 25), ModContent.DustType<Dusts.Glow>(), (Vector2.Normalize(Player.velocity) * Main.rand.NextFloat(9)).RotatedByRandom(MathHelper.ToRadians(15f)), 0, new Color(150, 80, 40), Main.rand.NextFloat(0.25f, 0.5f));
                }


                Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
            }
        }
    }
}
