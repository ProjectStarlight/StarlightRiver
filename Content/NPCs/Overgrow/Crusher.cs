using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.NPCs.Overgrow
{
    internal class Crusher : ModNPC
    {
        public Tile Parent;

        public override string Texture => "StarlightRiver/Assets/NPCs/Overgrow/Crusher";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Masher");
        }

        public override void SetDefaults()
        {
            npc.width = 160;
            npc.height = 10;
            npc.immortal = true;
            npc.dontTakeDamage = true;
            npc.lifeMax = 1;
            npc.dontCountMe = true;
            npc.aiStyle = -1;
            npc.noGravity = true;
            npc.knockBackResist = 0;
            npc.behindTiles = true;
        }

        public override void AI()
        {
            if (npc.ai[0] < 10)
            {
                npc.velocity.Y += 1.5f; npc.damage = 120;
            }
            if (npc.ai[0] > 40 && npc.ai[0] < 50) { npc.velocity.Y = -3; npc.damage = 0; }
            if (npc.ai[0]++ > 80) { npc.ai[0] = 0; npc.velocity.Y = 0.01f; npc.ai[1] = 0; }

            if (npc.velocity.Y == 0 && npc.ai[1] != 1)
            {
                for (float k = 0; k <= 0.3f; k += 0.007f)
                {
                    Vector2 vel = new Vector2(1, 0).RotatedBy(-k) * Main.rand.NextFloat(8);
                    if (Main.rand.Next(2) == 0) vel = new Vector2(-1, 0).RotatedBy(k) * Main.rand.NextFloat(8); Dust.NewDustPerfect(npc.Center + new Vector2(vel.X * 3, 5), DustID.Stone, vel * 0.7f);
                    Dust.NewDustPerfect(npc.Center + new Vector2(vel.X * 3, 5), DustType<Dusts.Stamina>(), vel);
                }
                Main.PlaySound(SoundID.Item70.WithPitchVariance(0.6f), npc.Center);

                foreach (Player player in Main.player.Where(player => Vector2.Distance(player.Center, npc.Center) <= 250))
                    player.GetModPlayer<StarlightPlayer>().Shake = (250 - (int)Vector2.Distance(player.Center, npc.Center)) / 12;
                npc.ai[1] = 1;
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffType<Buffs.Squash>(), 450);
        }

        public override bool? CanHitNPC(NPC target) => true;

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            if (target.type == NPCID.Bunny)
            {
                damage *= 99;
                crit = true;
                for (int k = 0; k < 1000; k++) Dust.NewDustPerfect(target.Center, DustID.Blood, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20), 0, default, 3);
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D tex = GetTexture("StarlightRiver/Assets/NPCs/Overgrow/CrusherGlow");
            Texture2D tex2 = GetTexture("StarlightRiver/Assets/NPCs/Overgrow/CrusherTile");

            spriteBatch.Draw(tex, npc.Center - Main.screenPosition + new Vector2(0, -24), tex.Bounds, Color.White * 0.8f, 0, tex.Size() / 2, 1.2f + (float)Math.Sin(npc.ai[0] / 80f * 6.28f) * 0.2f, 0, 0);

            int count = npc.ai[0] < 10 ? (int)npc.ai[0] / 3 : npc.ai[0] > 40 ? (60 - (int)npc.ai[0]) / 4 : 3;
            for (int k = 1; k <= count; k++)
                spriteBatch.Draw(tex2, npc.position - Main.screenPosition + new Vector2(8, -48 - k * 28), drawColor);
        }
    }
}