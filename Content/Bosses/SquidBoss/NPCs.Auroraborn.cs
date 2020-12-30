using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
    class Auroraborn : ModNPC
    {
        public override string Texture => AssetDirectory.SquidBoss + Name;
        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) => false;

        public override void SetDefaults()
        {
            npc.width = 40;
            npc.height = 40;
            npc.lifeMax = 100;
            npc.damage = 15;
            npc.noGravity = true;
            npc.aiStyle = -1;
            npc.noTileCollide = true;
            npc.knockBackResist = 1.5f;
        }

        public override void AI()
        {
            npc.frame = new Rectangle((int)(npc.ai[0] / 10) % 6 * 58, 0, 58, 50);

            npc.TargetClosest();
            Player player = Main.player[npc.target];

            if (npc.ai[0] % 60 == 0)
            {
                npc.velocity = Vector2.Normalize(npc.Center - player.Center) * -6f;
                for (int k = 0; k < 10; k++) Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), DustType<Dusts.Starlight>(), npc.velocity * Main.rand.NextFloat(-5, 5));
            }

            npc.ai[0]++;

            npc.velocity *= 0.95f;

            npc.rotation = npc.velocity.ToRotation() + 1.57f;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D tex = GetTexture(AssetDirectory.SquidBoss + "AurorabornGlow");
            Texture2D tex2 = GetTexture(AssetDirectory.SquidBoss + "AurorabornGlow2");

            float sin = 1 + (float)Math.Sin(npc.ai[0] / 10f);
            float cos = 1 + (float)Math.Cos(npc.ai[0] / 10f);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

            spriteBatch.Draw(GetTexture(Texture), npc.Center - Main.screenPosition, npc.frame, drawColor * 1.2f, npc.rotation, npc.Size / 2, 1, 0, 0);
            spriteBatch.Draw(tex, npc.Center - Main.screenPosition, npc.frame, color * 0.8f, npc.rotation, npc.Size / 2, 1, 0, 0);
            spriteBatch.Draw(tex2, npc.Center - Main.screenPosition, npc.frame, color, npc.rotation, npc.Size / 2, 1, 0, 0);
            Lighting.AddLight(npc.Center, color.ToVector3() * 0.5f);
        }
    }
}
