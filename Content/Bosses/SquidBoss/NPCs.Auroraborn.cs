using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class Auroraborn : ModNPC
    {
        public override string Texture => AssetDirectory.SquidBoss + Name;

        public override void SetDefaults()
        {
            NPC.width = 40;
            NPC.height = 40;
            NPC.lifeMax = 100;
            NPC.damage = 15;
            NPC.noGravity = true;
            NPC.aiStyle = -1;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 1.5f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Aurora squid can gather ambient light from the aurora to 'fill' the glands on their head, storing it for later use as energy or for self defense.")
            });
        }

        public override void AI()
        {
            NPC.frame = new Rectangle((int)(NPC.ai[0] / 10) % 6 * 58, 0, 58, 50);

            NPC.TargetClosest();
            Player Player = Main.player[NPC.target];

            if (NPC.ai[0] % 60 == 0)
            {
                NPC.velocity = Vector2.Normalize(NPC.Center - Player.Center) * -6f;
                for (int k = 0; k < 10; k++) Dust.NewDustPerfect(NPC.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), DustType<Dusts.Starlight>(), NPC.velocity * Main.rand.NextFloat(-5, 5));
            }

            NPC.ai[0]++;

            NPC.velocity *= 0.95f;

            NPC.rotation = NPC.velocity.ToRotation() + 1.57f;
        }

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
            if(NPC.IsABestiaryIconDummy)
			{
                NPC.ai[0]++;
                NPC.frame = new Rectangle((int)(NPC.ai[0] / 10) % 6 * 58, 0, 58, 50);
            }

		    Texture2D tex = Request<Texture2D>(AssetDirectory.SquidBoss + "AurorabornGlow").Value;
            Texture2D tex2 = Request<Texture2D>(AssetDirectory.SquidBoss + "AurorabornGlow2").Value;

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
