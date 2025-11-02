using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.NPCs.Crimson;
using StarlightRiver.Content.Packets;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class GestaltAltar : DummyTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Crimson/GestaltAltar";

		public override int DummyType => DummySystem.DummyType<GestaltAltarDummy>();

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 5, 5, DustID.Blood, SoundID.Tink, false, new Color(200, 200, 200));
			MinPick = 101;
		}

		public override bool RightClick(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			(Dummy(i - tile.frameX / 18, j - tile.frameY / 18) as GestaltAltarDummy)?.Start();

			if (NPC.AnyNPCs(ModContent.NPCType<GestaltCell>()))
				return false;

			Rectangle arena = GestaltCellArenaSystem.ArenaWorld;
			Vector2 pos = arena.Center.ToVector2() + Vector2.UnitY * (arena.Height * 0.5f - 32);

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				var packet = new SpawnNPC((int)pos.X, (int)pos.Y, ModContent.NPCType<GestaltCell>());
				packet.Send(-1, -1, false);

				return true;
			}

			int n = Terraria.NPC.NewNPC(new EntitySource_BossSpawn(Main.LocalPlayer), (int)pos.X, (int)pos.Y, ModContent.NPCType<GestaltCell>());

			return true;
		}
	}

	internal class GestaltAltarDummy : Dummy
	{
		public int timer;
		public bool runningForLocalPlayer;
		public Vector2 savedPos;

		public GestaltAltarDummy() : base(ModContent.TileType<GestaltAltar>(), 5 * 16, 5 * 16) { }

		public void Start()
		{
			timer = 0;
			runningForLocalPlayer = true;
		}

		public override void Update()
		{
			if (runningForLocalPlayer && Main.netMode != NetmodeID.Server)
			{
				Player player = Main.LocalPlayer;
				timer++;

				if (timer == 1)
					savedPos = player.Center;

				if (timer < 150)
				{
					player.immune = true;
					player.immuneTime = 60;
					player.Center = savedPos;
					player.velocity.Y = 0;
				}

				if (timer > 45 && timer <= 75)
				{
					float prog = (timer - 45) / 30f;
					Fadeout.color = Color.Lerp(Color.DarkRed, Color.Black, prog);
					Fadeout.opacity = prog;
				}

				if (timer > 90 && timer < 120)
				{
					float prog = 1f - (timer - 90) / 30f;
					Fadeout.color = Color.Lerp(Color.DarkRed, Color.Black, prog);
					Fadeout.opacity = prog;
				}

				if (timer == 80)
				{
					Rectangle arena = GestaltCellArenaSystem.ArenaWorld;
					Vector2 pos = arena.Center.ToVector2() + Vector2.UnitY * (arena.Height * 0.5f - 120);
					savedPos = pos + Vector2.UnitX * Main.rand.Next(80, 160) * (Main.rand.NextBool() ? 1 : -1);
				}

				if (timer == 150)
				{
					runningForLocalPlayer = false;
					timer = 0;
				}
			}
		}

		public override void DrawOverPlayer()
		{
			if (timer <= 15)
				return;

			var tex = Assets.Tiles.Crimson.GestaltAltarAnimation.Value;
			Vector2 target = Main.LocalPlayer.Center - Main.screenPosition;
			Vector2 origin = Vector2.One * 50;
			Rectangle frame = new Rectangle(0, 0, 100, 100);

			if (timer > 15 && timer < 45)
			{
				frame.Y = (int)((timer - 15) / 30f * 5) * 100;
			}

			if (timer >= 45 && timer < 120)
			{
				frame.Y = 400;
			}

			if (timer > 120)
			{
				frame.Y = (int)((1 - (timer - 120) / 30f) * 5) * 100;
			}

			Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
			Main.spriteBatch.Draw(tex, target, frame, new Color(Lighting.GetSubLight(Main.LocalPlayer.Center)), 0, origin, 1, 0, 0);
			Main.spriteBatch.End();
		}
	}
}