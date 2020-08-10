using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Tiles.Dragons;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Dragons
{
    internal class DragonEgg : ModNPC
    {
        public NestEntity nest;
        public override string Texture => "StarlightRiver/Invisible";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dragon Egg");
        }

        public override void SetDefaults()
        {
            npc.width = 18;
            npc.height = 18;
            npc.damage = 0;
            npc.defense = 0;
            npc.lifeMax = 1;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.immortal = true;
            npc.value = 0f;
            npc.knockBackResist = 0f;
            npc.aiStyle = -1;
        }

        public override void AI()
        {
            npc.rotation = npc.ai[0]; //rotation for shaking animation
            npc.ai[0] = (float)Math.Sin(npc.ai[1]) * npc.ai[2] / 2;
            npc.ai[1] += 0.1f; if (npc.ai[1] > 6.28f) npc.ai[1] = 0; //rotation
            if (npc.ai[1] == 0 && Main.rand.Next(2) == 0) npc.ai[2] = 1;
            if (npc.ai[2] > 0) npc.ai[2] -= 0.01f;

            npc.ai[3]++;
            if (npc.ai[3] >= 600) npc.Kill();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (nest == null) return false;
            DragonData data = nest.Dragon.data;
            Texture2D tex = GetTexture("StarlightRiver/Items/Dragons/Egg");
            Texture2D tex2 = GetTexture("StarlightRiver/Items/Dragons/EggOver");
            spriteBatch.Draw(tex, npc.Center - Main.screenPosition, tex.Frame(), data.scaleColor.MultiplyRGB(drawColor), npc.rotation, new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);
            spriteBatch.Draw(tex2, npc.Center - Main.screenPosition, tex2.Frame(), data.bellyColor.MultiplyRGB(drawColor), npc.rotation, new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);
            return false;
        }

        public override bool CheckDead()
        {
            for (int k = 0; k < 20; k++) Dust.NewDustPerfect(npc.Center, DustID.Marble, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, nest.Dragon.data.scaleColor);
            Main.PlaySound(SoundID.Camera, npc.Center);
            Main.NewText(nest.owner.name + "'s dragon ''" + nest.Dragon.data.name + "'' hatched!", new Color(255, 240, 100));
            return true;
        }
    }
}