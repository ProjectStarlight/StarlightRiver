using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Systems.MetaballSystem;
using System.Linq;
using Terraria.ID;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.NPCs.Moonstone
{

	internal class DreamBeastDrawSystem : ModSystem
	{
		public override void Load()
		{
			On_Main.DrawNPCs += DrawDreamBeast;
		}

		private void DrawDreamBeast(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
		{
			orig(self, behindTiles);

			if (!behindTiles)
				DreamBeastActor.DrawSpecial();
		}
	}

	internal class DreamBeastActor : MetaballActor
	{
		public NPC activeBeast;

		public override bool Active => NPC.AnyNPCs(ModContent.NPCType<Dreambeast>());

		public override Color OutlineColor => new Color(220, 200, 255) * (activeBeast?.Opacity ?? 0) * 0.5f;

		public override void DrawShapes(SpriteBatch spriteBatch)
		{
			for (int k = 0; k < Main.maxNPCs; k++)
			{
				NPC npc = Main.npc[k];

				if (npc.ModNPC is Dreambeast)
					(npc.ModNPC as Dreambeast).DrawToMetaballs(spriteBatch);
			}
		}

		public override bool PostDraw(SpriteBatch spriteBatch, Texture2D target)
		{
			activeBeast = Main.npc.FirstOrDefault(n => n.active && n.type == ModContent.NPCType<Dreambeast>()); //TODO: proper find for onscreen beast

			return false;
		}

		public static void DrawSpecial()
		{
			MetaballSystem.actorsSem.WaitOne();

			NPC activeBeast = Main.npc.FirstOrDefault(n => n.active && n.type == ModContent.NPCType<Dreambeast>());

			if (activeBeast is null)
			{
				MetaballSystem.actorsSem.Release();
				return;
			}

			Texture2D target = MetaballSystem.actors.FirstOrDefault(n => n is DreamBeastActor).Target.RenderTarget;

			float lunacy = Main.LocalPlayer.GetModPlayer<LunacyPlayer>().lunacy;
			if (lunacy >= 0 && lunacy < 100)
			{
				Effect effect = Filters.Scene["MoonstoneBeastEffect"].GetShader().Shader;
				effect.Parameters["baseTexture"].SetValue(target);
				effect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise2").Value);
				effect.Parameters["size"].SetValue(target.Size());
				effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.005f);
				effect.Parameters["opacity"].SetValue(lunacy / 100f);
				effect.Parameters["noiseSampleSize"].SetValue(new Vector2(800, 800));
				effect.Parameters["noisePower"].SetValue(100f);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);
			}

			Main.spriteBatch.Draw(target, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2, 0, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			MetaballSystem.actorsSem.Release();
		}
	}

	class DreamBeastSpawner : PlayerTicker
	{
		public override int TickFrequency => 30;

		public override bool Active(Player Player)
		{
			return Player.InModBiome(ModContent.GetInstance<MoonstoneBiome>());
		}

		public override void Tick(Player Player)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return; //only spawn for singlePlayer or on the server

			if (Player.GetModPlayer<LunacyPlayer>().lunacy > 0 && Main.npc.Count(n => n.active && n.type == ModContent.NPCType<Dreambeast>() && n.position.Distance(Player.position) < 3000) == 0)
			{
				Vector2 pos = Player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(300, 500);
				NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)pos.X, (int)pos.Y, ModContent.NPCType<Dreambeast>());
			}
		}
	}
}
