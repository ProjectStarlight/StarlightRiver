﻿using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Packets;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static Terraria.GameContent.Animations.Actions.NPCs;

namespace StarlightRiver.Content.Tiles.Permafrost
{
	class Touchstone : DummyTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/Touchstone";

		public override int DummyType => DummySystem.DummyType<TouchstoneDummy>();

		public override void SetStaticDefaults()
		{
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<TouchstoneTileEntity>().Hook_AfterPlacement, -1, 0, false);
			Main.tileLighted[Type] = true;
			Main.tileSpelunker[Type] = true;
			Main.tileOreFinderPriority[Type] = 490;//just below chests

			TileID.Sets.AvoidedByMeteorLanding[Type] = true;//prevents meteor landing on aboveground structures

			QuickBlock.QuickSetFurniture(this, 2, 4, DustID.Ice, SoundID.Tink, false, new Color(155, 200, 255), false, false, "Touchstone");
			MinPick = int.MaxValue;
		}

		public override bool CanExplode(int i, int j)
		{
			return false;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			float sin1 = 1 + (float)Math.Sin(Main.GameUpdateCount / 10f);
			float cos1 = 1 + (float)Math.Cos(Main.GameUpdateCount / 10f);

			float bright = 1f;

			(r, g, b) = ((0.5f + cos1 * 0.2f) * bright, 0.8f * bright, (0.5f + sin1 * 0.2f) * bright);
		}

		public override void SafeNearbyEffects(int i, int j, bool closer)
		{
			Tile tile = Main.tile[i, j];

			if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
			{
				float sin1 = 1 + (float)Math.Sin(Main.GameUpdateCount / 10f);
				float cos1 = 1 + (float)Math.Cos(Main.GameUpdateCount / 10f);
				var auroraColor = new Color(0.5f + cos1 * 0.2f, 0.8f, 0.5f + sin1 * 0.2f, 0);

				if (Main.rand.NextBool(4))
				{
					var d = Dust.NewDustPerfect(new Vector2(i * 16 + 16 + Main.rand.NextFloat(-12, 12), j * 16 + 60), ModContent.DustType<Dusts.PixelatedEmber>(), Vector2.UnitY * -Main.rand.NextFloat(2), 0, auroraColor, Main.rand.NextFloat(0.2f));
				}
			}
		}

		public override bool RightClick(int i, int j)
		{
			// Prevent spawning a wisp if there is already one in the world (because their movement and killing is client-side there can be multiple in multiplayer)
			if (Main.npc.Any(n => n.active && n.type == ModContent.NPCType<TouchstoneWisp>()))
				return false;

			Tile tile = Framing.GetTileSafely(i, j);
			i -= tile.TileFrameX / 18;
			j -= tile.TileFrameY / 18;

			int index = ModContent.GetInstance<TouchstoneTileEntity>().Find(i, j);

			if (index == -1)
				return true;

			var entity = (TouchstoneTileEntity)TileEntity.ByID[index];

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				var packet = new SpawnNPC(i * 16, j * 16, ModContent.NPCType<TouchstoneWisp>(), ai0: entity.targetPoint.X, ai1: entity.targetPoint.Y);
				packet.Send(-1, -1, false);
			}

			NPC.NewNPC(new EntitySource_TileInteraction(null, i, j), i * 16, j * 16, ModContent.NPCType<TouchstoneWisp>(), ai0: entity.targetPoint.X, ai1: entity.targetPoint.Y);

			return true;
		}

		public override void MouseOver(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.cursorItemIconID = ModContent.ItemType<Items.Hovers.GenericHover>();
			Player.noThrow = 2;
			Player.cursorItemIconEnabled = true;
		}
	}

	internal sealed class TouchstoneTileEntity : ModTileEntity
	{
		public Vector2 targetPoint;

		public override bool IsTileValidForEntity(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			return tile.TileType == ModContent.TileType<Touchstone>() && tile.HasTile && tile.TileFrameX == 0 && tile.TileFrameY == 0;
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendTileSquare(Main.myPlayer, i - 1, j - 1, 3);
				NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i - 1, j - 1, Type, 0f, 0, 0, 0);
				return -1;
			}

			return Place(i - 1, j - 3);
		}

		public override void SaveData(TagCompound tag)
		{
			tag["Point"] = targetPoint;
		}

		public override void LoadData(TagCompound tag)
		{
			targetPoint = tag.Get<Vector2>("Point");
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.WriteVector2(targetPoint);
		}

		public override void NetReceive(BinaryReader reader)
		{
			targetPoint = reader.ReadVector2();
		}
	}

	public class TouchstoneDummy : Dummy
	{
		public TouchstoneDummy() : base(ModContent.TileType<Touchstone>(), 32, 48) { }

		public override void PostDraw(Color lightColor)
		{
			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderTiles", () =>
			{
				Color auroraColor;

				float sin1 = 1 + (float)Math.Sin(Main.GameUpdateCount / 10f);
				float cos1 = 1 + (float)Math.Cos(Main.GameUpdateCount / 10f);
				auroraColor = new Color(0.5f + cos1 * 0.2f, 0.8f, 0.5f + sin1 * 0.2f);

				Texture2D portalGlow = Assets.Bosses.SquidBoss.PortalGlow.Value;

				Color portalGlowColor = auroraColor;
				portalGlowColor.A = 0;

				for (int k = 0; k < 5; k++)
				{
					float sin = (float)Math.Sin(Main.GameUpdateCount * 0.05f + k * 0.5f);
					var target = new Rectangle((int)position.X + 16, (int)position.Y + 70, (int)(portalGlow.Width * 0.5f * (1.2f - k * 0.2f + 0.05f * sin)), (int)(portalGlow.Height * (0.1f + 0.15f * k + 0.05f * sin)));

					target.Offset((-Main.screenPosition).ToPoint());

					Main.spriteBatch.Draw(portalGlow, target, null, portalGlowColor * 0.3f, 0, new Vector2(portalGlow.Width / 2, portalGlow.Height), 0, 0);
				}
			});
		}
	}

	[SLRDebug]
	class TouchstoneItem : QuickTileItem
	{
		public TouchstoneItem() : base("Touchstone", "A guiding light", "Touchstone", 3, AssetDirectory.PermafrostTile) { }
	}

	class TouchstoneWisp : ModNPC //not sure if this is really a great place to put this but ehhhh
	{

		private ref float TargetX => ref NPC.ai[0];
		private ref float TargetY => ref NPC.ai[1];
		public Vector2 TargetPos => new(TargetX, TargetY);

		private List<Vector2> cache;
		private Trail trail;

		public override string Texture => AssetDirectory.SquidBoss + "InkBlob";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Aurora Spirit");
		}

		public override void SetDefaults()
		{
			NPC.width = 40;
			NPC.height = 40;
			NPC.aiStyle = -1;
			NPC.noTileCollide = true;
			NPC.friendly = true;
			NPC.lifeMax = 10;
			NPC.noGravity = true;
			NPC.dontTakeDamage = true;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			database.Entries.Remove(bestiaryEntry);
		}

		public override void AI()
		{
			foreach (Player player in Main.player.Where(n => Vector2.Distance(n.Center, NPC.Center) < 1000))
			{
				player.AddBuff(ModContent.BuffType<TouchstoneWispBuff>(), 60);
			}

			if (TargetPos == Vector2.Zero)
				return;

			if (NPC.Opacity < 1f)
				NPC.Opacity += 0.1f;

			NPC.rotation += Main.rand.NextFloat(0.2f);

			if (Main.player.Any(n => n.active && !n.dead && Vector2.Distance(n.Center, NPC.Center) < 300))
			{ //this is clientside only and not synced
				NPC.velocity += Vector2.Normalize(TargetPos - NPC.Center) * 0.05f;

				if (NPC.velocity.Length() > 6)
					NPC.velocity = Vector2.Normalize(NPC.velocity) * 5.9f;
			}
			else
			{
				NPC.velocity *= 0.91f;
			}

			NPC.velocity += NPC.velocity.RotatedBy(1.57f) * (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.05f;

			float sin = 1 + (float)Math.Sin(Main.GameUpdateCount / 10f);
			float cos = 1 + (float)Math.Cos(Main.GameUpdateCount / 10f);
			Color color = new Color(0.3f + cos * 0.3f, 0.6f, 0.3f + sin * 0.3f, 0) * (NPC.timeLeft < 30 ? (NPC.timeLeft / 30f) : 1);

			Lighting.AddLight(NPC.Center, color.ToVector3() * 0.8f);

			if (Main.rand.NextBool(4))
			{
				var d = Dust.NewDustPerfect(NPC.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), ModContent.DustType<Dusts.PixelatedGlow>(), Vector2.Zero, 0, color, Main.rand.NextFloat(0.2f));
				d.customData = Main.rand.NextFloat(0.5f, 1);
			}

			if (Vector2.Distance(NPC.Center, TargetPos) < 64 || TargetPos == Vector2.Zero)
			{
				NPC.velocity *= 0.91f;
				NPC.scale *= 0.95f;

				if (NPC.scale < 0.05f)
				{
					foreach (TileEntity entity in TileEntity.ByID.Values)
					{
						if (entity.type == ModContent.TileEntityType<TouchstoneTileEntity>() && Vector2.Distance(NPC.Center, entity.Position.ToVector2() * 16) < 120)
						{
							NPC.Kill(); // because movement is client side the actual wisp doesn't get killed and replaced by the server until players move far away from it, which is probably okay

							for (int n = 0; n < 50; n++)
							{
								Vector2 pos = NPC.Center;
								Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, color, 0.2f);
							}
						}
					}
				}
			}

			if (StarlightWorld.squidBossArena.Contains((NPC.Center / 16).ToPoint()))
			{
				NPC.Kill();

				for (int n = 0; n < 50; n++)
				{
					Vector2 pos = NPC.Center;
					Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), 0, color, 0.2f);
				}
			}

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("OverPlayers", () =>
			{
				DrawPrimitives();

				Texture2D glowTex = Assets.Masks.GlowAlpha.Value;
				Texture2D star = Assets.StarTexture.Value;

				float sin1 = 1 + (float)Math.Sin(Main.GameUpdateCount / 10f);
				float cos1 = 1 + (float)Math.Cos(Main.GameUpdateCount / 10f);
				var auroraColor = new Color(0.3f + cos1 * 0.3f, 0.6f, 0.3f + sin1 * 0.3f, 0);

				auroraColor *= 1f - NPC.velocity.Length();

				spriteBatch.Draw(glowTex, NPC.Center - Main.screenPosition, null, auroraColor * NPC.Opacity * 1.5f, 0f, glowTex.Size() / 2, 0.7f * NPC.scale, SpriteEffects.None, 0f);
				spriteBatch.Draw(star, NPC.Center - Main.screenPosition, null, auroraColor * NPC.Opacity * 1.5f, 0f, star.Size() / 2, 0.3f * NPC.scale, SpriteEffects.None, 0f);
			});

			return false;
		}

		protected void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 30; i++)
				{
					cache.Add(NPC.Center);
				}
			}

			cache.Add(NPC.Center);

			while (cache.Count > 30)
			{
				cache.RemoveAt(0);
			}
		}

		protected void ManageTrail()
		{
			if (trail is null || trail.IsDisposed)
			{
				trail = new Trail(Main.instance.GraphicsDevice, 30, new NoTip(), factor => 32 * MathF.Sin(factor * 3.14f), factor =>
							{
								float alpha = MathF.Sin(factor.X * 3.14f);

								if (factor.X == 1)
									alpha = 0;

								if (NPC.timeLeft < 20)
									alpha *= NPC.timeLeft / 20f;

								float sin = 1 + MathF.Sin(factor.X * 10);
								float cos = 1 + MathF.Cos(factor.X * 10);
								Color color = new Color(0.3f + cos * 0.3f, 0.6f, 0.3f + sin * 0.3f);

								return color * alpha * NPC.Opacity;
							});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = NPC.Center + NPC.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

			if (effect != null)
			{
				var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
				Matrix view = Matrix.Identity;
				var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

				effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
				effect.Parameters["repeats"].SetValue(1f);
				effect.Parameters["transformMatrix"].SetValue(world * view * projection);
				effect.Parameters["sampleTexture"].SetValue(Assets.BlurryTrail.Value);
				trail?.Render(effect);

				effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f + 0.2f);
				trail?.Render(effect);
			}
		}
	}

	class TouchstoneWispBuff : SmartBuff
	{
		public override string Texture => "StarlightRiver/Assets/Buffs/ProtectiveShard";

		public TouchstoneWispBuff() : base("Aurora Curiosity", "What lies below?\nIncreased mining speed", false) { }

		public override void Update(Player player, ref int buffIndex)
		{
			player.pickSpeed /= 2;
		}
	}
}