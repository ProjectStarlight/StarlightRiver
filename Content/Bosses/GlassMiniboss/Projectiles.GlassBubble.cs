using ReLogic.Content;
using StarlightRiver.Content.Packets;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassBubble : ModProjectile
	{
		public const int CRACK_TIME = 850;
		public const float EXPLOSION_RAD = 300f;

		public int hits;

		private NPC Parent => Main.npc[(int)Projectile.ai[0]];
		private ref float State => ref Projectile.ai[1];
		private ref float Timer => ref Projectile.ai[2];

		/// <summary>
		/// Way of ensuring projectile scale is correct on the first frame this is created since setting later would be delayed and cost an additional packet
		/// </summary>
		public static float staticScaleToSet = 1f;

		public bool isLoaded = false;
		public override string Texture => AssetDirectory.Glassweaver + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Volatile Bubble");
		}

		public override void SetDefaults()
		{
			Projectile.width = 50;
			Projectile.height = 50;
			Projectile.hostile = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 1100; //failsafe
			Projectile.tileCollide = false;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.scale = staticScaleToSet;
			staticScaleToSet = 1f;
		}

		public override void AI()
		{
			if (!isLoaded)
			{
				Helpers.Helper.PlayPitched("GlassMiniboss/WeavingSuper", 1f, 0f, Projectile.Center);
				isLoaded = true;
			}

			Timer++;

			if (State == 0)
			{

				if (!Parent.active || Parent.type != NPCType<Glassweaver>())
					Projectile.Kill();

				Glassweaver glassweaver = Parent.ModNPC as Glassweaver;

				if (glassweaver.AttackTimer <= 240 && glassweaver.AttackTimer > 80)
				{
					Vector2 staffPos = glassweaver.NPC.Center + new Vector2(5 * glassweaver.NPC.direction, -100).RotatedBy(glassweaver.NPC.rotation);
					Projectile.Center = staffPos + Main.rand.NextVector2Circular(3, 3) * Utils.GetLerpValue(220, 120, glassweaver.AttackTimer, true);
					glassweaver.NPC.velocity *= 0.87f;
					glassweaver.NPC.velocity.Y -= 0.01f;
					CameraSystem.shake += (int)(glassweaver.AttackTimer / 180f);
				}

				if (glassweaver.AttackTimer >= 235 && glassweaver.AttackTimer <= 240) //smear 6 frames incase of a 5 frame skip in multiplayer from low end PCs
					glassweaver.moveTarget = Projectile.Center;
			}

			if (Timer < 180 && Main.rand.Next((int)Timer) < 70)
			{
				Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(200, 200);
				Vector2 vel = pos.DirectionTo(Projectile.Center).RotatedBy(MathHelper.Pi / 2.2f * Main.rand.NextFloatDirection()) * Main.rand.NextFloat(4f);
				var swirl = Dust.NewDustPerfect(pos, DustType<Dusts.Cinder>(), vel, newColor: Glassweaver.GlowDustOrange, Scale: Main.rand.NextFloat(1f, 2f));
				swirl.customData = Projectile.Center;

				Projectile.velocity = Vector2.Zero;
			}

			Projectile.tileCollide = State > 0;

			if (hits > 0)
				hits--;

			if (State == 1)
			{

				if (Main.netMode == NetmodeID.Server)
				{
					foreach (Player eachPlayer in Main.player)
					{
						if (eachPlayer.active && !eachPlayer.dead && Projectile.Hitbox.Intersects(eachPlayer.Hitbox))
							OnHitPlayer(eachPlayer, new Player.HurtInfo()); //this is normally run only by the player being hit, but this has important phase info so we want to determine hits on the server side
					}
				}

				if (Timer < CRACK_TIME - 30)
				{
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(Parent.GetTargetData().Center) * 5f, 0.004f);
				}
				else
				{
					if (Timer <= CRACK_TIME + 20)
						Projectile.velocity += Projectile.DirectionTo(Parent.GetTargetData().Center) * 0.1f;

					Projectile.velocity *= 0.97f;
					Projectile.Center += Main.rand.NextVector2Circular(5, 5) * Utils.GetLerpValue(CRACK_TIME + 30, CRACK_TIME + 100, Timer, true);
					Projectile.rotation += Main.rand.NextFloat(-0.33f, 0.33f) * Utils.GetLerpValue(CRACK_TIME + 40, CRACK_TIME + 100, Timer, true);
				}

				if (Timer == CRACK_TIME + 30)
					Helpers.Helper.PlayPitched("GlassMiniboss/GlassExplode", 1.1f, 0f, Projectile.Center);
			}
			else if (Timer > 360)
			{
				Timer = 360;
			}

			if (Timer > CRACK_TIME + 100)
				Explode();

			Projectile.rotation += Projectile.velocity.X * 0.05f;
			Projectile.rotation = MathHelper.WrapAngle(Projectile.rotation);

			var lightColor = Color.Lerp(Glassweaver.GlassColor, Glassweaver.GlowDustOrange, Utils.GetLerpValue(CRACK_TIME, CRACK_TIME + 40, Timer, true));

			if (Main.rand.NextBool(5) && Timer > 150)
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(40, 40), DustType<Dusts.Cinder>(), Vector2.Zero, 0, lightColor, 0.7f);

			Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * Utils.GetLerpValue(120, 210, Timer, true));
		}

		private void Explode()
		{
			if (Timer == CRACK_TIME + 101 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				int shardCount = Main.masterMode ? 6 : Main.expertMode ? 8 : 6;

				for (int i = 0; i < shardCount; i++)
				{
					Vector2 velocity = new Vector2(0, Main.rand.NextFloat(0.9f, 1.1f) * 3).RotatedBy(MathHelper.TwoPi / shardCount * i);
					Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, velocity.RotatedByRandom(0.1f), ProjectileType<GlassBubbleFragment>(), Projectile.damage / 2, 2f, Main.myPlayer);
				}
			}

			if (Timer <= CRACK_TIME + 105)
			{
				CameraSystem.shake += 3;

				for (int i = 0; i < 50; i++)
				{
					Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(20, 20);
					Vector2 vel = Main.rand.NextVector2Circular(15, 15);
					Dust.NewDustPerfect(pos, DustType<Dusts.Cinder>(), vel, 0, Glassweaver.GlowDustOrange, 1.3f);

					if (Main.rand.NextBool(8))
						Dust.NewDustPerfect(pos, DustType<Dusts.GlassGravity>(), Main.rand.NextVector2Circular(5, 0) - new Vector2(0, Main.rand.NextFloat(5)));
				}
			}

			if (Timer > CRACK_TIME + 130)
				Projectile.Kill();
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Timer < CRACK_TIME + 100 && State == 1)
			{
				Helpers.Helper.PlayPitched("GlassMiniboss/GlassBounce", 0.9f, 0.2f + Main.rand.NextFloat(-0.2f, 0.4f), Projectile.Center);
				CameraSystem.shake += 4;

				if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > 0)
					Projectile.velocity.X = -oldVelocity.X * 1.05f;

				if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > 0)
					Projectile.velocity.Y = -oldVelocity.Y * 1.05f;

				Projectile.velocity = Projectile.velocity.RotatedByRandom(0.1f);
				hits += 2;
			}

			if (Timer < CRACK_TIME)
				Timer = CRACK_TIME;

			return false;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			if (State == 1)
			{
				if (Main.LocalPlayer.whoAmI == target.whoAmI)
				{
					PlayerHitPacket hitPacket = new PlayerHitPacket(Projectile.identity, target.whoAmI, info.Damage, Projectile.type);
					hitPacket.Send(-1, Main.LocalPlayer.whoAmI, false);
				}

				if (Timer < CRACK_TIME)
					Timer = CRACK_TIME;

				if (hits == 0 && Timer < CRACK_TIME + 100)
				{
					Helpers.Helper.PlayPitched("GlassMiniboss/GlassBounce", 0.9f, 0.1f, Projectile.Center);
					CameraSystem.shake += 3;
					Projectile.velocity = Projectile.DirectionFrom(target.Center) * 1.77f;
					hits += 30;
				}
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return Projectile.Distance(targetHitbox.Center.ToVector2()) < Projectile.width;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Timer < CRACK_TIME + 100)
				DrawBubble(ref lightColor);

			return false;
		}

		private void DrawBubble(ref Color lightColor)
		{
			Asset<Texture2D> glassBall = Request<Texture2D>(Texture);
			Asset<Texture2D> growingBall = Request<Texture2D>(Texture + "Growing");

			int growFrameY = (int)(Utils.GetLerpValue(50, 120, Timer, true) * 5f);
			Rectangle growFrame = growingBall.Frame(1, 6, 0, growFrameY);

			//regular ball
			float fadeIn = Utils.GetLerpValue(120, 130, Timer, true);
			Main.EntitySpriteDraw(glassBall.Value, Projectile.Center - Main.screenPosition, null, Color.Lerp(lightColor, Color.White, 0.4f) * fadeIn, Projectile.rotation, glassBall.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

			//weaving
			Color hotFade = new Color(255, 255, 255, 128) * Utils.GetLerpValue(200, 130, Timer, true);
			Main.EntitySpriteDraw(growingBall.Value, Projectile.Center - Main.screenPosition, growFrame, hotFade, Projectile.rotation, growFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

			//cracking
			//Main.EntitySpriteDraw(crackingBall.Value, Projectile.Center - Main.screenPosition, crackFrame, crackFade, Projectile.rotation, crackFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
			if (Timer > CRACK_TIME - 11)
				DrawBubbleCracks();

			//DrawVignette();

			DrawBloom();
		}

		private void DrawBubbleCracks()
		{
			float crackProgress = (float)Math.Pow(Utils.GetLerpValue(CRACK_TIME - 10, CRACK_TIME + 105, Timer, true), 2);

			Effect crack = Terraria.Graphics.Effects.Filters.Scene["MagmaCracks"].GetShader().Shader;
			crack.Parameters["sampleTexture2"].SetValue(Assets.Bosses.GlassMiniboss.BubbleCrackMap.Value);
			crack.Parameters["sampleTexture3"].SetValue(Assets.Bosses.GlassMiniboss.BubbleCrackProgression.Value);
			crack.Parameters["uTime"].SetValue(crackProgress);
			crack.Parameters["drawColor"].SetValue((Color.LightGoldenrodYellow * crackProgress * 1.5f).ToVector4());
			crack.Parameters["sourceFrame"].SetValue(new Vector4(0, 0, 128, 128));
			crack.Parameters["texSize"].SetValue(Request<Texture2D>(Texture).Value.Size());

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.NonPremultiplied, Main.DefaultSamplerState, default, RasterizerState.CullNone, crack, Main.GameViewMatrix.TransformationMatrix);

			Main.EntitySpriteDraw(Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, null, Color.Black, Projectile.rotation, Request<Texture2D>(Texture).Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.AlphaBlend, Main.DefaultSamplerState, default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawBloom()
		{
			Asset<Texture2D> bloomTex = Assets.Keys.GlowAlpha;
			Asset<Texture2D> shineTex = Assets.Bosses.GlassMiniboss.BubbleBloom;

			float colLerp = Utils.GetLerpValue(150, 190, Timer, true) * Utils.GetLerpValue(CRACK_TIME + 120, CRACK_TIME, Timer, true);
			var shine = Color.Lerp(Color.PaleGoldenrod, Glassweaver.GlassColor * 0.3f, colLerp);
			shine.A = 0;
			var bloom = Color.Lerp(Color.OrangeRed, Glassweaver.GlassColor * 0.14f, colLerp);
			bloom.A = 0;
			float disappear = Utils.GetLerpValue(CRACK_TIME + 80, CRACK_TIME - 10, Timer, true);
			float appear = 0.2f + Utils.GetLerpValue(40, 110, Timer, true) * 0.78f;
			Main.EntitySpriteDraw(bloomTex.Value, Projectile.Center - Main.screenPosition, null, bloom * (0.6f + disappear), Projectile.rotation, bloomTex.Size() * 0.5f, Projectile.scale * 1.8f * appear, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(shineTex.Value, Projectile.Center - Main.screenPosition, null, shine * disappear, Projectile.rotation, shineTex.Size() * 0.5f, Projectile.scale * 0.9f * appear, SpriteEffects.None, 0);

			float warble = appear * (float)Math.Pow(Math.Sin(Math.Pow(Timer / 100f, 2.1f)), 2) * 0.5f;
			Main.EntitySpriteDraw(bloomTex.Value, Projectile.Center - Main.screenPosition, null, bloom * disappear, Projectile.rotation, bloomTex.Size() * 0.5f, Projectile.scale * 1.2f * appear + warble, SpriteEffects.None, 0);

			if (Timer > CRACK_TIME)
			{
				int shardCount = Main.masterMode ? 6 : Main.expertMode ? 8 : 6;

				for (int i = 0; i < shardCount; i++)
				{
					float rotation = MathHelper.TwoPi / shardCount * i;
					Asset<Texture2D> tell = TextureAssets.Extra[98];
					float tellLength = Helpers.Helper.BezierEase((Timer - CRACK_TIME) / 130f) * 12f;
					Color tellFade = Color.OrangeRed * ((Timer - CRACK_TIME) / 130f) * 0.5f;
					tellFade.A = 0;
					Main.EntitySpriteDraw(tell.Value, Projectile.Center - Main.screenPosition, null, tellFade, rotation, tell.Size() * new Vector2(0.5f, 0.6f), new Vector2(0.4f, tellLength), SpriteEffects.None, 0);
				}
			}
		}

		private void DrawVignette()
		{
			float fade = Utils.GetLerpValue(20, 250, Timer, true) * Utils.GetLerpValue(CRACK_TIME + 105, CRACK_TIME + 90, Timer, true);
			Asset<Texture2D> dark = Assets.Misc.GradientBlack;

			for (int i = 0; i < 8; i++)
			{
				float rotation = MathHelper.TwoPi / 8 * i;
				Vector2 pos = Projectile.Center + new Vector2(90, 0).RotatedBy(rotation);
				Main.EntitySpriteDraw(dark.Value, pos - Main.screenPosition, null, Color.Black * 0.2f * fade, rotation, new Vector2(dark.Width() * 0.5f, 0), 12, 0, 0);
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(Projectile.scale);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			Projectile.scale = reader.ReadSingle();
		}
	}

	class GlassBubbleFragment : ModProjectile
	{

		public ref float Timer => ref Projectile.ai[0];
		public ref float Variant => ref Projectile.ai[1];

		public override string Texture => AssetDirectory.Glassweaver + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Glass Shard");
		}

		public override void SetDefaults()
		{
			Projectile.width = 15;
			Projectile.height = 15;
			Projectile.hostile = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 540;
			Projectile.tileCollide = true;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Variant = Main.rand.Next(3);
		}

		public override void AI()
		{
			if (Projectile.velocity.Length() > 0.1f)
				Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

			Timer++;

			if (Projectile.tileCollide == true)
			{
				if (Timer > 20)
					Projectile.velocity *= 1.09f;
			}
			else
			{
				Projectile.velocity *= 0.2f;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (Projectile.timeLeft > 5)
				return Projectile.Distance(targetHitbox.Center.ToVector2()) < 24;
			else
				return Projectile.Distance(targetHitbox.Center.ToVector2()) < 50;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.tileCollide = false;
			Projectile.timeLeft = 10;

			Helpers.Helper.PlayPitched("GlassMiniboss/GlassShatter", 1f, 0.2f, Projectile.Center);

			for (int i = 0; i < 30; i++)
				Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(2, 2), 0, Color.DarkOrange, Main.rand.NextFloat(0.5f));

			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Asset<Texture2D> fragment = Request<Texture2D>(Texture);
			Rectangle fragFrame = fragment.Frame(4, 2, (int)Variant, 0);
			Rectangle hotFrame = fragment.Frame(4, 2, (int)Variant, 1);

			Main.EntitySpriteDraw(fragment.Value, Projectile.Center - Main.screenPosition, fragFrame, lightColor, Projectile.rotation, fragFrame.Size() * 0.5f, Projectile.scale, 0, 0);

			var hotFade = new Color(255, 255, 255, 128);
			Main.EntitySpriteDraw(fragment.Value, Projectile.Center - Main.screenPosition, hotFrame, hotFade, Projectile.rotation, hotFrame.Size() * 0.5f, Projectile.scale, 0, 0);

			Asset<Texture2D> fragGlow = Assets.Keys.GlowAlpha;
			Color glowFade = Color.OrangeRed;
			glowFade.A = 0;
			Main.EntitySpriteDraw(fragGlow.Value, Projectile.Center - Main.screenPosition, null, glowFade, Projectile.rotation, fragGlow.Size() * 0.5f, Projectile.scale, 0, 0);

			return false;
		}
	}
}