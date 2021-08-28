using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Content.NPCs.SpaceEvent
{
    class SwordEnemy : ModNPC
    {
        public ref float Timer => ref npc.ai[0];
        public ref float Phase => ref npc.ai[1];

        public Player Target => Main.player[npc.target];

        public override string Texture => AssetDirectory.SpaceEventNPC + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sky Splicer");
        }

        public override void SetDefaults()
        {
            npc.width = 60;
            npc.height = 60;
            npc.lifeMax = 200;
            npc.damage = 20;
            npc.noGravity = true;
            npc.aiStyle = -1;

            npc.frame.Width = 192;
            npc.frame.Height = 164;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if(Phase == 1)
            {
                if(Timer < 30 || (Timer > 60 && Timer < 90))
                {
                    float angle = npc.direction == 1 ? 0 : 3.14f;
                    return Helper.CheckConicalCollision(npc.Center, 80, angle, 1.5f, target.Hitbox);
                }

                if (Timer > 120)
                    return Helper.CheckCircularCollision(npc.Center, 60, target.Hitbox);
            }

            return false;
        }

        

        public override void AI()
        {
            Timer++;

            switch(Phase)
            {
                case 0: //idle/move to player. Might add targeting radius or LoS of some sort?
                    npc.TargetClosest();

                    npc.rotation = npc.velocity.X * 0.1f;

                    if(Vector2.Distance(npc.Center, Target.Center) < 160)
                    {
                        npc.velocity *= 0.8f;

                        if (npc.velocity.Length() < 2)
                        {
                            Phase = 1;
                            Timer = 0;
                        }
                    }

                    else
					{
                        npc.velocity += Vector2.Normalize(Target.Center - npc.Center);

                        if (npc.velocity.Length() > 4)
                            npc.velocity = Vector2.Normalize(npc.velocity) * 3.9f;
                    }

                    break;

                case 1: //slash slash spin combo TODO: Add framing and VFX!

                    if (Timer == 1 || Timer == 40)
                    {
                        npc.velocity.X += npc.direction * 8;
                        Helper.PlayPitched("Effects/FancySwoosh", 1, 1, npc.Center);
                    }

                    if (Timer < 30)
                    {
                        npc.Frame(0, (int)(Timer / 30f * 6) * npc.frame.Height);
                    }

                    if (Timer > 40 && Timer < 70)
                    {
                        npc.Frame(0, (int)(6 + (Timer - 40) / 30f * 6) * npc.frame.Height);
                    }

                    if (Timer > 80 && Timer < 100)
                        npc.velocity += Vector2.Normalize(Target.Center - npc.Center) * 0.8f;

                    if (Timer == 80)
                    {
                        npc.Frame(npc.frame.Width, 0);
                        Helper.PlayPitched("Effects/SwordUltimate", 0.4f, 0.5f, npc.Center);
                    }

                    if (Timer >= 85)
                    {
                        npc.Frame(npc.frame.Width, npc.frame.Height * Main.rand.Next(1, 3));

                        if (Timer < 120)
                            for (int k = 0; k < 10; k++)
                            {
                                var rot = Main.rand.NextFloat(6.28f);
                                Dust.NewDustPerfect(npc.Center + Vector2.UnitX.RotatedBy(rot) * 60, DustType<Dusts.BlueStamina>(), Vector2.UnitX.RotatedBy(rot + 1 * npc.direction) * Main.rand.NextFloat(7));
                            }
                    }

                    if(Timer >= 135)
                        npc.Frame(npc.frame.Width, 0);

                    if (Timer < 70 || Timer >= 100)
                        npc.velocity *= 0.95f;

                    if(Timer > 140)
                    {
                        Phase = 0;
                        Timer = 0;
                        npc.velocity *= 0;
                    }
                    break;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            npc.frame.Width = 192;
            npc.frame.Height = 164;

            var tex = GetTexture(Texture);
            var texSlash = GetTexture(AssetDirectory.SpaceEventNPC + "SwordEnemySlash");

            var effects = npc.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if(Phase == 0)
                spriteBatch.Draw(tex, npc.Center - Main.screenPosition, null, Color.White, npc.rotation, tex.Size() / 2, npc.scale, effects, 0);

            if (Phase == 1)
                spriteBatch.Draw(texSlash, npc.Center - Main.screenPosition, npc.frame, Color.White, 0, new Vector2(192 / 2, 164 / 2), 1, effects, 0);

            return false;
        }
    }
}
