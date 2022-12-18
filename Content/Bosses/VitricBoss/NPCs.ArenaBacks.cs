using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	public class VitricBackdropLeft : ModNPC
	{
		public const int Scrolltime = 1000;
		public const int Risetime = 360;
		public List<NPC> platforms = new();

		public int shake = 0;
		private float prevState = 0;

		protected ref float Timer => ref NPC.ai[0];
		protected ref float State => ref NPC.ai[1];
		protected ref float ScrollTimer => ref NPC.ai[2];
		protected ref float ScrollDelay => ref NPC.ai[3];

		public override string Texture => AssetDirectory.Invisible;

		protected virtual int PlatformCount => 8;

		public override bool CheckActive()
		{
			return false;
		}

		public override bool? CanBeHitByProjectile(Projectile Projectile)
		{
			return false;
		}

		public override bool? CanBeHitByItem(Player Player, Item Item)
		{
			return false;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("");
		}

		public override void SetDefaults()
		{
			NPC.height = 1;
			NPC.width = 560;
			NPC.aiStyle = -1;
			NPC.lifeMax = 10;
			NPC.knockBackResist = 0f;
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.dontTakeDamage = true;
			NPC.dontCountMe = true;
			NPC.netAlways = true;
			NPC.hide = true;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			if (State != 0 && State != 1)
			{
				for (int i = 0; i < PlatformCount; i++)
				{
					writer.Write((byte)platforms[i].whoAmI);
				}
			}
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			if (State != 0 && State != 1)
			{
				for (int i = 0; i < PlatformCount; i++)
				{
					if (i >= platforms.Count)
						platforms.Add(Main.npc[reader.ReadByte()]);
					else
						platforms[i] = Main.npc[reader.ReadByte()];
				}
			}
		}

		public override void AI()
		{
			/* AI fields:
             * 0: timer
             * 1: activation state, im too lazy to create an enum for this so: (0 = hidden, 1 = rising, 2 = still, 3 = scrolling, 4 = resetting)
             * 2: scrolling timer
             * 3: scroll acceleration
             */

			if (StarlightWorld.HasFlag(WorldFlags.VitricBossOpen) && State == 0)
				State = 1; //when the altar is hit, make the BG rise out of the ground

			if (State == 1)
			{
				Timer++;

				if (Main.netMode != NetmodeID.MultiplayerClient)
					SpawnPlatforms();

				ScrollDelay = 20; //initial acceleration delay

				if (Timer == Risetime - 1) //hitting the top
				{
					CameraSystem.shake += 18;
					Helper.PlayPitched("ArenaHit", 0.2f, 0, NPC.Center);
				}

				if (Timer > Risetime)
					State = 2;

				if (Timer % 10 == 0)
					CameraSystem.shake += Timer < 100 ? 3 : 2;

				for (int k = 0; k < 18; k++)
					Dust.NewDust(NPC.position, 560, 1, DustType<Dusts.Sand>(), 0, Main.rand.NextFloat(-5f, -1f), Main.rand.Next(255), default, Main.rand.NextFloat(1.5f)); //spawns dust
			}

			if (State == 2)
			{
				Timer = Risetime;

				foreach (NPC NPC in Main.npc.Where(n => n.ModNPC is VitricBossPlatformUp))
				{
					NPC.ai[0] = 0;
					NPC.ai[1] = 0;
				}

				ResyncPlatforms();
			}

			if (State == 3) //scrolling
			{
				Timer++;

				foreach (NPC NPC in Main.npc.Where(n => n.ModNPC is VitricBossPlatformUp))
				{
					NPC.ai[0] = 1;
				}

				if (Timer <= Risetime + 120) //when starting moving
					shake = (int)(Helper.BezierEase(120 - (Timer - Risetime)) * 5); //this should work?

				if (Timer == Risetime + 120)
				{
					for (int k = 0; k < 200; k++)
					{
						Dust.NewDust(NPC.position, 560, 1, DustType<Dusts.Sand>(), 0, Main.rand.NextFloat(-5f, -1f), Main.rand.Next(255), default, Main.rand.NextFloat(1.5f)); //spawns dust
						Dust.NewDust(NPC.position + new Vector2(0, -55 * 16), 560, 1, DustType<Dusts.Sand>(), 0, Main.rand.NextFloat(-5f, -1f), Main.rand.Next(255), default, Main.rand.NextFloat(1.5f)); //spawns dust
					}
				}

				if (Timer % ScrollDelay == 0)
				{
					ScrollTimer++;

					if (ScrollDelay > 1)
						ScrollDelay--;
				}
			}

			if (ScrollTimer > Scrolltime)
			{
				ScrollTimer = 0;

				ResyncPlatforms();
			}

			if (State == 4)
			{
				if (ScrollTimer != 0)
				{
					ScrollTimer++; //stops once we're reset.
				}
				else
				{
					foreach (NPC NPC in Main.npc.Where(n => n.ModNPC is VitricBossPlatformUp))
					{
						NPC.ai[0] = 0;
					}

					ResyncPlatforms();

					State = 2;
					ScrollDelay = 20; //reset acceleration delay
				}
			}

			if ((prevState != State || State == 3 && Timer % 60 == 0) && Main.netMode == NetmodeID.Server)
			{
				NPC.netUpdate = true;
				prevState = State;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (!NPC.active)
				return false;

			if (State == 3 || State == 4)
				ScrollDraw(spriteBatch);
			else  //animation for rising out of the sand
				MainDraw(spriteBatch);

			return false;
		}

		public override void DrawBehind(int index)
		{
			Main.instance.DrawCacheNPCsMoonMoon.Add(index);
		}

		public virtual void MainDraw(SpriteBatch sb)
		{
			string path = AssetDirectory.VitricBoss + Name;
			Texture2D tex = Request<Texture2D>(path).Value;
			Texture2D tex2 = Request<Texture2D>(path + "Top").Value;
			Texture2D tex3 = Request<Texture2D>(path + "Side").Value;
			int targetHeight = (int)(Timer / Risetime * tex.Height);

			if (State >= 3) //ignore timer after rising is done
				targetHeight = tex.Height;

			const int yOffset = 3; // Fit perfectly in the gap

			int xPos = (int)(NPC.position.X - Main.screenPosition.X);
			int yPos = (int)(NPC.position.Y - targetHeight - Main.screenPosition.Y) - yOffset;

			var target = new Rectangle(xPos, yPos, tex.Width, targetHeight);
			var source = new Rectangle(0, 0, tex.Width, targetHeight);

			var target2 = new Rectangle(xPos - tex3.Width, yPos, tex3.Width, targetHeight);
			var source2 = new Rectangle(0, 0, tex3.Width, targetHeight);

			Core.Systems.LightingSystem.LightingBufferRenderer.DrawWithLighting(target, tex, source, default);
			Core.Systems.LightingSystem.LightingBufferRenderer.DrawWithLighting(target2, tex3, source2, default);
			Core.Systems.LightingSystem.LightingBufferRenderer.DrawWithLighting(target.TopLeft() - Vector2.UnitY * 56, tex2, tex2.Bounds, default);
		}

		public virtual void ScrollDraw(SpriteBatch sb) //im lazy
		{
			string path = AssetDirectory.VitricBoss + Name;
			Texture2D tex = Request<Texture2D>(path).Value;
			int height1 = (int)(ScrollTimer / Scrolltime * tex.Height);
			int height2 = tex.Height - height1;
			//Color color = new Color(180, 225, 255);
			Vector2 off = Vector2.One.RotatedByRandom(6.28f) * shake;

			var target1 = new Rectangle((int)(NPC.position.X - Main.screenPosition.X + off.X), (int)(NPC.position.Y - height1 - Main.screenPosition.Y + off.Y), tex.Width, height1);
			var target2 = new Rectangle((int)(NPC.position.X - Main.screenPosition.X + off.X), (int)(NPC.position.Y - height1 - height2 - Main.screenPosition.Y + off.Y), tex.Width, height2);
			var source1 = new Rectangle(0, 0, tex.Width, height1);
			var source2 = new Rectangle(0, tex.Height - height2, tex.Width, height2);

			Core.Systems.LightingSystem.LightingBufferRenderer.DrawWithLighting(target1, tex, source1, default);
			Core.Systems.LightingSystem.LightingBufferRenderer.DrawWithLighting(target2, tex, source2, default);

			Texture2D tex2 = Request<Texture2D>(path + "Glow").Value;
			sb.Draw(tex2, target1, source1, Color.White * (0.5f + (float)System.Math.Sin(StarlightWorld.visualTimer) * 0.1f), 0, Vector2.Zero, 0, 0);
			sb.Draw(tex2, target2, source2, Color.White * (0.5f + (float)System.Math.Sin(StarlightWorld.visualTimer) * 0.1f), 0, Vector2.Zero, 0, 0);
		}

		public virtual void SpawnPlatforms(bool rising = true)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			PlacePlatform(260, 790, NPCType<VitricBossPlatformUpSmall>(), rising);
			PlacePlatform(440, 668, NPCType<VitricBossPlatformUp>(), rising);
			PlacePlatform(230, 570, NPCType<VitricBossPlatformUpSmall>(), rising);
			PlacePlatform(140, 420, NPCType<VitricBossPlatformUp>(), rising);
			PlacePlatform(280, 310, NPCType<VitricBossPlatformUpSmall>(), rising);
			PlacePlatform(400, 230, NPCType<VitricBossPlatformUpSmall>(), rising);
			PlacePlatform(205, 136, NPCType<VitricBossPlatformUp>(), rising);
			PlacePlatform(210, 30, NPCType<VitricBossPlatformUpSmall>(), rising);
		}

		public virtual void ResyncPlatforms()
		{
			if (platforms.Count != PlatformCount)
				return;

			SyncPlatform(platforms[0], 790);
			SyncPlatform(platforms[1], 668);
			SyncPlatform(platforms[2], 570);
			SyncPlatform(platforms[3], 420);
			SyncPlatform(platforms[4], 310);
			SyncPlatform(platforms[5], 230);
			SyncPlatform(platforms[6], 136);
			SyncPlatform(platforms[7], 30);
		}

		public void PlacePlatform(int x, int y, int type, bool rising)
		{
			if (rising && Timer == Risetime - (int)(y / 880f * Risetime))
			{
				int i = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.position.X + x, (int)NPC.position.Y - 2, type, 0, 0, Risetime - Timer); //When rising out of the ground, check for the appropriate time to spawn the platform based on y coord
				if (Main.npc[i].type == type)
					(Main.npc[i].ModNPC as VitricBossPlatformUp).parent = this;

				platforms.Add(Main.npc[i]);
			}
			else if (!rising)
			{
				int i = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.position.X + x, (int)NPC.position.Y - y, type, 0, 2, Risetime); //otherwise spawn it instantly AT the y coord
				if (Main.npc[i].type == type)
					(Main.npc[i].ModNPC as VitricBossPlatformUp).parent = this;

				platforms.Add(Main.npc[i]);
			}
		}

		public void SyncPlatform(NPC platform, int y)
		{
			platform.position.Y = (int)NPC.position.Y - y - platform.height;
		}
	}

	public class VitricBackdropRight : VitricBackdropLeft //im lazy
	{
		/*public override void MainDraw(SpriteBatch sb)
        {
            string path = AssetDirectory.VitricBoss + Name;
            Texture2D tex = Request<Texture2D>(path).Value;
            Texture2D tex2 = Request<Texture2D>(path + "Top").Value;
            int targetHeight = (int)(Timer / Risetime * tex.Height);

            if (State >= 3) //ignore timer after rising is done
                targetHeight = tex.Height;

            const int yOffset = 3; // Fit perfectly in the gap

            Rectangle target = new Rectangle(
                (int)(NPC.position.X - Main.screenPosition.X),
                (int)(NPC.position.Y - targetHeight - Main.screenPosition.Y) - yOffset,
                tex.Width,
                targetHeight);

            Rectangle source = new Rectangle(0, 0, tex.Width, targetHeight);

            Helpers.LightingBufferRenderer.DrawWithLighting(target, tex, source, default);
            Helpers.LightingBufferRenderer.DrawWithLighting(target.TopLeft() - Vector2.UnitY * 56, tex2, tex2.Bounds, default);
        }*/

		public override void MainDraw(SpriteBatch sb)
		{
			string path = AssetDirectory.VitricBoss + Name;
			Texture2D tex = Request<Texture2D>(path).Value;
			Texture2D tex2 = Request<Texture2D>(path + "Top").Value;
			Texture2D tex3 = Request<Texture2D>(path + "Side").Value;
			int targetHeight = (int)(Timer / Risetime * tex.Height);

			if (State >= 3) //ignore timer after rising is done
				targetHeight = tex.Height;

			const int yOffset = 3; // Fit perfectly in the gap

			int xPos = (int)(NPC.position.X - Main.screenPosition.X);
			int yPos = (int)(NPC.position.Y - targetHeight - Main.screenPosition.Y) - yOffset;

			var target = new Rectangle(xPos, yPos, tex.Width, targetHeight);
			var source = new Rectangle(0, 0, tex.Width, targetHeight);

			var target2 = new Rectangle(xPos + tex.Width, yPos, tex3.Width, targetHeight);
			var source2 = new Rectangle(0, 0, tex3.Width, targetHeight);

			Core.Systems.LightingSystem.LightingBufferRenderer.DrawWithLighting(target, tex, source, default);
			Core.Systems.LightingSystem.LightingBufferRenderer.DrawWithLighting(target2, tex3, source2, default);
			Core.Systems.LightingSystem.LightingBufferRenderer.DrawWithLighting(target.TopLeft() - Vector2.UnitY * 56, tex2, tex2.Bounds, default);

			if (Holidays.AnySpecialEvent)//1 in 32 or any special date event
			{
				Texture2D egg = Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/VitricRightEasterEgg").Value;
				Core.Systems.LightingSystem.LightingBufferRenderer.DrawWithLighting(target, egg, source);
			}
		}

		public override void ScrollDraw(SpriteBatch sb)
		{
			string path = AssetDirectory.VitricBoss + Name;
			Texture2D tex = Request<Texture2D>(path).Value;
			int height1 = (int)(ScrollTimer / Scrolltime * tex.Height);
			int height2 = tex.Height - height1;
			//Color color = new Color(180, 225, 255);
			Vector2 off = Vector2.One.RotatedByRandom(6.28f) * shake;

			var target1 = new Rectangle((int)(NPC.position.X - Main.screenPosition.X + off.X), (int)(NPC.position.Y - tex.Height * 2 + height1 + height2 - Main.screenPosition.Y + off.Y), tex.Width, height1);
			var target2 = new Rectangle((int)(NPC.position.X - Main.screenPosition.X + off.X), (int)(NPC.position.Y - tex.Height + height1 - Main.screenPosition.Y + off.Y), tex.Width, height2);
			var source2 = new Rectangle(0, 0, tex.Width, height2);
			var source1 = new Rectangle(0, tex.Height - height1, tex.Width, height1);

			Core.Systems.LightingSystem.LightingBufferRenderer.DrawWithLighting(target1, tex, source1, default);
			Core.Systems.LightingSystem.LightingBufferRenderer.DrawWithLighting(target2, tex, source2, default);

			Texture2D tex2 = Request<Texture2D>(path + "Glow").Value;
			sb.Draw(tex2, target1, source1, Color.White * (0.5f + (float)System.Math.Sin(StarlightWorld.visualTimer) * 0.1f), 0, Vector2.Zero, 0, 0);
			sb.Draw(tex2, target2, source2, Color.White * (0.5f + (float)System.Math.Sin(StarlightWorld.visualTimer) * 0.1f), 0, Vector2.Zero, 0, 0);

			if (Holidays.AnySpecialEvent)//1 in 32 or any special date event
			{
				Texture2D egg = ModContent.Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/VitricRightEasterEgg").Value;
				Core.Systems.LightingSystem.LightingBufferRenderer.DrawWithLighting(target1, egg, source1);
				Core.Systems.LightingSystem.LightingBufferRenderer.DrawWithLighting(target2, egg, source2);
			}
		}

		protected override int PlatformCount => 7;

		public override void SpawnPlatforms(bool rising = true)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			PlacePlatform(294, 760, NPCType<VitricBossPlatformDownSmall>(), rising);
			PlacePlatform(424, 660, NPCType<VitricBossPlatformDownSmall>(), rising);
			PlacePlatform(192, 580, NPCType<VitricBossPlatformDown>(), rising);
			PlacePlatform(94, 440, NPCType<VitricBossPlatformDownSmall>(), rising);
			PlacePlatform(272, 330, NPCType<VitricBossPlatformDown>(), rising);
			PlacePlatform(394, 198, NPCType<VitricBossPlatformDownSmall>(), rising);
			PlacePlatform(160, 90, NPCType<VitricBossPlatformDown>(), rising);
		}

		public override void ResyncPlatforms()
		{
			if (platforms.Count != PlatformCount)
				return;

			SyncPlatform(platforms[0], 760);
			SyncPlatform(platforms[1], 660);
			SyncPlatform(platforms[2], 580);
			SyncPlatform(platforms[3], 440);
			SyncPlatform(platforms[4], 330);
			SyncPlatform(platforms[5], 198);
			SyncPlatform(platforms[6], 90);
		}
	}
}