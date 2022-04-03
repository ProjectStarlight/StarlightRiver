using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Codex.Entries;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Purify;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Pickups
{
	internal class Purity : ModNPC
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Corona of Purity");
        }

        public override void SetDefaults()
        {
            NPC.width = 32;
            NPC.height = 32;
            NPC.aiStyle = -1;
            NPC.immortal = true;
            NPC.lifeMax = 1;
            NPC.knockBackResist = 0;
            NPC.noGravity = true;
        }

        public override bool CheckActive()
        {
            return false;
        }

        private int animate = 0;

        public override void AI()
        {
            NPC.TargetClosest(true);
            Player Player = Main.player[NPC.target];
            AbilityHandler mp = Player.GetHandler();

            if (NPC.Hitbox.Intersects(Player.Hitbox) && !mp.Unlocked<Pure>() && animate == 0)
            {
                animate = 500;
            }

            if (animate == 100)
            {
                mp.Unlock<Pure>();
                for (float k = 3.48f; k >= -0.4f; k -= 0.1f)
                {
                    Dust.NewDustPerfect(Player.Center + new Vector2((float)Math.Cos(k) * 32, (float)Math.Sin(k) * 16 - 55), Mod.DustType("Purify2"), new Vector2(0, -2), 0, default, 3f);
                }
                for (int k = 0; k <= 10; k++)
                {
                    Dust.NewDustPerfect(Player.Center + new Vector2(-5 + k / 2, -k * 3 - 39), Mod.DustType("Purify2"), new Vector2(0, -2), 0, default, 3f);
                    Dust.NewDustPerfect(Player.Center + new Vector2(5 - k / 2, -k * 3 - 39), Mod.DustType("Purify2"), new Vector2(0, -2), 0, default, 3f);
                    Dust.NewDustPerfect(Player.Center + new Vector2(-25 + k / 2, -k * 1.2f - 47), Mod.DustType("Purify2"), new Vector2(0, -2), 0, default, 3f);
                    Dust.NewDustPerfect(Player.Center + new Vector2(25 - k / 2, -k * 1.2f - 47), Mod.DustType("Purify2"), new Vector2(0, -2), 0, default, 3f);
                }
                for (int k = 0; k <= 100; k++)
                {
                    float r = Main.rand.NextFloat(0, 6.28f);
                    float r2 = Main.rand.NextFloat(3, 9);
                    Dust.NewDustPerfect(Player.Center, Mod.DustType("Purify2"), new Vector2((float)Math.Cos(r) * r2, (float)Math.Sin(r) * r2), 0, default, 4f);
                }
            }

            if (animate >= 1)
            {
                Player.position = new Vector2(NPC.position.X, NPC.position.Y - 16);
                Player.immune = true;
                Player.immuneTime = 5;
                Player.immuneNoBlink = true;
                if (animate == 1)
                {
                    Player.AddBuff(BuffID.Featherfall, 120);
                    mp.GetAbility<Pure>(out var purify);
                    UILoader.GetUIState<TextCard>().Display("Coronoa of Purity", "Press " + StarlightRiver.Instance.AbilityKeys.Get<Pure>().GetAssignedKeys()[0] + " to purify nearby tiles", purify);
                    Helper.UnlockEntry<PureEntry>(Player);
                }
            }

            if (animate > 0)
            {
                animate--;
            }
        }

        public static Texture2D wind = Request<Texture2D>("StarlightRiver/Assets/Abilities/PureCrown").Value;
        private float timer = 0;

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (Main.LocalPlayer == Main.player[NPC.target])
            {
                //darkness
                if (animate >= 400)
                {
                    //spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/NPCs/Pickups/Overlay").Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(0, 0, 0, (100 - ((float)animate - 400)) / 100));
                    Lighting.brightness = (float)(animate - 400) / 100;
                }

                if (animate >= 30 && animate < 400)
                {
                    //spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/NPCs/Pickups/Overlay").Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(0, 0, 0, 0.99f));
                    Lighting.brightness = 0f;
                }

                if (animate < 30 && animate > 0)
                {
                    //spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/NPCs/Pickups/Overlay").Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(0, 0, 0, (float)animate / 30));
                    Lighting.brightness = (float)(30 - animate) / 30;
                }

                //crown
                if (animate <= 400 && animate > 120)
                {
                    spriteBatch.Draw(wind, Vector2.Lerp(new Vector2(NPC.position.X - Main.screenPosition.X, 0), (NPC.position - Main.screenPosition) + new Vector2(0, -32), 1 - ((float)animate - 120) / 280), Color.White * (((float)animate - 120) / 30));
                }
            }

            AbilityHandler mp = Main.LocalPlayer.GetHandler();

            timer += (float)(Math.PI * 2) / 120;
            if (timer >= Math.PI * 2)
            {
                timer = 0;
            }

            if (!mp.Unlocked<Pure>() && animate == 0)
            {
                spriteBatch.Draw(wind, NPC.position - Main.screenPosition + new Vector2(0, (float)Math.Sin(timer) * 4), Color.White);
                Dust.NewDust(NPC.position + new Vector2(0, (float)Math.Sin(timer) * 16), NPC.width, NPC.height, DustType<Content.Dusts.Purify>());

                Dust.NewDustPerfect(NPC.Center + new Vector2((float)Math.Cos(timer) * 40, (float)Math.Sin(timer) * 20), DustType<Content.Dusts.Purify>(), null, 0, default, 2f);
                Dust.NewDustPerfect(NPC.Center + new Vector2((float)Math.Cos(timer) * 40, (float)Math.Sin(timer) * 20) * -1, DustType<Content.Dusts.Purify>(), null, 0, default, 2f);
            }
        }
    }
}