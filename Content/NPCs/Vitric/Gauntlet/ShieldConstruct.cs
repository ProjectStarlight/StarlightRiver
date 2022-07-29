using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Terraria.Audio;

using System;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
    internal class ShieldConstruct : ModNPC, IGauntletNPC
    {
        public override string Texture => AssetDirectory.GauntletNpc + "ShieldConstruct";

        public int bounceCooldown = 0;
        private int timer = 0;

        private Vector2 shieldOffset;

        private Player target => Main.player[NPC.target];

        public bool guarding => timer > 260;
      
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shield Construct");
        }

        public override void SetDefaults()
        {
            NPC.width = 52;
            NPC.height = 56;
            NPC.damage = 10;
            NPC.defense = 5;
            NPC.lifeMax = 250;
            NPC.value = 10f;
            NPC.knockBackResist = 0.6f;
            NPC.aiStyle = 3;
            NPC.DeathSound = SoundID.Shatter;
            NPC.behindTiles = true;
        }

        public override bool PreAI() //TODO: Document checks with actions and real conditions
        {
            NPC.TargetClosest(false);

            if (bounceCooldown > 0)
                bounceCooldown--;

            if (timer < 300 || timer >= 400)
                timer++;

            timer %= 500;

            if (timer > 200)
            {
                float shieldAnimationProgress;
                Vector2 up = new Vector2(0, -12);
                Vector2 down = new Vector2(0, 14);

                if (timer < 400)
                {
                    if (timer < 250)
                    {
                        shieldAnimationProgress = EaseFunction.EaseCubicInOut.Ease(((timer - 200) / 50f));
                        shieldOffset = up * shieldAnimationProgress;
                    }
                    else if (timer <= 260)
                    {
                        shieldAnimationProgress = EaseFunction.EaseQuarticIn.Ease((timer - 250) / 10f);
                        shieldOffset = Vector2.Lerp(up, down, shieldAnimationProgress);
                    }

                    if (timer == 260) //Shield hits the ground
                    {
                        Helper.PlayPitched("GlassMiniboss/GlassSmash", 1f, 0.3f, NPC.Center);
                        Core.Systems.CameraSystem.Shake += 4;

                        for (int i = 0; i < 10; i++)
                        {
                            Dust.NewDustPerfect(NPC.Center + new Vector2(16 * NPC.spriteDirection, 20), DustID.Copper);
                            Dust.NewDustPerfect(NPC.Center + new Vector2(16 * NPC.spriteDirection, 20), DustType<Dusts.GlassGravity>());
                        }
                    }
                }
                else
                {
                    if (timer < 464)
                    {
                        shieldAnimationProgress = EaseFunction.EaseQuadIn.Ease((timer - 400) / 64f);
                        shieldOffset = Vector2.Lerp(down, new Vector2(0,4), shieldAnimationProgress);
                    }
                    else if (timer < 470)
                    {
                        shieldAnimationProgress = EaseFunction.EaseQuadOut.Ease((timer - 464) / 6f);
                        shieldOffset = Vector2.Lerp(new Vector2(0, 4), up, shieldAnimationProgress);
                    }
                    else
                    {
                        shieldAnimationProgress = EaseFunction.EaseQuinticInOut.Ease((timer - 470) / 30f);
                        shieldOffset = up * (1 - shieldAnimationProgress);
                    }

                    if (timer == 421)
                        Helper.PlayPitched("StoneSlide", 1f, -1f, NPC.Center);

                    if (timer == 464)
                    {
                        Core.Systems.CameraSystem.Shake += 2;

                        for (int i = 0; i < 6; i++)
                        {
                            Dust.NewDustPerfect(NPC.Center + new Vector2(16 * NPC.spriteDirection, 20), DustID.Copper);
                            Dust.NewDustPerfect(NPC.Center + new Vector2(16 * NPC.spriteDirection, 20), DustType<Dusts.GlassGravity>());
                        }
                    }
                }

                if (guarding && (Math.Sign(NPC.Center.DirectionTo(target.Center).X) != NPC.spriteDirection || NPC.Distance(target.Center) > 350) && timer < 400)
                    timer = 400;

                NPC.velocity.X *= 0.9f;
                return false;
            }

            shieldOffset = Vector2.Zero;
            NPC.spriteDirection = Math.Sign(NPC.Center.DirectionTo(target.Center).X);
            return true;
        }

        public override void AI()
        {
            if (timer < 10 && NPC.velocity.Y < 0)
                NPC.velocity.Y = 0;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects effects = SpriteEffects.None;

            Texture2D mainTex = Request<Texture2D>(Texture).Value;
            Texture2D glowTex = Request<Texture2D>(Texture + "_Glow").Value;
            Texture2D shieldTex = Request<Texture2D>(Texture + "_Shield").Value;

            if (NPC.spriteDirection != 1)
                effects = SpriteEffects.FlipHorizontally;

            Main.spriteBatch.Draw(mainTex, NPC.Center - screenPos, null, drawColor, 0f, mainTex.Size() / 2 + new Vector2(0, 8), NPC.scale, effects, 0f);
            Main.spriteBatch.Draw(glowTex, NPC.Center - screenPos, null, Color.White, 0f, mainTex.Size() / 2 + new Vector2(0, 8), NPC.scale, effects, 0f);
            Main.spriteBatch.Draw(shieldTex, NPC.Center - screenPos + shieldOffset, null, drawColor, 0f, mainTex.Size() / 2 + new Vector2(0, 8), NPC.scale, effects, 0f);
            return false;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (guarding)
                return base.CanHitPlayer(target, ref cooldownSlot);

            return false;
        }

        public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {
            if (guarding || Math.Sign(NPC.Center.DirectionTo(player.Center).X) == NPC.spriteDirection)
                knockback = 0f;

            if (Math.Sign(NPC.Center.DirectionTo(player.Center).X) == NPC.spriteDirection)
            {
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.1f }, NPC.Center);
                if (guarding)
                    damage = 1;
                else
                    damage = (int)(damage * 0.4f);
            }
            else
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.3f }, NPC.Center);
        }


        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (guarding || Math.Sign(NPC.Center.DirectionTo(target.Center).X) == NPC.spriteDirection)
                knockback = 0f;

            if (Math.Sign(NPC.Center.DirectionTo(target.Center).X) == NPC.spriteDirection)
            {
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.6f }, NPC.Center);
                if (guarding)
                    damage = 1;
                else
                    damage = (int)(damage * 0.4f);
            }
            else
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.3f }, NPC.Center);
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 12; i++)
                    Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(3, 3), 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = false;

                for (int k = 1; k <= 17; k++)
                    Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("ConstructGore" + k).Type);
            }
        }
    }
}