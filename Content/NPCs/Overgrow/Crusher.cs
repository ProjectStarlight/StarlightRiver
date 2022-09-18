using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Overgrow
{
	internal class Crusher : ModNPC
    {
        public Tile Parent;

        public ref float homeY => ref NPC.ai[0];

        public bool falling { get => NPC.ai[1] == 1; set => NPC.ai[1] = value ? 1 : 0; }

        public override string Texture => "StarlightRiver/Assets/NPCs/Overgrow/Crusher";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crusher");
        }

        public override void SetDefaults()
        {
            NPC.width = 160;
            NPC.height = 10;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.lifeMax = 100;
            NPC.dontCountMe = true;
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.knockBackResist = 0;
            NPC.behindTiles = true;
        }

        public override void AI()
        {
            if (falling && NPC.velocity.Y == 0) //when falling, check for if the NPC hits the grond to trigger effects
            {
                for (float k = 0; k <= 0.3f; k += 0.007f)
                {
                    Vector2 vel = new Vector2(1, 0).RotatedBy(-k) * Main.rand.NextFloat(8);

                    if (Main.rand.NextBool(2))
                        vel = new Vector2(-1, 0).RotatedBy(k) * Main.rand.NextFloat(8);

                    Dust.NewDustPerfect(NPC.Center + new Vector2(vel.X * 3, 5), DustID.Stone, vel * 0.7f);
                    Dust.NewDustPerfect(NPC.Center + new Vector2(vel.X * 3, 5), DustType<Dusts.Stamina>(), vel);
                }

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item70 with { PitchVariance = 0.6f }, NPC.Center);

                foreach (Player Player in Main.player.Where(Player => Vector2.Distance(Player.Center, NPC.Center) <= 250))
                    Core.Systems.CameraSystem.Shake = (250 - (int)Vector2.Distance(Player.Center, NPC.Center)) / 12;

                falling = false;
            }

            if (falling)
			{
                NPC.velocity.Y += 0.5f; //slightly faster than normal gravity to give it some extra punch/danger
                NPC.damage = 100;
			}
			else
			{
                if (NPC.Center.Y >= homeY)
                    NPC.velocity.Y = -0.5f;
                else
                    NPC.velocity.Y = 0;

                NPC.damage = 0;
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffType<Buffs.Squash>(), 450);
        }

        public override bool? CanHitNPC(NPC target)
		{
            return true;
		}

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            if (target.type == NPCID.Bunny)
            {
                damage *= 99;
                crit = true;
                for (int k = 0; k < 1000; k++) Dust.NewDustPerfect(target.Center, DustID.Blood, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20), 0, default, 3);
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/NPCs/Overgrow/CrusherGlow").Value;
            Texture2D tex2 = Request<Texture2D>("StarlightRiver/Assets/NPCs/Overgrow/CrusherTile").Value;

            spriteBatch.Draw(tex, NPC.Center - screenPos + new Vector2(0, -24), tex.Bounds, Color.White * 0.8f, 0, tex.Size() / 2, 1.2f + (float)Math.Sin(NPC.ai[0] / 80f * 6.28f) * 0.2f, 0, 0);

            int count = (int)((NPC.Center.Y - homeY) / tex2.Height);

            for (int k = 1; k <= count; k++)
                spriteBatch.Draw(tex2, NPC.position - screenPos + new Vector2(8, -48 - k * tex2.Height), drawColor);
        }
    }
}