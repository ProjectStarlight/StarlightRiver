using Terraria.Graphics.Effects;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Actors
{
	internal class HellLavaActor : ModNPC
	{
		const int DUST_RANGE = 250;//used for horizontal dust distance and circularrange of underwater lights
		const int ITEM_RANGE = 200;//circular range for detecting items

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			NPC.width = 1;
			NPC.height = 1;
			NPC.lifeMax = 100;
			NPC.immortal = true;
			NPC.dontTakeDamage = true;
			NPC.friendly = true;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.hide = true;
		}

		public override void AI()
		{
			if (NPC.wet || WorldGen.SolidTile((int)(NPC.position.X / 16), (int)(NPC.position.Y / 16)))//float to surface if in water or blocks
				NPC.position.Y -= 1;
			else if (Main.tile[(int)(NPC.position.X / 16), (int)(NPC.position.Y / 16)].LiquidAmount < 1)
				NPC.position.Y += 1;

			int y = 0;

			float rand = Main.rand.NextFloat();
			Dust.NewDustPerfect(NPC.Center + Vector2.UnitX * Main.rand.NextFloat(-100, 100), DustType<Dusts.Cinder>(), Vector2.UnitY * rand * -4, 0, new Color(255, (int)((1 - rand) * 200), 0), Main.rand.NextFloat());

			/*
			if (Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.MultiplayerClient)//clientside only
			{
				float distToPlayer = Vector2.Distance(Main.LocalPlayer.Center, NPC.Center);

				if (distToPlayer < StarwaterConversion.MaxItemGlow)//makes the transformable items of nearby players glow
					StarwaterConversion.StarwaterGlobalItemGlow = Math.Max(StarwaterConversion.StarwaterGlobalItemGlow, 1 - distToPlayer / StarwaterConversion.MaxItemGlow);
			}

			Vector2 surfaceLightPos = NPC.Center + Vector2.UnitX * Main.rand.NextFloat(-DUST_RANGE, DUST_RANGE) + Vector2.UnitY * Main.rand.NextFloat(-6, 0);
			Tile tile = Framing.GetTileSafely(surfaceLightPos);
			Tile tileDown = Framing.GetTileSafely(surfaceLightPos + Vector2.UnitY * 16);

			if ((tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Water || tileDown.LiquidAmount > 0 && tileDown.LiquidType == LiquidID.Water) && Main.rand.Next(10) > 3)//surface lights
			{
				//smaller dusts that home in on item if it exists (the dusts do the checking)
				var d = Dust.NewDustPerfect(surfaceLightPos, ModContent.DustType<Dusts.AuroraSuction>(), Vector2.Zero, 200, new Color(Main.rand.NextBool(30) ? 200 : 0, Main.rand.Next(150), 255));
				d.customData = new Dusts.AuroraSuctionData(this, Main.rand.NextFloat(0.6f, 0.8f));

				//vertical light above water
				if (Main.rand.NextBool())
				{
					bool red = Main.rand.NextBool(35);
					bool green = Main.rand.NextBool(15) && !red;
					var color = new Color(red ? 255 : Main.rand.Next(10), green ? 255 : Main.rand.Next(100), Main.rand.Next(240, 255));

					Dust.NewDustPerfect(surfaceLightPos + new Vector2(0, Main.rand.Next(-4, 1)), DustType<Dusts.VerticalGlow>(), Vector2.UnitX * Main.rand.NextFloat(-0.15f, 0.15f), 200, color);
				}
			}

			//circular area of dust
			Vector2 circularLightPos = NPC.Center + Vector2.UnitX.RotatedByRandom(6.28f) * Main.rand.NextFloat(-DUST_RANGE, DUST_RANGE);
			Tile tile2 = Framing.GetTileSafely(circularLightPos);
			if (tile2.LiquidAmount > 0 && tile2.LiquidType == LiquidID.Water && Main.rand.NextBool(2))//under water lights
			{
				var d = Dust.NewDustPerfect(circularLightPos, DustType<Dusts.AuroraSuction>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(), 0, new Color(0, 50, 255), 0.5f);
				d.customData = new Dusts.AuroraSuctionData(this, Main.rand.NextFloat(0.4f, 0.5f));
			}*/
		}

		public override void DrawBehind(int index)
		{
			Main.instance.DrawCacheNPCsOverPlayers.Add(index);
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/GlowTrailVertical").Value;

			var target = new Rectangle(0, 0, 400, 200);
			target.Offset((NPC.Center + new Vector2(-200, -200) - Main.screenPosition).ToPoint());

			Effect effect = Filters.Scene["ColoredFire"].GetShader().Shader;

			if (effect is null)
				return;

			effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f % 2f);
			effect.Parameters["primary"].SetValue(new Vector3(1, 0.75f, 0));
			effect.Parameters["primaryScaling"].SetValue(new Vector3(1, 1.5f, 1));
			effect.Parameters["secondary"].SetValue(new Vector3(0.75f, 0, 0));

			effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);
			effect.Parameters["mapTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);

			spriteBatch.Draw(tex, target, Color.White);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
		}
	}
}
