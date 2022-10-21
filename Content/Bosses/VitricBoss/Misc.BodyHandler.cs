using StarlightRiver.Content.Physics;
using System;
using System.Collections.Generic;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	class BodyHandler
	{
		private readonly VitricBoss parent;
		private readonly VerletChain chain;
		private bool stopDrawingBody;

		public BodyHandler(VitricBoss parent)
		{
			this.parent = parent;

			var end = new Vector2(parent.arena.Center.X, parent.arena.Bottom); //WIP, dunno how this works yet - will not change anything in-game
			chain = new VerletChain(9, true, parent.NPC.Center, end, 5, Vector2.UnitY, false, null, true, new List<int>() { 100, 100, 48, 36, 36, 32, 32, 32, 32 })
			{
				drag = 1.1f
			};
		}

		public static void LoadGores()
		{
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.VitricBoss + "Gore/HeadTop");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.VitricBoss + "Gore/HeadNose");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.VitricBoss + "Gore/HeadJaw");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.VitricBoss + "Gore/HeadLeft");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.VitricBoss + "Gore/HeadRight");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.VitricBoss + "Gore/CheekLeft");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.VitricBoss + "Gore/CheekRight");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.VitricBoss + "Gore/HornLeft");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.VitricBoss + "Gore/HornRight");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.VitricBoss + "Gore/BodyTop");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.VitricBoss + "Gore/BodyBottom");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.VitricBoss + "Gore/SegmentLarge");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.VitricBoss + "Gore/SegmentMedium");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.VitricBoss + "Gore/SegmentSmall");
		}

		public void DrawBody(SpriteBatch sb)
		{
			if (Main.netMode == Terraria.ID.NetmodeID.Server)
				return;

			chain.DrawRope(sb, DrawSegment);

			if (parent.Phase == (int)VitricBoss.AIStates.FirstPhase && parent.NPC.dontTakeDamage) //draws the NPC's shield when immune and in the first phase
				chain.DrawRope(sb, DrawShield);
		}

		private void DrawSegment(SpriteBatch sb, int index, Vector2 pos)
		{
			if (Main.netMode == Terraria.ID.NetmodeID.Server)
				return;

			if (stopDrawingBody && index > 0)
				return;

			Texture2D tex = Request<Texture2D>(AssetDirectory.VitricBoss + "VitricBossBody").Value;
			Texture2D glowTex = Request<Texture2D>(AssetDirectory.VitricBoss + "VitricBossBodyGlow").Value;

			float rot = 0;

			if (index != 0)
				rot = (chain.ropeSegments[index].posNow - chain.ropeSegments[index - 1].posNow).ToRotation() - (float)Math.PI / 2;
			Rectangle source = index switch
			{
				0 => new Rectangle(0, 0, 0, 0),
				1 => new Rectangle(0, 0, 114, 114),
				2 => new Rectangle(18, 120, 78, 44),
				3 or 4 => new Rectangle(26, 168, 62, 30),
				_ => new Rectangle(40, 204, 34, 28),
			};
			int thisTimer = parent.twistTimer;
			int threshold = (int)(parent.maxTwistTimer / 8f * (index + 1)) - 1;

			bool shouldBeTwistFrame = false;
			if (Math.Abs(parent.lastTwistState - parent.twistTarget) == 2) //flip from 1 side to the other
			{
				int diff = parent.maxTwistTimer / 8 / 3;
				shouldBeTwistFrame = thisTimer < threshold - diff || thisTimer > threshold + diff;
			}

			if (Math.Abs(parent.twistTarget) == 1) //flip from front to side
				shouldBeTwistFrame = thisTimer > threshold;
			else if (parent.twistTarget == 0) //flip from side to front
				shouldBeTwistFrame = thisTimer < threshold;

			SpriteEffects flip = (
				parent.twistTarget == -1 && parent.twistTimer > threshold ||
				parent.lastTwistState == -1
				) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			if (Math.Abs(parent.lastTwistState) == Math.Abs(parent.twistTarget) && parent.lastTwistState != parent.twistTarget)
			{
				int dir = (int)flip;
				dir *= parent.twistTimer > parent.maxTwistTimer / 2 ? 1 : -1;

				flip = dir == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			}

			if (shouldBeTwistFrame)
				source.X += 114;

			if (index == 0 && parent.Phase != (int)VitricBoss.AIStates.Dying &&
				parent.Phase != (int)VitricBoss.AIStates.SpawnAnimation) //change this so the head is drawn from here later maybe? and move all visuals into this class?
			{
				parent.NPC.spriteDirection = (int)flip;

				if (parent.NPC.frame.Y == 0)
				{
					if (Math.Abs(parent.lastTwistState) == Math.Abs(parent.twistTarget) && parent.lastTwistState != parent.twistTarget)
					{
						parent.NPC.frame.X = parent.NPC.frame.Width * (2 - (int)(Math.Sin(parent.twistTimer / (float)parent.maxTwistTimer * 3.14f) * 2));
					}

					else if (shouldBeTwistFrame) //this is dumb, mostly just to test
					{
						parent.NPC.frame.X = parent.NPC.frame.Width * (int)(parent.twistTimer / (float)parent.maxTwistTimer * 2);
					}
					else
					{
						parent.NPC.frame.X = parent.NPC.frame.Width * (2 - (int)(parent.twistTimer / (float)parent.maxTwistTimer * 2));
					}
				}

				if (parent.NPC.frame.Y == parent.NPC.frame.Height * 2 || parent.NPC.frame.Y == parent.NPC.frame.Height * 3)
				{
					parent.NPC.frame.Y = parent.NPC.frame.Height * (shouldBeTwistFrame ? 2 : 3);
				}
			}

			float brightness = 0.5f + (float)Math.Sin(StarlightWorld.rottime + index);

			if (index == 1)
				brightness += 0.25f;

			sb.Draw(tex, pos - Main.screenPosition, source, Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16), rot, source.Size() / 2, 1, flip, 0);
			sb.Draw(glowTex, pos - Main.screenPosition, source, Color.White * brightness, rot, source.Size() / 2, 1, flip, 0);

			Tile tile = Framing.GetTileSafely((int)pos.X / 16, (int)pos.Y / 16);

			if (!tile.HasTile && tile.WallType == 0)
				Lighting.AddLight(pos, new Vector3(1, 0.8f, 0.2f) * brightness * 0.4f);
		}

		private void DrawShield(SpriteBatch sb, int index, Vector2 pos)
		{
			if (Main.netMode == Terraria.ID.NetmodeID.Server)
				return;

			if (index == 0)
				return;

			Texture2D tex = Request<Texture2D>(AssetDirectory.VitricBoss + "VitricBossBodyShield").Value;

			float rot = (chain.ropeSegments[index].posNow - chain.ropeSegments[index - 1].posNow).ToRotation() - (float)Math.PI / 2;
			Rectangle source = index switch
			{
				1 => new Rectangle(0, 0, 114, 114),
				2 => new Rectangle(18, 120, 78, 44),
				3 or 4 => new Rectangle(26, 168, 62, 30),
				_ => new Rectangle(40, 204, 34, 28),
			};
			int thisTimer = parent.twistTimer;
			int threshold = (int)(parent.maxTwistTimer / 8f * (index + 1)) - 1;

			bool shouldBeTwistFrame = false;
			if (Math.Abs(parent.lastTwistState - parent.twistTarget) == 2) //flip from 1 side to the other
			{
				int diff = parent.maxTwistTimer / 8 / 3;
				shouldBeTwistFrame = thisTimer < threshold - diff || thisTimer > threshold + diff;
			}

			if (Math.Abs(parent.twistTarget) == 1) //flip from front to side
				shouldBeTwistFrame = thisTimer > threshold;
			else if (parent.twistTarget == 0) //flip from side to front
				shouldBeTwistFrame = thisTimer < threshold;

			SpriteEffects flip = (
				parent.twistTarget == -1 && parent.twistTimer > threshold ||
				parent.lastTwistState == -1
				) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			if (Math.Abs(parent.lastTwistState) == Math.Abs(parent.twistTarget) && parent.lastTwistState != parent.twistTarget)
			{
				int dir = (int)flip;
				dir *= parent.twistTimer > parent.maxTwistTimer / 2 ? 1 : -1;

				flip = dir == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			}

			if (shouldBeTwistFrame)
				source.X += 114;

			Effect effect = Terraria.Graphics.Effects.Filters.Scene["MoltenForm"].GetShader().Shader;
			effect.Parameters["sampleTexture2"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/ShieldMap").Value);
			effect.Parameters["uTime"].SetValue(2 - parent.shieldShaderTimer / 120f * 2);
			effect.Parameters["sourceFrame"].SetValue(new Vector4(source.X, source.Y, source.Width, source.Height));
			effect.Parameters["texSize"].SetValue(tex.Size());

			sb.End();
			sb.Begin(default, BlendState.NonPremultiplied, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

			sb.Draw(tex, pos - Main.screenPosition, source, Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16), rot, source.Size() / 2, 1, flip, 0);

			sb.End();
			sb.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
		}

		public void UpdateBody()
		{
			chain.UpdateChain(parent.NPC.Center + parent.PainOffset);
			chain.IterateRope(UpdateBodySegment);
		}

		private void UpdateBodySegment(int index)
		{
			if (index == 1)
				chain.ropeSegments[index].posNow.Y += 20;

			if (index == 2)
				chain.ropeSegments[index].posNow.Y += 10;

			if (index == 3)
				chain.ropeSegments[index].posNow.Y += 5;

			chain.ropeSegments[index].posNow.X += (float)Math.Sin(Main.GameUpdateCount / 40f + index * 0.8f) * 0.5f;
		}

		public void SpawnGores2()
		{
			if (chain?.ropeSegments[0]?.posNow == null)
				return;

			Vector2 pos = chain.ropeSegments[0].posNow;

			GoreMe(pos, new Vector2(-60, -30), "HeadLeft");
			GoreMe(pos, new Vector2(60, -30), "HeadRight");

			GoreMe(pos, new Vector2(-60, 0), "HornLeft");
			GoreMe(pos, new Vector2(60, 0), "HornRight");

			GoreMe(pos, new Vector2(0, -15), "HeadTop");
			GoreMe(pos, new Vector2(0, 0), "HeadNose");
			GoreMe(pos, new Vector2(0, 80), "HeadJaw");

			GoreMe(pos, new Vector2(-45, 40), "CheekLeft");
			GoreMe(pos, new Vector2(45, 40), "CheekRight");
		}

		public void SpawnGores()
		{
			stopDrawingBody = true;

			if (chain?.ropeSegments == null)
				return;

			for (int k = 0; k < chain.ropeSegments.Count; k++)
			{
				Vector2 pos = chain.ropeSegments[k].posNow;

				switch (k)
				{
					case 1:
						GoreMe(pos, new Vector2(0, 0), "BodyTop");
						GoreMe(pos, new Vector2(0, 50), "BodyBottom");
						break;

					case 2:
						GoreMe(pos, new Vector2(0, 0), "SegmentLarge");
						break;

					case 3:
					case 4:
						GoreMe(pos, new Vector2(0, 0), "SegmentMedium");
						break;

					default:
						GoreMe(pos, new Vector2(0, 0), "SegmentSmall");
						break;
				}
			}
		}

		private void GoreMe(Vector2 pos, Vector2 offset, string tex)
		{
			Texture2D texture = Request<Texture2D>(AssetDirectory.VitricBoss + "Gore/" + tex).Value;
			Gore.NewGorePerfect(parent.NPC.GetSource_FromThis(), pos + offset - texture.Size() / 2, offset == Vector2.Zero ? Vector2.One.RotatedByRandom(6.28f) : Vector2.Normalize(offset) * Main.rand.NextFloat(6, 8), StarlightRiver.Instance.Find<ModGore>(tex).Type);
		}
	}
}
