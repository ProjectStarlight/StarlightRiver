using StarlightRiver.Content.Items.Hell;
using StarlightRiver.Content.Items.Infernal;
using System;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Actors
{
	internal class HellLavaActor : ModNPC
	{
		const int DUST_RANGE = 250;//used for horizontal dust distance and circularrange of underwater lights
		const int ITEM_RANGE = 200;//circular range for detecting items

		public ref float Timer => ref NPC.ai[0];
		public ref float Fadeout => ref NPC.ai[1];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			NPC.width = 200;
			NPC.height = 10;
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
			Timer++;

			if (Fadeout > 0)
				Fadeout++;
			/*
			if (NPC.wet || WorldGen.SolidTile((int)(NPC.position.X / 16), (int)(NPC.position.Y / 16)))//float to surface if in water or blocks
				NPC.position.Y -= 1;
			else if (Main.tile[(int)(NPC.position.X / 16), (int)(NPC.position.Y / 16)].LiquidAmount < 1)
				NPC.position.Y += 1;*/

			float rand = Main.rand.NextFloat();
			Dust.NewDustPerfect(NPC.Center + Vector2.UnitX * Main.rand.NextFloat(-100, 100), DustType<Dusts.Cinder>(), Vector2.UnitY * rand * -4, 0, new Color(255, (int)((1 - rand) * 200), 0), Main.rand.NextFloat());

			if (Timer % 16 == 0 && Main.rand.NextBool(2))
				Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(Main.rand.NextFloat(-80, 80), 20), Vector2.Zero, ProjectileType<FirePillar>(), 0, 0, Main.myPlayer, Main.rand.NextFloat(0.75f, 1f));

			foreach (Item item in Main.item)
			{
				if (item.Hitbox.Intersects(NPC.Hitbox) && HellLavaConversion.GetConversionType(item) != 0)
				{
					item.type = HellLavaConversion.GetConversionType(item);
					item.SetDefaults(item.type);
					item.GetGlobalItem<HellTransformItem>().transformed = true;
					item.velocity = Vector2.UnitY * -15;

					for (int k = 0; k < 50; k++)
					{
						float rand2 = Main.rand.NextFloat();
						Dust.NewDustPerfect(item.Center, DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(1, 1) * rand2 * -10, 0, new Color(255, (int)((1 - rand2) * 200), 0), Main.rand.NextFloat(1f, 3f));
					}

					Projectile.NewProjectile(item.GetSource_FromThis(), item.Center + new Vector2(0, 20), Vector2.Zero, ProjectileType<FirePillar>(), 0, 0, Main.myPlayer, 2.25f);

					Projectile.NewProjectile(item.GetSource_FromThis(), item.Center + new Vector2(10, 20), Vector2.Zero, ProjectileType<FirePillar>(), 0, 0, Main.myPlayer, 1.25f);
					Projectile.NewProjectile(item.GetSource_FromThis(), item.Center + new Vector2(-5, 20), Vector2.Zero, ProjectileType<FirePillar>(), 0, 0, Main.myPlayer, 1.5f);

					Helpers.Helper.PlayPitched("Magic/FireHit", 1, -0.25f, item.Center);

					Fadeout = 1;
				}
			}

			if (Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.MultiplayerClient)//clientside only
			{
				float distToPlayer = Vector2.Distance(Main.LocalPlayer.Center, NPC.Center);

				if (distToPlayer < HellLavaConversion.MAX_GLOW)//makes the transformable items of nearby players glow
					HellLavaConversion.HellLavaItemGlow = Math.Max(HellLavaConversion.HellLavaItemGlow, 1 - distToPlayer / HellLavaConversion.MAX_GLOW);
			}

			if (Fadeout > 60)
				NPC.active = false;
		}

		public override void DrawBehind(int index)
		{
			Main.instance.DrawCacheNPCsOverPlayers.Add(index);
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/GlowTrailVertical").Value;

			var target = new Rectangle(0, 0, 400, 200);
			target.Offset((NPC.Center + new Vector2(-200, -196) - Main.screenPosition).ToPoint());

			Effect effect = Filters.Scene["ColoredFire"].GetShader().Shader;

			if (effect is null)
				return;

			float opacity = Timer < 60 ? Swoop(Timer / 60f) : 1;
			opacity *= 1 - Fadeout / 60f;

			effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f % 2f);
			effect.Parameters["primary"].SetValue(new Vector3(1, 0.75f, 0.05f) * opacity);
			effect.Parameters["primaryScaling"].SetValue(new Vector3(1, 1.5f, 1));
			effect.Parameters["secondary"].SetValue(new Vector3(0.75f, 0.05f, 0.05f) * opacity);

			effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);
			effect.Parameters["mapTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);

			spriteBatch.Draw(tex, target, Color.White * opacity);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
		}

		private float Swoop(float input)
		{
			if (input < 0.2f)
				return input * 20;
			else
				return 4 - (input - 0.2f) / 0.8f * 3;
		}
	}

	internal class FirePillar : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.tileCollide = false;
			Projectile.timeLeft = 60;
			Projectile.hide = true;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			overWiresUI.Add(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Misc/SpikeTell").Value;

			var frame = new Rectangle(59, 0, 59, 120);

			Effect effect = Filters.Scene["ColoredFire"].GetShader().Shader;

			if (effect is null)
				return false;

			float Timer = 60 - Projectile.timeLeft;
			float opacity = (Timer < 60 ? Swoop(Timer / 60f) : 1) * Projectile.ai[0];

			effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f % 2f);
			effect.Parameters["primary"].SetValue(new Vector3(1, 0.95f, 0.05f) * opacity);
			effect.Parameters["primaryScaling"].SetValue(new Vector3(1, 1.5f, 1));
			effect.Parameters["secondary"].SetValue(new Vector3(0.75f, 0.45f, 0.05f) * opacity);

			effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);
			effect.Parameters["mapTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, Color.White * opacity, 0, new Vector2(28, 114), opacity, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}

		private float Swoop(float input)
		{
			if (input < 0.2f)
				return input * 20;
			else
				return 4 - (input - 0.2f) / 0.8f * 3;
		}
	}

	public class HellLavaConversion : IOrderedLoadable
	{
		public static float HellLavaItemGlow; //brightness for item glow, taken from distance of closetest starwater actor (0 - 1 scale)
		public const float MAX_GLOW = 1000f;

		private static Dictionary<int, int> ConversionTable; //from/to

		public float Priority => 1f;

		//returns 0 if there is no defined type
		public static int GetConversionType(Item item)
		{
			if (ConversionTable.TryGetValue(item.type, out int conversionType))
				return conversionType;

			return 0;
		}

		public static bool ShouldItemGlow(Item item) //this does not include vanity items
		{
			return ConversionTable.ContainsKey(item.type);
		}

		private static void ResetInventoryGlow(StarlightPlayer Player)
		{
			if (HellLavaItemGlow > 0.075f)
				HellLavaItemGlow *= 0.985f; //fades out the shine on items
			else
				HellLavaItemGlow = 0; //rounds to zero as there is a check on the item to save performance
		}

		public void Load()
		{
			StarlightPlayer.ResetEffectsEvent += ResetInventoryGlow;

			ConversionTable = new()
			{
				{ ItemID.NightmarePickaxe, ItemType<InfernalHarvest>() },
				{ ItemID.DeathbringerPickaxe, ItemType<InfernalHarvest>() },
				{ ItemID.CloudinaBottle, ItemType<FuryInABottle>() },
			};
		}

		public void Unload()
		{
			StarlightPlayer.ResetEffectsEvent -= ResetInventoryGlow;
			ConversionTable = null;
		}
	}

	public class HellTransformItem : GlobalItem
	{
		public bool transformed;

		public override bool InstancePerEntity => true;

		public override GlobalItem Clone(Item item, Item itemClone)
		{
			return item.TryGetGlobalItem(out HellTransformItem gi) ? gi : base.Clone(item, itemClone);
		}

		public override bool PreDrawInInventory(Item Item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			if (HellLavaConversion.HellLavaItemGlow != 0 && HellLavaConversion.ShouldItemGlow(Item))
			{
				RasterizerState RasterizerCullMode = spriteBatch.GraphicsDevice.RasterizerState;
				SamplerState SamplerMode = spriteBatch.GraphicsDevice.SamplerStates[0];

				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, SamplerMode, default, RasterizerCullMode, default, Main.UIScaleMatrix);

				Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
				spriteBatch.Draw(tex, position, null, new Color(255, 140, 60) * (HellLavaConversion.HellLavaItemGlow + (float)Math.Sin(StarlightWorld.visualTimer) * 0.2f), 0, tex.Size() / 2, 1, 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, SamplerMode, default, RasterizerCullMode, default, Main.UIScaleMatrix);
			}

			return base.PreDrawInInventory(Item, spriteBatch, position, frame, drawColor, ItemColor, origin, scale);
		}

		public override void PostUpdate(Item Item)
		{
			if (transformed)
			{
				Item.velocity.Y *= 0.96f;

				if (Item.velocity.Y > 0)
					Item.velocity.Y *= 0.6f;

				float rand2 = Main.rand.NextFloat();
				Dust.NewDustPerfect(Item.Center, DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(1, 1) * rand2 * -3, 0, new Color(255, (int)((1 - rand2) * 200), 0), Main.rand.NextFloat());
			}
		}

		public override bool PreDrawInWorld(Item Item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			if (transformed)
			{
				RasterizerState RasterizerCullMode = spriteBatch.GraphicsDevice.RasterizerState;
				SamplerState SamplerMode = spriteBatch.GraphicsDevice.SamplerStates[0];

				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, SamplerMode, default, RasterizerCullMode, default, Main.GameViewMatrix.TransformationMatrix);

				Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
				spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, new Color(255, 140, 60), 0, tex.Size() / 2, 1, 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, SamplerMode, default, RasterizerCullMode, default, Main.GameViewMatrix.TransformationMatrix);
			}

			return base.PreDrawInWorld(Item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
		}
	}
}