﻿using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Systems.BarrierSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader.Utilities;

namespace StarlightRiver.Content.NPCs.Dungeon
{
	internal class CrescentCaster : ModNPC, IDrawPrimitive
	{
		private const float ACCELERATION = 0.15f;
		private const float MAXSPEED = 2;

		private readonly int aiCounterReset = 800;

		private int xFrame = 0;
		private int yFrame = 0;

		private float starOpacity = 0;

		private ref float Timer => ref NPC.ai[0];

		private List<NPC> supportTargets = new();

		private readonly List<CrescentCasterBolt> Bolts = Main.netMode == NetmodeID.Server ? null : new();

		private Player Target => Main.player[NPC.target];

		private bool Casting => Timer % aiCounterReset > 400;

		private bool Supporting => xFrame == 2;

		public override string Texture => AssetDirectory.DungeonNPC + Name;

		public override void Load()
		{
			for (int j = 1; j <= 5; j++)
				GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.DungeonNPC + "CrescentCasterGore" + j);
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crescent Caster");
			Main.npcFrameCount[NPC.type] = 10;

			var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0);
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults()
		{
			NPC.width = 30;
			NPC.height = 48;
			NPC.damage = 0;
			NPC.defense = 5;
			NPC.lifeMax = 200;
			NPC.value = 500f;
			NPC.knockBackResist = 0.6f;
			NPC.HitSound = SoundID.NPCHit2;
			NPC.DeathSound = SoundID.NPCDeath2;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheDungeon,
				new FlavorTextBestiaryInfoElement("As the first in a long line of dead wizards, this one has had ample time to practice magic. Using its crescent staff, this fiend generates Barrier for all nearby foes - but is immobile while doing so.")
			});
		}

		public override void AI()
		{
			NPC.TargetClosest(true);
			Timer++;

			if (Casting)
				CastBehavior();
			else
				WalkBehavior();
		}

		private void CastBehavior()
		{
			List<NPC> tempTargets = ValidTargets(); //purpose of temptargets is to check npcs who were being supported but are no longer
			var toReduceBarrier = Main.npc.Where(n => n.active && n.type != NPCID.None && !tempTargets.Contains(n) && supportTargets.Contains(n)).ToList();

			ClearBarrierAndBolts(toReduceBarrier);

			var toAddBolts = Main.npc.Where(n => n.active && n.type != NPCID.None && tempTargets.Contains(n) && !supportTargets.Contains(n)).ToList();

			foreach (NPC boltNPC in toAddBolts)
			{
				BarrierNPC barrierNPC = boltNPC.GetGlobalNPC<BarrierNPC>();
				barrierNPC.maxBarrier += 250;
				CreateBolt(boltNPC);
			}

			supportTargets = tempTargets;

			UpdateBolts();

			if (supportTargets.Count == 0)
			{
				Timer = 390;
				WalkBehavior();
				return;
			}

			switch (xFrame)
			{
				case 0: //first frame of casting
					xFrame = 1;
					NPC.frameCounter = 0;
					yFrame = 0;
					break;

				case 1: //Winding up to cast
					NPC.frameCounter++;

					if (NPC.frameCounter % 4 == 0)
						yFrame++;

					if (yFrame == 10)
					{
						yFrame = 0;
						xFrame = 2;
						NPC.frameCounter = 0;
					}

					break;

				case 2: //Supporting
					SupportBehavior();
					break;
			}
		}

		private void SupportBehavior()
		{
			NPC.frameCounter++;

			if (NPC.frameCounter % 4 == 0)
			{
				yFrame++;
				yFrame %= 5;
			}

			if (Timer % aiCounterReset < aiCounterReset - 10)
			{
				if (starOpacity < 1)
					starOpacity += 0.1f;
			}
			else if (starOpacity > 0)
			{
				starOpacity -= 0.1f;
			}

			foreach (NPC supportTarget in supportTargets)
			{
				BarrierNPC barrierNPC = supportTarget.GetGlobalNPC<BarrierNPC>();

				barrierNPC.rechargeRate = 90;
				barrierNPC.rechargeDelay = 60;
			}
		}

		private void WalkBehavior()
		{
			ClearBarrierAndBolts(supportTargets);
			supportTargets.Clear();
			xFrame = 0;

			int direction;
			List<NPC> positionTargets = ValidTargets();

			if (positionTargets.Count > 0)
			{
				float xPositionToBe = Helper.Centeroid(positionTargets).X; //Calculate middle of valid enemies

				if (Math.Abs(xPositionToBe - NPC.Center.X) > 20)
					direction = Math.Sign(xPositionToBe - NPC.Center.X);
				else
					direction = 0;
			}
			else
			{
				direction = Math.Sign(NPC.Center.X - Target.Center.X);
			}

			if (direction != 0)
				NPC.direction = NPC.spriteDirection = direction;

			NPC.velocity.X += direction * ACCELERATION;
			NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -MAXSPEED, MAXSPEED);

			if (NPC.collideX && NPC.velocity.Y == 0)
				NPC.velocity.Y = -6;

			NPC.frameCounter++;

			if (direction != 0)
			{
				if (NPC.velocity.Y == 0)
				{
					if (NPC.frameCounter % 4 == 0)
					{
						yFrame++;
						yFrame %= 8;
					}
				}
				else
				{
					yFrame = 7;
				}
			}
			else
			{
				NPC.direction = NPC.spriteDirection = Math.Sign(Target.Center.X - NPC.Center.X);
				yFrame = 0;
			}
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			return SpawnCondition.Dungeon.Chance * 0.17f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Dungeon.InertStaff>(), 20));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Magnet.UnchargedMagnet>(), 1));
		}

		public override void FindFrame(int frameHeight)
		{
			int frameWidth = 70;
			NPC.frame = new Rectangle(frameWidth * xFrame, frameHeight * yFrame, frameWidth, frameHeight);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D starTex = ModContent.Request<Texture2D>(Texture + "_Star").Value;

			var origin = new Vector2(NPC.frame.Width / 2, 52);
			SpriteEffects effects = SpriteEffects.FlipHorizontally;

			if (NPC.spriteDirection != 1)
			{
				effects = SpriteEffects.None;
				origin.X = NPC.frame.Width - origin.X;
			}

			spriteBatch.Draw(tex, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);
			spriteBatch.Draw(glowTex, NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, origin, NPC.scale, effects, 0f);

			if (Supporting)
			{
				float starSize = Main.rand.NextFloat(0.9f, 1.1f);
				Vector2 starPos = NPC.Center + new Vector2(NPC.spriteDirection * 3, -25);
				Color starColor = new Color(200, 230, 255, 0) * starOpacity;
				spriteBatch.Draw(starTex, starPos - Main.screenPosition, null, starColor, 0, starTex.Size() / 2, NPC.scale * 0.3f * starSize, SpriteEffects.None, 0f);

				starColor = new Color(255, 255, 255, 0) * starOpacity;
				spriteBatch.Draw(starTex, starPos - Main.screenPosition, null, starColor, 0, starTex.Size() / 2, NPC.scale * 0.2f * starSize, SpriteEffects.None, 0f);
			}

			return false;
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (NPC.life <= 0)
			{
				ClearBarrierAndBolts(supportTargets);

				if (Main.netMode != NetmodeID.Server)
				{
					for (int j = 1; j <= 5; j++)
						Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("CrescentCasterGore" + j).Type);
				}
			}
		}

		public void DrawPrimitives()
		{
			Effect effect = Terraria.Graphics.Effects.Filters.Scene["LightningTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);

			foreach (CrescentCasterBolt bolt in Bolts)
			{
				bolt.Render(effect);
			}
		}

		private void UpdateBolts()
		{
			if (Main.netMode == NetmodeID.Server)
				return;

			foreach (CrescentCasterBolt bolt in Bolts)
			{
				bolt.resetCounter += bolt.resetCounterIncrement;
				bolt.midPointDirection *= 1.05f;

				bolt.midPoint += bolt.midPointDirection;

				bolt.UpdateTrailPoints();

				if (Timer % aiCounterReset < aiCounterReset - 10 && xFrame == 2)
				{
					if (bolt.fade < 1)
						bolt.fade += 0.1f;
				}
				else if (bolt.fade > 0)
				{
					bolt.fade -= 0.1f;
				}

				foreach (Vector2 point in bolt.BezierPoints())
					Lighting.AddLight(point, Color.Cyan.ToVector3() * 0.3f * bolt.fade);

			}

			foreach (CrescentCasterBolt bolt in Bolts.ToArray())
			{
				if (bolt.resetCounter > 20 && !bolt.cloneCreated) //Change bolt curve
				{
					bolt.cloneCreated = true;
					CreateBolt(bolt.targetNPC);
				}

				if (bolt.resetCounter > 30 || !IsValidTarget(bolt.targetNPC))
					Bolts.Remove(bolt);
			}
		}

		private void CreateBolt(NPC other)
		{
			if (Main.netMode == NetmodeID.Server)
				return;

			Vector2 midPoint = CalculateMidpoint(other);
			Bolts.Add(new CrescentCasterBolt(other, NPC, midPoint, Main.rand.NextFloat(2.5f) * NPC.DirectionTo(midPoint)));
		}

		private void ClearBarrierAndBolts(List<NPC> npcs)
		{
			foreach (NPC npc in npcs)
			{
				BarrierNPC clearBarrierNPC = npc.GetGlobalNPC<BarrierNPC>();
				clearBarrierNPC.maxBarrier = 0;
				clearBarrierNPC.rechargeRate = 0;
				clearBarrierNPC.rechargeDelay = 180;
			}

			if (Main.netMode == NetmodeID.Server)
				return;

			foreach (CrescentCasterBolt bolt in Bolts.ToArray())
			{
				if (npcs.Contains(bolt.targetNPC))
					Bolts.Remove(bolt);
			}
		}

		private List<NPC> ValidTargets()
		{
			return Main.npc.Where(n => n.active && n.type != NPCID.None && IsValidTarget(n)).ToList();
		}

		private bool IsValidTarget(NPC potentialTarget)
		{
			return potentialTarget.active && !potentialTarget.friendly && potentialTarget.CanBeChasedBy() && potentialTarget.Distance(NPC.Center) < 500 && potentialTarget != NPC && ClearPath(NPC.Center, potentialTarget.Center);
		}

		private Vector2 CalculateMidpoint(NPC other)
		{
			if (other.Center == NPC.Center)
				return NPC.Center;

			Vector2 directionTo = NPC.DirectionTo(other.Center);
			return NPC.Center + directionTo.RotatedBy(-Math.Sign(directionTo.X) * Main.rand.NextFloat(0.5f, 1f)) * Main.rand.NextFloat(1f, 1.5f) * NPC.Distance(other.Center);
		}

		private static bool ClearPath(Vector2 pos1, Vector2 pos2)
		{
			Vector2 direction = pos2 - pos1;
			int length = (int)(direction.Length() / 16);
			direction.Normalize();

			Vector2 tilePos1 = pos1 / 16;

			for (int i = 0; i < length; i++)
			{
				tilePos1 += direction;
				Tile tile = Main.tile[(int)tilePos1.X, (int)tilePos1.Y];

				if (tile.HasTile && !TileID.Sets.Platforms[tile.TileType] && Main.tileSolid[tile.TileType])
					return false;
			}

			return true;
		}
	}

	public class CrescentCasterBolt
	{
		public NPC targetNPC;
		public NPC owner;

		public Vector2 midPoint;
		public Vector2 midPointDirection;

		public Trail trail;
		public Trail trail2;

		public float fade;

		public float resetCounter = 0;
		public float resetCounterIncrement;

		public bool cloneCreated = false;

		public Color baseColor = new(200, 230, 255);
		public Color endColor = Color.Purple;

		public float DistanceFade => 1 - resetCounter / 30f;

		public CrescentCasterBolt(NPC targetNPC, NPC owner, Vector2 midPoint, Vector2 midPointDirection)
		{
			this.targetNPC = targetNPC;
			this.owner = owner;
			this.midPoint = midPoint;
			this.midPointDirection = midPointDirection;
			resetCounterIncrement = Main.rand.NextFloat(0.85f, 1.15f);

			if (!Main.dedServ)
			{
				GraphicsDevice device = Main.graphics.GraphicsDevice;

				trail = new Trail(device, 15, new NoTip(), factor => 16, factor =>
				{
					if (factor.X > 0.99f)
						return Color.Transparent;

					return new Color(160, 220, 255) * fade * 0.1f * EaseFunction.EaseCubicOut.Ease(1 - factor.X) * DistanceFade;
				});

				trail2 = new Trail(device, 15, new NoTip(), factor => 3 * Main.rand.NextFloat(0.55f, 1.45f), factor =>
				{
					float progress = EaseFunction.EaseCubicOut.Ease(1 - factor.X);
					return Color.Lerp(baseColor, endColor, EaseFunction.EaseCubicIn.Ease(1 - progress)) * fade * progress * DistanceFade;
				});

				UpdateTrailPoints();
			}

			fade = 0;
		}

		public void UpdateTrailPoints()
		{
			List<Vector2> points = BezierPoints();

			var pointsWithOffset = new List<Vector2>();

			int index = 0;

			foreach (Vector2 point in points)
			{
				float offsetAmount;

				if (index == 0)
					offsetAmount = 0;
				else
					offsetAmount = (point - points[index - 1]).Length() * 0.5f * (1.125f - Math.Abs(7 - index) / 7f);

				index++;
				pointsWithOffset.Add(point + Main.rand.NextVector2Circular(offsetAmount, offsetAmount));
			}

			trail.Positions = trail2.Positions = pointsWithOffset.ToArray();
			trail.NextPosition = trail2.NextPosition = targetNPC.Center;
		}

		public List<Vector2> BezierPoints()
		{
			Vector2 startPoint = owner.Center + new Vector2(owner.spriteDirection * 3, -25);
			var curve = new BezierCurve(startPoint, midPoint, targetNPC.Center);
			return curve.GetPoints(15);
		}

		public void Render(Effect effect)
		{
			trail?.Render(effect);
			trail2?.Render(effect);
		}
	}
}