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
        public ref float Timer => ref NPC.ai[0];
        public ref float Phase => ref NPC.ai[1];

        public Player Target => Main.player[NPC.target];

        public override string Texture => AssetDirectory.SpaceEventNPC + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sky Splicer");
        }

        public override void SetDefaults()
        {
            NPC.width = 60;
            NPC.height = 60;
            NPC.lifeMax = 200;
            NPC.damage = 20;
            NPC.noGravity = true;
            NPC.aiStyle = -1;

            NPC.frame.Width = 192;
            NPC.frame.Height = 164;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if(Phase == 1)
            {
                if(Timer < 30 || (Timer > 60 && Timer < 90))
                {
                    float angle = NPC.direction == 1 ? 0 : 3.14f;
                    return Helper.CheckConicalCollision(NPC.Center, 80, angle, 1.5f, target.Hitbox);
                }

                if (Timer > 120)
                    return Helper.CheckCircularCollision(NPC.Center, 60, target.Hitbox);
            }

            return false;
        }

        

        public override void AI()
        {
            Timer++;

            switch(Phase)
            {
                case 0: //idle/move to Player. Might add targeting radius or LoS of some sort?
                    NPC.TargetClosest();

                    NPC.rotation = NPC.velocity.X * 0.1f;

                    if(Vector2.Distance(NPC.Center, Target.Center) < 160)
                    {
                        NPC.velocity *= 0.8f;

                        if (NPC.velocity.Length() < 2)
                        {
                            Phase = 1;
                            Timer = 0;
                        }
                    }

                    else
					{
                        NPC.velocity += Vector2.Normalize(Target.Center - NPC.Center);

                        if (NPC.velocity.Length() > 4)
                            NPC.velocity = Vector2.Normalize(NPC.velocity) * 3.9f;
                    }

                    break;

                case 1: //slash slash spin combo TODO: Add framing and VFX!

                    if (Timer == 1 || Timer == 40)
                    {
                        NPC.velocity.X += NPC.direction * 8;
                        Helper.PlayPitched("Effects/FancySwoosh", 1, 1, NPC.Center);
                    }

                    if (Timer < 30)
                    {
                        NPC.Frame(0, (int)(Timer / 30f * 6) * NPC.frame.Height);
                    }

                    if (Timer > 40 && Timer < 70)
                    {
                        NPC.Frame(0, (int)(6 + (Timer - 40) / 30f * 6) * NPC.frame.Height);
                    }

                    if (Timer > 80 && Timer < 100)
                        NPC.velocity += Vector2.Normalize(Target.Center - NPC.Center) * 0.8f;

                    if (Timer == 80)
                    {
                        NPC.Frame(NPC.frame.Width, 0);
                        Helper.PlayPitched("Effects/SwordUltimate", 0.4f, 0.5f, NPC.Center);
                    }

                    if (Timer >= 85)
                    {
                        NPC.Frame(NPC.frame.Width, NPC.frame.Height * Main.rand.Next(1, 3));

                        if (Timer < 120)
                            for (int k = 0; k < 10; k++)
                            {
                                var rot = Main.rand.NextFloat(6.28f);
                                Dust.NewDustPerfect(NPC.Center + Vector2.UnitX.RotatedBy(rot) * 60, DustType<Dusts.BlueStamina>(), Vector2.UnitX.RotatedBy(rot + 1 * NPC.direction) * Main.rand.NextFloat(7));
                            }
                    }

                    if(Timer >= 135)
                        NPC.Frame(NPC.frame.Width, 0);

                    if (Timer < 70 || Timer >= 100)
                        NPC.velocity *= 0.95f;

                    if(Timer > 140)
                    {
                        Phase = 0;
                        Timer = 0;
                        NPC.velocity *= 0;
                    }
                    break;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            NPC.frame.Width = 192;
            NPC.frame.Height = 164;

            var tex = Request<Texture2D>(Texture).Value;
            var texSlash = Request<Texture2D>(AssetDirectory.SpaceEventNPC + "SwordEnemySlash").Value;

            var effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if(Phase == 0)
                spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, null, Color.White, NPC.rotation, tex.Size() / 2, NPC.scale, effects, 0);

            if (Phase == 1)
                spriteBatch.Draw(texSlash, NPC.Center - Main.screenPosition, NPC.frame, Color.White, 0, new Vector2(192 / 2, 164 / 2), 1, effects, 0);

            return false;
        }
    }
}
