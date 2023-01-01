using StarlightRiver.Content.Physics;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
	class BalloonGun : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Balloon Gun");
			Tooltip.SetDefault("Attach balloons to enemies to make them float away");
		}

		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 64;
			Item.useTime = 50;
			Item.useAnimation = 50;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.knockBack = 0;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.channel = true;
			Item.shoot = ProjectileType<BalloonGunBalloon>();
			Item.shootSpeed = 10f;
			Item.autoReuse = true;
			Item.useTurn = false;
			Item.UseSound = SoundID.Item102;
		}
	}

	public class BalloonGunBalloon : ModProjectile
	{
		public const int NUM_SEGMENTS = 15;

		private Trail trail;
		private List<Vector2> cache = new();

		int textureNumber = 0;

		float floatRotation = 0;

		bool attached = false;

		NPC attachTarget = default;

		public VerletChain Chain;

		Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Balloon");
		}

		public override void SetDefaults()
		{
			Projectile.hostile = false;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 999;
			Projectile.ignoreWater = true;
			Projectile.hide = true;
			textureNumber = Main.rand.Next(1, 8);
			floatRotation = Main.rand.NextFloat(-0.4f, 0.4f);
		}

		public override void OnSpawn(IEntitySource source)
		{
			Chain = new VerletChain(NUM_SEGMENTS, true, Projectile.Center, 6, true)
			{
				forceGravity = new Vector2(0, -0.01f),
				simStartOffset = 0,
				constraintRepetitions = 6
			};
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCs.Add(index);
		}

		public override void AI()
		{
			if (!attached)
			{
				if (Projectile.velocity.Y > -1)
					Projectile.velocity.Y -= 0.05f;

				NPC target = Main.npc.Where(n => n.active && n.Hitbox.Intersects(Projectile.Hitbox) && n.knockBackResist != 0).FirstOrDefault();

				if (target != default)
				{
					attached = true;
					attachTarget = target;
					attachTarget.GetGlobalNPC<BalloonGunGNPC>().balloonsAttached++;
				}
			}
			else
			{
				if (!attachTarget.active)
				{
					attached = false;
					Projectile.Kill();
					return;
				}

				Projectile.Center = attachTarget.Center;
			}

			Chain.UpdateChain();
			Chain.startPoint = Projectile.Center;

			for (int i = 0; i < NUM_SEGMENTS; i++)
			{
				if (attached)
				{
					float lerper = (float)i / (NUM_SEGMENTS - 2);
					var posToBe = Vector2.Lerp(Chain.startPoint, Chain.startPoint - new Vector2(0, 100).RotatedBy(floatRotation), lerper);
					Chain.ropeSegments[i].posNow = Vector2.Lerp(Chain.ropeSegments[i].posNow, posToBe, 0.015f);
				}
				else
				{
					Chain.ropeSegments[i].posNow = Chain.startPoint;
				}
			}

			if (!Main.dedServ)
			{
				ManageCache();
				ManageTrail();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			List<Vector2> points = GetChainPoints();
			Vector2 pos = points[NUM_SEGMENTS - 1];
			Texture2D tex = Request<Texture2D>(Texture + textureNumber).Value;

			if (Owner.name == "cloud")
				tex = Request<Texture2D>(Texture + "_Cloud").Value;

			DrawChain();

			Vector2 origin = tex.Size() * new Vector2(0.5f, 0.92f);
			float rot = pos.DirectionTo(Projectile.Center).ToRotation() - 1.57f;

			if (!attached)
			{
				origin = tex.Size() * 0.5f;
				rot = 0;
			}

			Main.spriteBatch.Draw(tex, pos - Main.screenPosition, null, Lighting.GetColor((int)(pos.X / 16), (int)(pos.Y / 16)), rot, origin, Projectile.scale, SpriteEffects.None, 0f);

			return false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override void Kill(int timeLeft)
		{
			Vector2 dustPos = GetChainPoints()[NUM_SEGMENTS - 1];
			SoundEngine.PlaySound(SoundID.NPCDeath63, dustPos);

			for (int i = 0; i < 12; i++)
			{
				Vector2 dir = Main.rand.NextVector2Circular(8, 8);
				Dust.NewDustPerfect(dustPos + dir * 2, ModContent.DustType<Dusts.GlowLineFast>(), dir, 0, Color.White, 0.5f);
			}

			if (attached)
				attachTarget.GetGlobalNPC<BalloonGunGNPC>().balloonsAttached--;
		}

		private void ManageCache()
		{
			cache = new List<Vector2>
			{
				Projectile.Center
			};

			float pointLength = TotalLength(GetChainPoints()) / NUM_SEGMENTS;
			float pointCounter = 0;
			int presision = 30; //This normalizes length between points so it doesnt squash super weirdly on certain parts

			for (int i = 0; i < NUM_SEGMENTS - 1; i++)
			{
				for (int j = 0; j < presision; j++)
				{
					pointCounter += (Chain.ropeSegments[i].posNow - Chain.ropeSegments[i + 1].posNow).Length() / presision;

					while (pointCounter > pointLength)
					{
						float lerper = j / (float)presision;
						cache.Add(Vector2.Lerp(Chain.ropeSegments[i].posNow, Chain.ropeSegments[i + 1].posNow, lerper));
						pointCounter -= pointLength;
					}
				}
			}

			while (cache.Count < NUM_SEGMENTS)
			{
				cache.Add(Chain.ropeSegments[NUM_SEGMENTS - 1].posNow);
			}

			while (cache.Count > NUM_SEGMENTS)
			{
				cache.RemoveAt(cache.Count - 1);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, NUM_SEGMENTS, new TriangularTip(1), factor => 4, factor => Lighting.GetColor((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16)));

			List<Vector2> positions = cache;
			trail.NextPosition = positions[NUM_SEGMENTS - 1];

			trail.Positions = positions.ToArray();
		}

		private List<Vector2> GetChainPoints()
		{
			var points = new List<Vector2>();

			foreach (RopeSegment ropeSegment in Chain.ropeSegments)
				points.Add(ropeSegment.posNow);

			return points;
		}

		private void DrawChain()
		{
			if (trail == null || trail == default)
				return;

			Main.spriteBatch.End();
			Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["alpha"].SetValue(1);

			trail?.Render(effect);

			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		private float TotalLength(List<Vector2> points)
		{
			float ret = 0;

			for (int i = 1; i < points.Count; i++)
			{
				ret += (points[i] - points[i - 1]).Length();
			}

			return ret;
		}
	}

	public class BalloonGunGNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public int balloonsAttached = 0;

		public override void AI(NPC npc)
		{
			if (npc.noGravity)
				npc.velocity.Y -= 0.005f * MathF.Pow(balloonsAttached, 0.7f);
			else if (!npc.collideY)
				npc.velocity.Y -= 0.08f * MathF.Pow(balloonsAttached, 0.7f);
		}
	}
}