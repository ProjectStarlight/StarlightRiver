using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Faeflame;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using StarlightRiver.Helpers;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Vitric
{
    internal class DesertWisp : ModNPC
    {
        public override string Texture => "StarlightRiver/Assets/Invisible";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Desert Wisp");
        }

        public override void SetDefaults()
        {
            npc.width = 8;
            npc.height = 8;
            npc.damage = 0;
            npc.defense = 0;
            npc.lifeMax = 1;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.dontTakeDamage = true;
            npc.value = 0f;
            npc.knockBackResist = 0f;
            npc.aiStyle = 65;
        }

        public override void AI()
        {
            npc.TargetClosest(true);
            Player player = Main.player[npc.target];
            AbilityHandler mp = player.GetHandler();
            Vector2 distance = player.Center - npc.Center;

            Dust.NewDustPerfect(npc.Center, DustType<Dusts.Air>(), Vector2.Zero);

            if (distance.Length() <= 180 && !mp.Unlocked<Wisp>() || Main.dayTime) npc.ai[3] = 1;

            if (npc.ai[3] == 1)
            {
                npc.velocity.Y = 10;
                npc.velocity.X = 0;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.player.ZoneOverworldHeight && !Main.dayTime && spawnInfo.player.ZoneDesert ? 0.2f : 0f;
    }

    internal class DesertWisp2 : DesertWisp
    {
        public override string Texture => "StarlightRiver/Assets/Invisible";
        public Trail trail;// = new Trail(Main.graphics.GraphicsDevice, 80, new NoTip(), TrailWidth, TrailColor);

		public override void SetStaticDefaults()
		{
            NPCID.Sets.TrailCacheLength[npc.type] = 80;
            NPCID.Sets.TrailingMode[npc.type] = 1;
        }

		public override void SetDefaults()
		{
            if (!Main.dedServ)
                trail = new Trail(Main.graphics.GraphicsDevice, 80, new NoTip(), TrailWidth, TrailColor);
        }

		private static Color TrailColor(Vector2 textureCoordinates)
		{
            return Color.Lerp(new Color(100, 200, 255), new Color(80, 120, 255), (float)Math.Sin(textureCoordinates.X * 6.28f + Main.GameUpdateCount / 100f)) * (float)Math.Pow(Math.Sin((1 - textureCoordinates.X) * 3.14f), 4);

            //return Color.Lerp(new Color(255, 70, 40), new Color(255, 160, 60), (float)Math.Sin(textureCoordinates.X * 6.28f + Main.GameUpdateCount / 100f)) * (float)Math.Pow(Math.Sin((1 - textureCoordinates.X) * 3.14f), 4);
        }

		private static float TrailWidth(float factorAlongTrail)
		{
            return (float)Math.Sin(factorAlongTrail * 3.14f) * 16;
		}

		public override void AI()
        {
            if (npc.velocity.Length() < 1.7f)
                npc.velocity = Vector2.Normalize(npc.velocity) * 1.71f;
        }

		public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
		{
            if (npc.oldPos[79] == Vector2.Zero)
                return;

            trail.Positions = npc.oldPos;

            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(GetTexture("StarlightRiver/Assets/EnergyTrail"));
            effect.Parameters["time"].SetValue(-Main.GameUpdateCount / 40f);
            effect.Parameters["repeats"].SetValue(1);

            effect.CurrentTechnique.Passes[0].Apply();

            trail.Render(effect);
        }

		public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.player.ZoneRockLayerHeight && spawnInfo.player.GetModPlayer<BiomeHandler>().ZoneGlass ? 1f : 0f;
    }
}