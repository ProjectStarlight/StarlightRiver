using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
    class Auroraling : ModNPC
    {
        public override string Texture => AssetDirectory.SquidBoss + Name;
        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) => false;

        public override void SetDefaults()
        {
            npc.width = 26;
            npc.height = 30;
            npc.lifeMax = 40;
            npc.damage = 10;
            npc.noGravity = true;
            npc.aiStyle = -1;
            npc.knockBackResist = 3f;
        }

        public override void AI()
        {
            npc.ai[0]++;
            npc.frame = new Rectangle(26 * ((int)(npc.ai[0] / 5) % 3), 0, 26, 30);

            npc.TargetClosest();
            Player player = Main.player[npc.target];

            npc.velocity += Vector2.Normalize(npc.Center - player.Center) * -0.1f;
            if (npc.velocity.LengthSquared() > 4) npc.velocity = Vector2.Normalize(npc.velocity) * 2;
            if (npc.ai[0] % 15 == 0) npc.velocity.Y -= 0.5f;

            npc.rotation = npc.velocity.X * 0.25f;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D tex = GetTexture(AssetDirectory.SquidBoss + "AuroralingGlow");
            Texture2D tex2 = GetTexture(AssetDirectory.SquidBoss + "AuroralingGlow2");

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
