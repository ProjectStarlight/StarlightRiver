using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Crimson
{
	enum ThoughtPoliceState : int
	{
		Idle,
		Aggroed
	}

	internal class ThoughtPolice : ModNPC
	{
		public static List<ThoughtPolice> toRender = new();

		public float localScanOpacity;

		public float homeX;

		public ref float Timer => ref NPC.ai[0];

		public ThoughtPoliceState State
		{
			get => (ThoughtPoliceState)NPC.ai[1];
			set => NPC.ai[1] = (float)value;
		}

		public ref float AimRotation => ref NPC.ai[2];
		public ref float Variant => ref NPC.ai[3];

		public override string Texture => AssetDirectory.CrimsonNPCs + Name;

		public override void Load()
		{
			GraymatterBiome.onDrawOverHallucinationMap += DrawThoughPoliceHallucination;
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
		}

		public override void SetDefaults()
		{
			NPC.width = 52;
			NPC.height = 94;
			NPC.knockBackResist = 0.0f;
			NPC.lifeMax = 200;
			NPC.noGravity = true;
			NPC.noTileCollide = false;
			NPC.damage = 25;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;

			toRender.Add(this);
		}

		public override void AI()
		{
			Timer++;

			// calculate local scan opacity
			if (Main.LocalPlayer.HasBuff(ModContent.BuffType<CrimsonHallucination>()) && localScanOpacity < 1)
				localScanOpacity += 0.05f;
			if (!Main.LocalPlayer.HasBuff(ModContent.BuffType<CrimsonHallucination>()) && localScanOpacity > 0)
				localScanOpacity -= 0.05f;

			// check for and set home X position if needed
			if (homeX == 0)
				homeX = NPC.Center.X;

			switch(State)
			{
				case ThoughtPoliceState.Idle:

					// Idle bobbing
					NPC.position.Y += MathF.Sin(Timer * 0.05f) * 0.5f;

					// Patrol
					NPC.position.X = homeX + MathF.Sin(Timer * 6.28f / 2400) * 500f;

					// Hover up if needed
					bool foundGround = false;

					for(int k = 0; k < 30; k++)
					{
						var tile = Framing.GetTileSafely((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16 + k);

						if (tile.HasTile && Main.tileSolid[tile.TileType])
						{
							NPC.velocity.Y -= 0.05f;
							foundGround = true;
							break;
						}
					}

					if (!foundGround)
						NPC.velocity.Y += 0.05f;

					if (Math.Abs(NPC.velocity.Y) > 6)
					{
						NPC.velocity.Y *= 6f / Math.Abs(NPC.velocity.Y);
					}

					NPC.velocity.Y *= 0.95f;

					// rotate cone
					AimRotation = 1.57f + MathF.Sin(Timer * 6.28f / 600);

					break;

				case ThoughtPoliceState.Aggroed:
					break;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var tex = Assets.NPCs.Crimson.ThoughtPolice.Value;
			Rectangle frame = new Rectangle((int)Variant * 104, (int)(Timer / 10 % 6) * 148, 104, 148);

			spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, frame, drawColor, NPC.rotation, frame.Size() / 2f ,NPC.scale, 0, 0);

			return false;
		}

		private void DrawThoughPoliceHallucination(SpriteBatch spriteBatch)
		{
			if (toRender != null && toRender.Count > 0)
			{
				var tex = Assets.Misc.TriTellTransparent.Value;

				foreach(ThoughtPolice tp in toRender)
				{
					Vector2 pos = tp.NPC.Center + new Vector2(0, -16) - Main.screenPosition;
					Vector2 origin = new Vector2(0, tex.Height);
					Color color = new Color(255, 100, 100, (byte)(125 * tp.localScanOpacity));

					spriteBatch.Draw(tex, pos, null, color, tp.AimRotation + 1.57f / 2f, origin, 1f, 0, 0);
				}

				toRender.RemoveAll(n => n is null || !n.NPC.active);
			}
		}
	}
}
