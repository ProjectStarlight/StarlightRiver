using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using StarlightRiver.Noise;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.MechBoss
{
	internal class VileVial : ModItem
	{
		public override string Texture => AssetDirectory.MechBossItem + Name;
		
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vile Vial");
			Tooltip.SetDefault("WIP");
			ItemID.Sets.Spears[Type] = true;
		}
		
		public override void SetDefaults()
		{
			Item.damage = 78;
			Item.crit = 12;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.gold * 6;
			Item.DamageType = DamageClass.Summon;
			Item.shootSpeed = 1;
			Item.shoot = ModContent.ProjectileType<VileVialSentry>();
			Item.noMelee = true;
			Item.UseSound = SoundID.Tink;
			Item.rare = ItemRarityID.Pink;
			Item.sentry = true;
			Item.mana = 20;
		}
		
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.HallowedBar, 15);
			recipe.AddIngredient(ItemID.SoulofFright, 15);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
		
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			position = player.GetModPlayer<ControlsPlayer>().mouseWorld;
			velocity = Vector2.Zero;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse != 2)
			{
				Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
				player.UpdateMaxTurrets();
			}
			
			return false;
		}
	}

	internal class VileVialSentry : ModProjectile
	{
		public const int RADIUS = 1000;
		
		public Player Owner => Main.player[Projectile.owner];
		
		public ref float Timer => ref Projectile.ai[0];
		public ref float Souls => ref Projectile.ai[1];
		public int spawnedSouls = 0;

		public List<VileVialInnerSoul> souls =  new();
		
		public override string Texture => AssetDirectory.MechBossItem + Name;
		
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vile Vial");
		}
		
		public override void SetDefaults()
		{
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.tileCollide = true;
			Projectile.penetrate = -1;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.sentry = true;
			Projectile.timeLeft = Projectile.SentryLifeTime;
		}
		
		public override void OnSpawn(IEntitySource source)
		{
			Owner.FindSentryRestingSpot(Projectile.whoAmI, out int worldX, out int worldY, out int pushYUp);
			Projectile.position = new Vector2(worldX, worldY - pushYUp - 27);
		}
		
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override void AI()
		{
			Timer++;
			
			foreach (NPC npc in Main.npc.Where(n => n.active && Vector2.DistanceSquared(n.Center, Projectile.Center) < Math.Pow(RADIUS / 2, 2) && n.CanBeChasedBy(Projectile)))
			{
				BuffInflictor.Inflict<VileVialDrain>(npc, 1);
				float dustAngle = Projectile.Center.AngleFrom(npc.Center);
				dustAngle += 0.6f * Vector2.Dot(Vector2.UnitX, Vector2.Normalize(npc.Center - Projectile.Center));
				VileVialDrainDust.Spawn(Main.rand.NextVector2Circular(32, 32), dustAngle, npc, 0.4f, new Color(127, 255, 255));
				Souls += 0.01f;
				Main.NewText($"{Souls} {spawnedSouls}");
			}

			if (Math.Floor(Souls) > spawnedSouls)
			{
				spawnedSouls++;
				souls.Add(new VileVialInnerSoul(new Vector2(32, 16), Vector2.Zero));
			}

			foreach (VileVialInnerSoul soul in souls)
			{
				soul.Update();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			foreach (NPC npc in Main.npc.Where(n => n.active && Vector2.DistanceSquared(n.Center, Projectile.Center) < Math.Pow(RADIUS / 2, 2) && n.CanBeChasedBy(Projectile)))
			{
				DrawDrainLine(Main.spriteBatch, npc.Center);
			}
			//DrawDrainLine(Main.spriteBatch, Owner.Center);
			
			Texture2D statueTex = ModContent.Request<Texture2D>(Texture).Value;
			
			var source = new Rectangle(0, 0, 48, 64);

			Main.spriteBatch.Draw(statueTex, Projectile.Center - Main.screenPosition, source, lightColor, 0, new Vector2(24, 32), 1, 0, 0);
			
			foreach (VileVialInnerSoul soul in souls)
			{
				soul.Draw(Projectile.Center - Main.screenPosition, Main.spriteBatch);
			}
			
			return false;
		}

		public void DrawDrainLine(SpriteBatch spriteBatch, Vector2 endPoint)
		{
			Texture2D texBeam = Assets.EnergyTrail.Value;

			float rotation = Projectile.Center.DirectionTo(endPoint).ToRotation();
			float distance = Vector2.Distance(Projectile.Center, endPoint);

			Color color1 = new Color(150, 10, 10);
			color1.A = 0;
			Color color2 = new Color(127, 255, 255) * 0.5f;
			color2.A = 0;

			var origin = new Vector2(0, texBeam.Height / 2);

			Effect effect = StarlightRiver.Instance.Assets.Request<Effect>("Effects/BentTrail").Value;

			if (effect is null)
				return;
			
			float height = texBeam.Height;
			int width = (int)distance;
			
			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderNPCs", () =>
			{
				effect.Parameters["time"].SetValue(Timer);
				effect.Parameters["speed"].SetValue(0.02f);

				effect.Parameters["squeeze1"].SetValue(0.0f);
				effect.Parameters["squeeze2"].SetValue(1.0f);
				effect.Parameters["distance"].SetValue(distance / texBeam.Width / 2);
			
				effect.Parameters["bendFac"].SetValue(1.0f * Vector2.Dot(Vector2.UnitX, Vector2.Normalize(endPoint -  Projectile.Center)) * (distance / 1200.0f));
			
				effect.Parameters["color1"].SetValue(color1.ToVector4());
				effect.Parameters["color2"].SetValue(color2.ToVector4() * 0.201f);

				Vector2 pos = Projectile.Center - Main.screenPosition;

				var target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)height);

				var source = new Rectangle(0, 0, (int)(distance / texBeam.Width * texBeam.Width / 2), texBeam.Height);
				
				spriteBatch.End();
			
				spriteBatch.Begin(default, default, SamplerState.PointWrap, default, RasterizerState.CullNone, effect);
				spriteBatch.Draw(texBeam, target, source, Color.White, rotation, origin, 0, 0);
				spriteBatch.End();
			
				effect.Parameters["speed"].SetValue(0.01f);
				effect.Parameters["color1"].SetValue(color1.ToVector4() * 0.1f);
				effect.Parameters["color2"].SetValue(color2.ToVector4() * 0.5f);
			
				spriteBatch.Begin(default, default, SamplerState.PointWrap, default, RasterizerState.CullNone, effect);
				spriteBatch.Draw(texBeam, target, source, Color.White * 0.0f, rotation, origin, 0, 0);
				spriteBatch.End();
			
				spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
			});

			Lighting.AddLight(endPoint, color2.ToVector3() * height * 0.002f);

			Texture2D impactTex = Assets.Masks.GlowAlpha.Value;

			spriteBatch.Draw(impactTex, endPoint - Main.screenPosition, null, color2 * (height * 0.004f), 0, impactTex.Size() / 2, 0.8f, 0, 0);
		}
	}
	
	internal class VileVialDrain : StackableBuff
	{
		public override string Name => "VileVialDrain";

		public override string DisplayName => "Vile Drain";

		public override string Texture => AssetDirectory.Debug;

		public override bool Debuff => true;

		public override BuffStack GenerateDefaultStack(int duration)
		{
			return new BuffStack()
			{
				duration = duration
			};
		}

		public override void PerStackEffectsNPC(NPC npc, BuffStack stack)
		{
			npc.lifeRegen -= 40;
		}
	}
	
	internal class VileVialInnerSoul
	{	
		public Vector2 position;
		public Vector2 velocity;
		public List<Vector2> tail = new(16);
		private FastNoise noise;

		public VileVialInnerSoul(Vector2 position, Vector2  velocity)
		{
			this.position = position;
			this.velocity = velocity;
		}

		public void Update()
		{
			noise ??= new FastNoise(Main.rand.Next(9999))
			{
				NoiseType = FastNoise.NoiseTypes.Perlin
			};
			
			velocity += new Vector2(Noise(position.X + 100), Noise(position.Y + 200f)) * 0.2f;
			velocity += -position * 0.001f;
			velocity *= 0.98f;
			position += velocity;
			if (tail.Count >= 16)
				tail.RemoveAt(tail.Count - 1);
			tail.Insert(0, position);
		}

		private float Noise(float x)
		{
			return noise.GetPerlin(x * 5.0f, (float)Main.timeForVisualEffects * 20.0f);
		} 

		public void Draw(Vector2 center, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Assets.Masks.Glow.Value, center + position, null, Color.White, 0, new Vector2(32, 32), new Vector2(0.1f, 0.1f), 0, 1f);
		}
	}

	
	public class VileVialDrainDust : Glow
	{
		public struct VileVialDrainDustData(Vector2 offset, float angle, NPC target)
		{
			public Vector2 offset = offset;
			public float angle = angle;
			public NPC target = target;
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData is VileVialDrainDustData data)
			{
				dust.color.A = 0;
				dust.fadeIn++;

				if (data.target is null || !data.target.active)
				{
					dust.active = false;
					return false;
				}

				dust.position = data.target.Center + data.offset + new Vector2(dust.fadeIn * 2f, 0).RotatedBy(data.angle);
				Lighting.AddLight(dust.position, dust.color.ToVector3());

				dust.alpha = (int) (dust.fadeIn * 255 / 40 / 2) + 127;
				
				if (dust.fadeIn++ > 40)
					dust.active = false;
			}
			else
			{
				Main.NewText("Dust spawned with invalid custom data.", Color.Red);
				dust.active = false;
			}

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = Assets.Masks.GlowAlpha.Value;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
			{
				Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, dust.color * lerper, dust.rotation, tex.Size() / 2f, dust.scale * lerper, 0f, 0f);
			});

			return false;
		}

		public static void Spawn(Vector2 offset, float angle, NPC target, float scale, Color color)
		{
			var d = Dust.NewDustPerfect(target.Center + offset, ModContent.DustType<VileVialDrainDust>(), Vector2.Zero, 0, color, scale);
			d.customData = new VileVialDrainDustData(offset, angle, target);
		}
	}
}