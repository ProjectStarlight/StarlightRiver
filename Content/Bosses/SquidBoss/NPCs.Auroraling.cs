using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria.Audio;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class Auroraling : ModNPC
    {
        public override string Texture => AssetDirectory.SquidBoss + Name;

        public override void SetDefaults()
        {
            NPC.width = 26;
            NPC.height = 30;
            NPC.lifeMax = 40;
            NPC.damage = 10;
            NPC.noGravity = true;
            NPC.aiStyle = -1;
            NPC.knockBackResist = 3f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Baby aurora squid are born with their light-sacs fully charged from the glow of their mother, and will rely on this energy untill they are old enough to venture to the surface to gather their own.")
            });
        }

        public override void AI()
        {
            NPC.ai[0]++;
            NPC.frame = new Rectangle(26 * ((int)(NPC.ai[0] / 5) % 3), 0, 26, 30);

            NPC.TargetClosest();
            Player Player = Main.player[NPC.target];

            NPC.velocity += Vector2.Normalize(NPC.Center - Player.Center) * -0.15f;
            if (NPC.velocity.LengthSquared() > 4) NPC.velocity *= 0.95f;
            if (NPC.ai[0] % 15 == 0) NPC.velocity.Y -= 0.5f;

            NPC.rotation = NPC.velocity.X * 0.25f;

            foreach (NPC npc in Main.npc.Where(n => n.active && n.type == Type && Vector2.Distance(n.Center, NPC.Center) < 32))
                npc.velocity += (npc.Center - NPC.Center) * 0.05f;
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            target.noKnockback = true;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 8; i++)
                    Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(8, 8), DustType<Dusts.Glow>(), Main.rand.NextVector2Circular(5,5), 0, new Color(150, 200, 255) * 0.5f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
            if (NPC.IsABestiaryIconDummy)
			{
                NPC.ai[0]++;
                NPC.frame = new Rectangle(26 * ((int)(NPC.ai[0] / 5) % 3), 0, 26, 30);
            }

            Texture2D tex = Request<Texture2D>(AssetDirectory.SquidBoss + "AuroralingGlow").Value;
            Texture2D tex2 = Request<Texture2D>(AssetDirectory.SquidBoss + "AuroralingGlow2").Value;

            float sin = 1 + (float)Math.Sin(NPC.ai[0] / 10f);
            float cos = 1 + (float)Math.Cos(NPC.ai[0] / 10f);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

            spriteBatch.Draw(Request<Texture2D>(Texture).Value, NPC.Center - screenPos, NPC.frame, drawColor * 1.2f, NPC.rotation, NPC.Size / 2, 1, 0, 0);
            spriteBatch.Draw(tex, NPC.Center - screenPos, NPC.frame, color * 0.8f, NPC.rotation, NPC.Size / 2, 1, 0, 0);
            spriteBatch.Draw(tex2, NPC.Center - screenPos, NPC.frame, color, NPC.rotation, NPC.Size / 2, 1, 0, 0);

            Lighting.AddLight(NPC.Center, color.ToVector3() * 0.5f);
            return false;
        }
    }
}
