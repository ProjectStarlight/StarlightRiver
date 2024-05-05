﻿using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Packets;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.LightingSystem;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class VitricBossAltar : DummyTile, IHintable
	{
		public override int DummyType => DummySystem.DummyType<VitricBossAltarDummy>();

		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSetFurniture(5, 7, DustType<Air>(), SoundID.Tink, false, new Color(200, 113, 113), false, false, "Ceiro's Altar");
			MinPick = int.MaxValue;
		}

		public override bool CanExplode(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			if (tile.TileType == TileType<VitricBossAltar>())
				return false;

			return base.CanExplode(i, j);
		}

		public override bool SpawnConditions(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			return tile.TileFrameX % 90 == 0 && tile.TileFrameY == 0;
		}

		public override void SafeNearbyEffects(int i, int j, bool closer)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			if (Main.rand.NextBool(200) && tile.TileFrameX < 90 && tile.TileFrameX > 16)
			{
				var pos = new Vector2(i * 16 + Main.rand.Next(16), j * 16 + Main.rand.Next(16));

				if (Main.rand.NextBool())
					Dust.NewDustPerfect(pos, DustType<CrystalSparkle>(), Vector2.Zero);
				else
					Dust.NewDustPerfect(pos, DustType<CrystalSparkle2>(), Vector2.Zero);
			}

			base.SafeNearbyEffects(i, j, closer);
		}

		public override void MouseOver(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			if (tile.TileFrameX >= 90)
			{
				Player Player = Main.LocalPlayer;
				Player.cursorItemIconID = ItemType<Items.Vitric.GlassIdol>();
				Player.noThrow = 2;
				Player.cursorItemIconEnabled = true;
			}
		}

		public override bool RightClick(int i, int j)
		{
			var tile = (Tile)Framing.GetTileSafely(i, j).Clone();
			Player player = Main.LocalPlayer;

			if (StarlightWorld.HasFlag(WorldFlags.VitricBossOpen) && tile.TileFrameX >= 90 && !NPC.AnyNPCs(NPCType<VitricBoss>()) && (player.ConsumeItem(ItemType<Items.Vitric.GlassIdol>()) || player.HasItem(ItemType<Items.Vitric.GlassIdolEndless>())))
			{
				int x = i - (tile.TileFrameX - 90) / 18;
				int y = j - tile.TileFrameY / 18;
				SpawnBoss(x, y, player);
				return true;
			}

			return false;
		}

		public void SpawnBoss(int i, int j, Player player)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				var packet = new SpawnNPC(i * 16 + 40, j * 16 + 556, NPCType<VitricBoss>());
				packet.Send(-1, -1, false);

				return;
			}

			int n = Terraria.NPC.NewNPC(new EntitySource_BossSpawn(player), i * 16 + 40, j * 16 + 556, NPCType<VitricBoss>());
			NPC NPC = Main.npc[n];

			if (NPC.type == NPCType<VitricBoss>())
				(Dummy(i, j) as VitricBossAltarDummy).boss = Main.npc[n];
		}

		public string GetHint()
		{
			Tile tile = Framing.GetTileSafely((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);

			if (tile.TileFrameX < 90)
				return "An altar, encased in crystal rich with binding Starlight. You'd have to use a Starlight power of equal strength...";
			else
				return "An altar awaiting an offering...";
		}
	}

	[SLRDebug]
	class VitricBossAltarItem : QuickTileItem
	{
		public VitricBossAltarItem() : base("Vitric Boss Altar Item", "{{{{Debug}}}} Item", "VitricBossAltar", 1, AssetDirectory.Debug, true) { }
	}

	internal class VitricBossAltarDummy : Dummy
	{
		private NPC arenaLeft;
		private NPC arenaRight;

		public NPC boss;

		public float barrierTimer;
		public float cutsceneTimer;

		public override bool DoesCollision => true;

		public VitricBossAltarDummy() : base(TileType<VitricBossAltar>(), 80, 112) { }

		bool collisionHappened = false;

		public override void Collision(Player Player)
		{
			var parentPos = new Point16((int)position.X / 16, (int)position.Y / 16);
			Tile parent = Framing.GetTileSafely(parentPos.X, parentPos.Y);

			if (parent.TileFrameX == 0 && Abilities.AbilityHelper.CheckDash(Player, Hitbox) && !collisionHappened)
			{
				collisionHappened = true;

				cutsceneTimer = 0;

				if (Main.netMode != NetmodeID.Server)
				{
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter);

					for (int k = 0; k < 100; k++)
					{
						Dust.NewDust(position, width, height, DustType<GlassGravity>(), 0, 0, 0, default, 1.2f);
					}

					if (Main.myPlayer == Player.whoAmI)
					{
						for (int x = parentPos.X; x < parentPos.X + 5; x++)
						{
							for (int y = parentPos.Y; y < parentPos.Y + 7; y++)
								Framing.GetTileSafely(x, y).TileFrameX += 90;
						}

						NetMessage.SendTileSquare(Player.whoAmI, parentPos.X, parentPos.Y, 5, 7, TileChangeType.None);
					}
				}
			}
		}

		public void FindParent()
		{
			boss = null;
			arenaLeft = null;
			arenaRight = null;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC NPC = Main.npc[i];

				if (NPC.active && NPC.type == NPCType<VitricBoss>())
					boss = NPC;

				if (NPC.active && NPC.type == NPCType<VitricBackdropLeft>())
					arenaLeft = NPC;

				if (NPC.active && NPC.type == NPCType<VitricBackdropRight>())
					arenaRight = NPC;
			}

			return;
		}

		public override void Update()
		{
			var parentPos = new Point16((int)position.X / 16, (int)position.Y / 16);
			Tile parent = Framing.GetTileSafely(parentPos.X, parentPos.Y);

			if (StarlightWorld.HasFlag(WorldFlags.VitricBossOpen) && cutsceneTimer < 660) //should prevent the cutscene from reoccuring?
				cutsceneTimer = 999;

			if (boss is null || arenaLeft is null || arenaRight is null)
				FindParent();

			//This controls spawning the rest of the arena
			if (arenaLeft is null || arenaRight is null || !arenaLeft.active || !arenaRight.active && Main.netMode != NetmodeID.MultiplayerClient)
			{
				foreach (NPC NPC in Main.npc.Where(n => n.active && //reset the arena if one of the sides somehow dies
				 (
				 n.type == NPCType<VitricBackdropLeft>() ||
				 n.type == NPCType<VitricBackdropRight>() ||
				 n.type == NPCType<VitricBossPlatformDown>() ||
				 n.type == NPCType<VitricBossPlatformDownSmall>() ||
				 n.type == NPCType<VitricBossPlatformUp>() ||
				 n.type == NPCType<VitricBossPlatformUpSmall>()
				 )))
				{
					NPC.active = false;
					NPC.netUpdate = true;
				}

				Vector2 center = Center + new Vector2(0, 60);
				int timerset = StarlightWorld.HasFlag(WorldFlags.VitricBossOpen) && cutsceneTimer >= 660 ? 360 : 0; //the arena should already be up if it was opened before

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					int index = NPC.NewNPC(GetSource_FromThis(), (int)center.X + 352, (int)center.Y, NPCType<VitricBackdropRight>(), 0, timerset);
					arenaRight = Main.npc[index];

					if (StarlightWorld.HasFlag(WorldFlags.VitricBossOpen) && Main.npc[index].ModNPC is VitricBackdropRight)
						(Main.npc[index].ModNPC as VitricBackdropRight).SpawnPlatforms(false);

					index = NPC.NewNPC(GetSource_FromThis(), (int)center.X - 352, (int)center.Y, NPCType<VitricBackdropLeft>(), 0, timerset);
					arenaLeft = Main.npc[index];

					if (StarlightWorld.HasFlag(WorldFlags.VitricBossOpen) && Main.npc[index].ModNPC is VitricBackdropLeft)
						(Main.npc[index].ModNPC as VitricBackdropLeft).SpawnPlatforms(false);
				}
			}

			if (parent.TileFrameX == 0)
				return;

			if (boss is null || !boss.active || boss.type != NPCType<VitricBoss>())
				boss = null;

			if (parent.TileFrameX == 90 && !StarlightWorld.HasFlag(WorldFlags.VitricBossOpen))
			{
				if (Main.LocalPlayer.InModBiome(GetInstance<VitricDesertBiome>()))
				{
					CameraSystem.shake += 1;
					Dust.NewDust(Center + new Vector2(-632, height / 2), 560, 1, DustType<Dusts.Sand>(), 0, Main.rand.NextFloat(-5f, -1f), Main.rand.Next(255), default, Main.rand.NextFloat(1.5f));
					Dust.NewDust(Center + new Vector2(72, height / 2), 560, 1, DustType<Dusts.Sand>(), 0, Main.rand.NextFloat(-5f, -1f), Main.rand.Next(255), default, Main.rand.NextFloat(1.5f));

					if (cutsceneTimer > 120 && cutsceneTimer <= 240)
						Main.musicFade[Main.curMusic] = 1 - (cutsceneTimer - 120) / 120f;

					if (cutsceneTimer == 180)
						Helper.PlayPitched("ArenaRise", 0.5f, -0.1f, Center);
				}

				cutsceneTimer++;

				if (cutsceneTimer >= 180 && !StarlightWorld.HasFlag(WorldFlags.VitricBossOpen))
				{
					StarlightWorld.Flag(WorldFlags.VitricBossOpen);

					if (Main.LocalPlayer.InModBiome(GetInstance<VitricDesertBiome>()))
					{
						CameraSystem.DoPanAnimation(VitricBackdropLeft.Risetime + 120, Center, Center + new Vector2(0, -400));
					}
				}
			}

			if (cutsceneTimer > 240 && cutsceneTimer < 660)
				Main.musicFade[Main.curMusic] = 0;

			cutsceneTimer++;

			//controls the drawing of the barriers
			if (barrierTimer < 120 && ShouldBarrierBeUp())
			{
				barrierTimer++;

				if (Main.LocalPlayer.InModBiome(GetInstance<VitricDesertBiome>()))
				{
					if (barrierTimer % 3 == 0)
						CameraSystem.shake += 2; //screenshake

					if (barrierTimer == 119) //hitting the top
					{
						CameraSystem.shake += 15;
						Helper.PlayPitched("VitricBoss/CeirosPillarImpact", 0.5f, 0, Center);
					}
				}
			}
			else if (barrierTimer > 0 && !ShouldBarrierBeUp())
			{
				barrierTimer--;
			}
		}

		private bool ShouldBarrierBeUp()
		{
			if (boss != null && boss.active)
				return true;

			NPC left = Main.npc.FirstOrDefault(n => n.ModNPC is VitricBackdropLeft);
			if (left?.ModNPC != null && (left.ModNPC as VitricBackdropLeft).State >= 3)
				return true;

			NPC right = Main.npc.FirstOrDefault(n => n.ModNPC is VitricBackdropRight);
			if (right?.ModNPC != null && (left.ModNPC as VitricBackdropRight).State >= 3)
				return true;

			return false;
		}

		private bool ShouldDrawReflection()
		{
			var parentPos = new Point16((int)position.X / 16, (int)position.Y / 16);
			Tile parent = Framing.GetTileSafely(parentPos.X, parentPos.Y);

			return parent.TileFrameX < 90 && Hitbox.Intersects(new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight));
		}

		public override void DrawBehindTiles()
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			var parentPos = new Point16((int)position.X / 16, (int)position.Y / 16);
			Tile parent = Framing.GetTileSafely(parentPos.X, parentPos.Y);

			if (parent.TileFrameX >= 90 && !NPC.AnyNPCs(NPCType<VitricBoss>()))
			{
				Texture2D texSkull = Assets.Symbol.Value;
				spriteBatch.Draw(texSkull, Center - Main.screenPosition, null, new Color(255, 100, 100) * (1 - Vector2.Distance(Main.LocalPlayer.Center, Center) / 200f), 0, texSkull.Size() / 2, 1, 0, 0);
			}

			else if (parent.TileFrameX < 90 && ReflectionTarget.canUseTarget)
			{
				if (ShouldDrawReflection())
				{
					ReflectionTarget.DrawReflection(spriteBatch, screenPos: position - Main.screenPosition, normalMap: Assets.Tiles.Vitric.VitricBossAltarReflectionMap.Value, flatOffset: new Vector2(-0.0075f, 0.011f), tintColor: new Color(150, 150, 255, 200), offsetScale: 0.05f);
					ReflectionTarget.isDrawReflectablesThisFrame = true;
				}

				Texture2D glow = Assets.Tiles.Vitric.VitricBossAltarGlow.Value;
				spriteBatch.Draw(glow, position - Main.screenPosition + new Vector2(-1, 7), glow.Frame(), Helper.IndicatorColorProximity(300, 600, Center), 0, Vector2.Zero, 1, 0, 0);
			}

			//Barriers
			Vector2 center = Center + new Vector2(0, 56);
			Texture2D tex = Assets.Bosses.VitricBoss.VitricBossBarrier.Value;
			Texture2D tex2 = Assets.Bosses.VitricBoss.VitricBossBarrier2.Value;
			Texture2D texTop = Assets.Bosses.VitricBoss.VitricBossBarrierTop.Value;
			//Color color = new Color(180, 225, 255);

			int off = (int)(barrierTimer / 120f * tex.Height);
			int off2 = (int)(barrierTimer / 120f * texTop.Width / 2);

			LightingBufferRenderer.DrawWithLighting(new Rectangle((int)center.X - 790 - (int)Main.screenPosition.X, (int)center.Y - off - 16 - (int)Main.screenPosition.Y, tex.Width, off), tex, new Rectangle(0, 0, tex.Width, off), default);
			LightingBufferRenderer.DrawWithLighting(new Rectangle((int)center.X + 606 - (int)Main.screenPosition.X, (int)center.Y - off - 16 - (int)Main.screenPosition.Y, tex.Width, off), tex2, new Rectangle(0, 0, tex.Width, off), default);

			//left
			LightingBufferRenderer.DrawWithLighting(new Rectangle((int)center.X - 592 - (int)Main.screenPosition.X, (int)center.Y - 1040 - (int)Main.screenPosition.Y, off2, texTop.Height), texTop, new Rectangle(texTop.Width / 2 - off2, 0, off2, texTop.Height), default);

			//right
			LightingBufferRenderer.DrawWithLighting(new Rectangle((int)center.X + 608 - off2 - (int)Main.screenPosition.X, (int)center.Y - 1040 - (int)Main.screenPosition.Y, off2, texTop.Height), texTop, new Rectangle(texTop.Width / 2, 0, off2, texTop.Height), default);

			//spriteBatch.Draw(tex, new Rectangle((int)center.X - 790 - (int)Main.screenPosition.X, (int)center.Y - off - 16 - (int)Main.screenPosition.Y, tex.Width, off),
			//new Rectangle(0, 0, tex.Width, off), color);

			//spriteBatch.Draw(tex, new Rectangle((int)center.X + 606 - (int)Main.screenPosition.X, (int)center.Y - off - 16 - (int)Main.screenPosition.Y, tex.Width, off),
			//new Rectangle(0, 0, tex.Width, off), color);
		}
	}
}