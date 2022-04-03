using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class Auroraling : ModNPC
    {
        public override string Texture => AssetDirectory.SquidBoss + Name;
        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) => false;

        public override void SetDefaults()
        {
            NPC.width = 26;
            NPC.height = 30;
            NPC.lifeMax = 40;
            NPC.damage = 10;
            NPC.noGravity = true;
            NPC.aiStyle = -1;
            NPC.knockBackResist = 3f;
        }

        public override void AI()
        {
            NPC.ai[0]++;
            NPC.frame = new Rectangle(26 * ((int)(NPC.ai[0] / 5) % 3), 0, 26, 30);

            NPC.TargetClosest();
            Player Player = Main.player[NPC.target];

            NPC.velocity += Vector2.Normalize(NPC.Center - Player.Center) * -0.1f;
            if (NPC.velocity.LengthSquared() > 4) NPC.velocity = Vector2.Normalize(NPC.velocity) * 2;
            if (NPC.ai[0] % 15 == 0) NPC.velocity.Y -= 0.5f;

            NPC.rotation = NPC.velocity.X * 0.25f;
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            target.noKnockback = true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D tex = Request<Texture2D>(AssetDirectory.SquidBoss + "AuroralingGlow").Value;
            Texture2D tex2 = Request<Texture2D>(AssetDirectory.SquidBoss + "AuroralingGlow2").Value;

            float sin = 1 + (float)Math.Sin(NPC.ai[0] / 10f);
            float cos = 1 + (float)Math.Cos(NPC.ai[0] / 10f);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

            spriteBatch.Draw(Request<Texture2D>(Texture).Value, NPC.Center - Main.screenPosition, NPC.frame, drawColor * 1.2f, NPC.rotation, NPC.Size / 2, 1, 0, 0);
            spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, NPC.frame, color * 0.8f, NPC.rotation, NPC.Size / 2, 1, 0, 0);
            spriteBatch.Draw(tex2, NPC.Center - Main.screenPosition, NPC.frame, color, NPC.rotation, NPC.Size / 2, 1, 0, 0);

            Lighting.AddLight(NPC.Center, color.ToVector3() * 0.5f);
        }
    }
}
