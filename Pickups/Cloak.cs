using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Abilities;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Pickups
{
    internal class Cloak : ModNPC
    {
        public override bool Autoload(ref string name) => false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Zzelera's Cloak");
        }

        public override void SetDefaults()
        {
            npc.width = 32;
            npc.height = 32;
            npc.aiStyle = -1;
            npc.immortal = true;
            npc.lifeMax = 1;
            npc.knockBackResist = 0;
            npc.noGravity = true;
        }

        public override bool CheckActive()
        {
            return false;
        }

        private int animate = 0;

        public override void AI()
        {
            npc.TargetClosest(true);
            Player player = Main.player[npc.target];
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();

            if (npc.Hitbox.Intersects(player.Hitbox) && mp.sdash.Locked)
            {
                mp.sdash.Locked = false;
                mp.StatStaminaMaxPerm += 1;
                animate = 300;
                Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Pickups/get"));
            }

            if (animate >= 1)
            {
                player.position = new Vector2(npc.position.X, npc.position.Y - 16);
                player.immune = true;
                player.immuneTime = 5;
                player.immuneNoBlink = true;
                if (animate > 100 && animate < 290)
                {
                    float rot = Main.rand.NextFloat(0, (float)Math.PI * 2);
                    Dust.NewDustPerfect(player.Center + new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot)) * -1000, mod.DustType("Void3"), new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot)) * 14.5f, 0, default, 3f);
                }
                if (animate <= 30 && animate % 10 == 0)
                {
                    for (float k = 0; k <= (float)Math.PI * 2; k += (float)Math.PI / 40)
                    {
                        Dust.NewDustPerfect(player.Center, mod.DustType("Void"), new Vector2((float)Math.Cos(k), (float)Math.Sin(k)) * -5, 0, default, 1.5f);
                    }
                }
                if (animate == 1)
                {
                    player.AddBuff(BuffID.Featherfall, 120);
                    StarlightRiver.Instance.textcard.Display("Zzelera's Cloak", "Press " + StarlightRiver.Superdash.GetAssignedKeys()[0] + " to become invincible and fly to your mouse", mp.sdash);
                }
            }

            if (animate > 0)
            {
                animate--;
            }
        }

        public static Texture2D wind = GetTexture("StarlightRiver/Pickups/Cloak1");
        private float timer = 0;

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            AbilityHandler mp = Main.LocalPlayer.GetModPlayer<AbilityHandler>();

            timer += (float)(Math.PI * 2) / 120;
            if (timer >= Math.PI * 2)
            {
                timer = 0;
            }

            if (mp.sdash.Locked)
            {
                spriteBatch.Draw(wind, npc.position - Main.screenPosition + new Vector2(0, (float)Math.Sin(timer) * 4), Color.White);
                Dust.NewDust(npc.position + new Vector2(0, (float)Math.Sin(timer) * 4), npc.width, npc.height, DustType<Dusts.Darkness>(), 0, 0, 0, default, 0.5f);

                for (float k = 0; k < 6.28f; k += 6.28f / 5)
                {
                    Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedBy(k + (float)Math.Sin((timer + k) * 2) * 0.25f) * 10, DustType<Dusts.Void>(), Vector2.One.RotatedBy(k + (float)Math.Sin(timer) * .5f) * 0.5f, 0, default, 0.5f);
                }
            }
        }
    }
}