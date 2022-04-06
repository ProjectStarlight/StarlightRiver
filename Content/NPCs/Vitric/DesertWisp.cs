using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Faeflame;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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
            NPC.width = 8;
            NPC.height = 8;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 10;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.value = 0f;
            NPC.knockBackResist = 0f;
            NPC.aiStyle = 65;
        }

        public override void AI()
        {
            NPC.TargetClosest(true);
            Player Player = Main.player[NPC.target];
            AbilityHandler mp = Player.GetHandler();
            Vector2 distance = Player.Center - NPC.Center;

            Dust.NewDustPerfect(NPC.Center, DustType<Dusts.Air>(), Vector2.Zero);

            if (distance.Length() <= 180 && !mp.Unlocked<Whip>() || Main.dayTime) NPC.ai[3] = 1;

            if (NPC.ai[3] == 1)
            {
                NPC.velocity.Y = 10;
                NPC.velocity.X = 0;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0;
    }

    internal class DesertWisp2 : DesertWisp
    {
        public override string Texture => "StarlightRiver/Assets/Invisible";
        public Trail trail;

		public override void SetStaticDefaults()
		{
            NPCID.Sets.TrailCacheLength[NPC.type] = 120;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
        }

		public override void SetDefaults()
		{
            base.SetDefaults();

            if (!Main.dedServ)
                trail = new Trail(Main.graphics.GraphicsDevice, 120, new NoTip(), TrailWidth, TrailColor);
        }

		private static Color TrailColor(Vector2 textureCoordinates)
		{
            return Color.Lerp(new Color(100, 200, 255), new Color(80, 120, 255), (float)Math.Sin(textureCoordinates.X * 6.28f + Main.GameUpdateCount / 100f)) * (float)Math.Pow(Math.Sin((1 - textureCoordinates.X) * 3.14f), 4);

            //return Color.Lerp(new Color(255, 70, 40), new Color(255, 160, 60), (float)Math.Sin(textureCoordinates.X * 6.28f + Main.GameUpdateCount / 100f)) * (float)Math.Pow(Math.Sin((1 - textureCoordinates.X) * 3.14f), 4);
        }

		private static float TrailWidth(float factorAlongTrail)
		{
            return (float)(Math.Sin(factorAlongTrail * 3.14f) * 36) * factorAlongTrail;
		}

		public override void AI()
        {
            if (NPC.velocity.Length() < 1.7f)
                NPC.velocity = Vector2.Normalize(NPC.velocity) * 1.71f;
        }

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
            if (NPC.oldPos[119] == Vector2.Zero || trail is null)
                return;

            trail.Positions = NPC.oldPos;

            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-screenPos.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);
            effect.Parameters["time"].SetValue(-Main.GameUpdateCount / 100f);
            effect.Parameters["repeats"].SetValue(1);

            effect.CurrentTechnique.Passes[0].Apply();

            trail?.Render(effect);
        }

		public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.player.InModBiome(ModContent.GetInstance<VitricDesertBiome>()) ? 50f : 0f;
    }
}