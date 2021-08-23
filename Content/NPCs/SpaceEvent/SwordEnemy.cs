using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarlightRiver.Core;
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
            npc.width = 16;
            npc.height = 16;
            npc.lifeMax = 200;
            npc.damage = 20;
            npc.noGravity = true;
            npc.aiStyle = -1;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if(Phase == 1)
            {
                if(Timer < 30 || (Timer > 60 && Timer < 90))
                {
                    float angle = npc.direction == 1 ? 0 : 3.14f;
                    return Helpers.Helper.CheckConicalCollision(npc.Center, 80, angle, 1.5f, target.Hitbox);
                }

                if (Timer > 120)
                    return Helpers.Helper.CheckCircularCollision(npc.Center, 80, target.Hitbox);
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

                    npc.velocity += Vector2.Normalize(npc.Center - Target.Center);

                    if (npc.velocity.Length() > 10)
                        npc.velocity = Vector2.Normalize(npc.velocity) * 9.9f;

                    if(Vector2.Distance(npc.Center, Target.Center) < 40)
                    {
                        npc.velocity *= 0.9f;

                        if (npc.velocity.Length() < 2)
                        {
                            Phase = 1;
                            Timer = 0;
                        }
                    }
                    break;

                case 1: //slash slash spin combo TODO: Add framing and VFX!

                    if (Timer == 1 || Timer == 60)
                        npc.velocity.X += npc.direction * 2;

                    if(Timer > 120 && Timer < 140)
                        npc.velocity += Vector2.Normalize(npc.Center - Target.Center) * 0.2f;                   

                    if (Timer < 120 || Timer >= 140)
                        npc.velocity *= 0.9f;

                    if(Timer > 180)
                    {
                        Phase = 0;
                        Timer = 0;
                    }
                    break;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            var tex = GetTexture(Texture);
            var texSlash = GetTexture(AssetDirectory.SpaceEventNPC + "SwordEnemySlash");

            var effects = npc.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(tex, npc.Center - Main.screenPosition, null, drawColor, npc.rotation, tex.Size() / 2, npc.scale, effects, 0);

            if (Phase == 1)
                spriteBatch.Draw(texSlash, npc.Center - Main.screenPosition, npc.frame, Color.White, 0, new Vector2(20, 20), 1, effects, 0);

            return false;
        }
    }
}
