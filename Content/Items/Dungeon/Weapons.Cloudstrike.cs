﻿using StarlightRiver.Content.Items.SpaceEvent;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Dungeon
{
	//POTENTIAL TODO: this weapon's projectile is severely unoptimized and could use a rewrite from the ground up
	class Cloudstrike : ModItem
	{
		public const int MAXCHARGE = 120;

		private int charge = 1; //How much charge the weapon has (out of MAXCHARGE)

		private float chargeRatio => charge / (float)MAXCHARGE;

		private int counter = 0;
		public override string Texture => AssetDirectory.DungeonItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Cloudstrike");

			Tooltip.SetDefault("Accumulate electrical charge while not firing\n" +
			"Damage and range of your next shot increases with charge\n" +
			"'Meet this storm of sound and fury, till thunder-clashes fade to silence'");
		}

		public override void SetDefaults()
		{
			Item.damage = 45;
			Item.useTime = 5;
			Item.useAnimation = 5;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 100;
			Item.shoot = ModContent.ProjectileType<CloudstrikeShot>();
			Item.shootSpeed = 10;
			Item.value = Item.sellPrice(0, 5, 0, 0);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.channel = true;
			Item.noMelee = true;
		}

		public override ModItem Clone(Item Item)
		{
			ModItem clone = base.Clone(Item);

			if (!(Item.ModItem is Cloudstrike))
				return clone;

			if (Main.mouseItem.type == ModContent.ItemType<Cloudstrike>())
				Item.ModItem.HoldItem(Main.player[Main.myPlayer]);

			(clone as Cloudstrike).charge = (Item.ModItem as Cloudstrike).charge;
			(clone as Cloudstrike).counter = (Item.ModItem as Cloudstrike).counter;

			return clone;
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(charge);
			writer.Write(counter);
		}

		public override void NetReceive(BinaryReader reader)
		{
			charge = reader.ReadInt32();
			counter = reader.ReadInt32();
		}

		public override void ModifyManaCost(Player Player, ref float reduce, ref float mult)
		{
			mult = (float)Math.Sqrt(chargeRatio);
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-10, 0);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (charge == 1)
			{
				Helper.PlayPitched("Magic/LightningShortest" + (1 + charge % 4).ToString(), 0.2f, Main.rand.NextFloat(0f, 0.2f), player.Center);
			}
			else if (charge == MAXCHARGE)
			{
				damage = (int)(damage * 1.5f);
				//Full charge attack sound here
				Helper.PlayPitched("Magic/LightningExplode", 0.4f, 0f, player.Center);
			}
			else
			{
				//staggered attack sound here

				Helper.PlayPitched("Magic/LightningExplodeShallow", 0.4f, MathHelper.Clamp(1.0f - charge * 0.01f, 0f, 1.0f), player.Center);
				//MathHelper.Clamp(1.1f - (0.01f * (120.0f / charge)), 0.0f, 1.0f)
			}

			var dir = Vector2.Normalize(velocity);
			Vector2 pos = position + dir * 75 + dir.RotatedBy(-player.direction * 1.57f) * 5;
			Projectile.NewProjectile(source, pos, velocity.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)), type, damage, knockback, player.whoAmI, charge);

			CameraSystem.shake += (int)(Math.Sqrt(charge) * 2);

			//Dust.NewDustPerfect(pos, DustID.Electric, dir.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(5));
			if (charge > 60)
				player.velocity -= dir * (float)Math.Sqrt(charge - 60);
			charge = 1;
			return false;
		}

		public override void HoldItem(Player Player)
		{
			counter++;
			if (charge < MAXCHARGE && !Player.channel)
			{
				charge++;
				if (charge == MAXCHARGE)
				{
					//REACHING FULL CHARGE SOUND HERE
					Helper.PlayPitched("Magic/LightningChargeReady", 0.6f, 0f, Player.Center);
					for (int i = 0; i < 12; i++)
						CreateStatic(Item, charge, Player, true);
				}

				if (counter % 3 == 0) //change the 10 to the number of ticks you want the sound to loop on
				{
					Helper.PlayPitched("Magic/LightningChargeShort", (float)Math.Pow(charge / 200f, 2) * 2, MathHelper.Clamp(0.1f + charge / 120f, 0, 1), Player.Center);
				}
			}

			if (charge == MAXCHARGE && counter % 10 == 0) //change the 10 to the number of ticks you want the sound to loop on
			{
				//CHARGED IDLE SOUND HERE
			}

			if (Main.rand.NextBool((int)(50 / (float)Math.Sqrt(charge))) && !Player.channel)
			{
				CreateStatic(Item, charge, Player);
			}

			base.HoldItem(Player);
		}

		private static void CreateStatic(Item item, int charge, Player Player, bool fullCharge = false)
		{
			if (Main.myPlayer != Player.whoAmI)
				return;

			Vector2 dir = Main.rand.NextFloat(6.28f).ToRotationVector2();
			Vector2 offset = Main.rand.NextBool(4) ? dir * Main.rand.NextFloat(30) : new Vector2(Main.rand.Next(-35, 35), Player.height / 2);

			float smallCharge = fullCharge ? 0.5f : 0.01f;

			var source = new EntitySource_ItemUse(Player, item);
			var proj = Projectile.NewProjectileDirect(source, Player.Center + offset, dir.RotatedBy(Main.rand.NextFloat(-1, 1)) * 5, ModContent.ProjectileType<CloudstrikeShot>(), 0, 0, Player.whoAmI, smallCharge, 2);
			var mp = proj.ModProjectile as CloudstrikeShot;
			mp.velocityMult = Main.rand.Next(1, 4);

			if (fullCharge)
				mp.thickness = 0.45f;

			mp.baseColor = charge == MAXCHARGE ? new Color(200, 230, 255) : Color.Violet;
			if (charge == MAXCHARGE && Main.rand.NextBool(10))
				mp.baseColor = Color.Cyan;
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
		{
			damage.Flat = (int)((MathHelper.Lerp(10, 100, chargeRatio) - 45) * player.GetDamage(DamageClass.Magic).Multiplicative);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<InertStaff>(), 1);
			recipe.AddIngredient(ModContent.ItemType<Astroscrap>(), 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class CloudstrikeShot : ModProjectile, IDrawAdditive, IDrawPrimitive
	{
		public bool followPlayer = false; //Whether or not the bolt stays on the Player if they move/rotate their mouse

		public float thickness = 1; //Thickness of the trail

		public int velocityMult = 10; //How fast the bolt travels

		public Color baseColor = new(200, 230, 255);
		public Color endColor = Color.Purple;

		private bool initialized = false;

		private bool reachedMouse = false; //Whether or not the bolt has gone further past the Player than the mouse

		private List<Vector2> cache;
		private List<Vector2> cache2;
		private Trail trail;
		private Trail trail2;

		private readonly List<NPC> hitTargets = new();
		private NPC target = default;

		private Vector2 startPoint = Vector2.Zero;

		private Vector2 mousePos = Vector2.Zero; // this doesn't seem like the name matches whatever this is supposed to be

		private float curve; //How much the bolt curves toward it's target position

		private Vector2 oldVel = Vector2.Zero; //Velocity of the Projectile before it stops. Useful for predicting primitive positions

		private Vector2 oldPlayerPos = Vector2.Zero; //These 2 variables are used for following the Player
		private float oldRotation = 0f;

		public NPC host; //Magnetized enemies use this class for the little sparks around them. If this isn't default, the charge is drawn to the enemy

		private float Charge => Projectile.ai[0];

		private float ChargeSqrt => (float)Math.Sqrt(Charge);

		private int Reach => (int)Charge * 5 + 100;

		private int Power => (int)(ChargeSqrt * 3) + 10;

		private Player Owner => Main.player[Projectile.owner];

		private float Fade => Projectile.extraUpdates == 0 ? EaseFunction.EaseCubicOut.Ease(Projectile.timeLeft / 25f) : 1;

		private bool Miniature => Projectile.ai[1] == 2; //If this is true, it's a spark created around the Player
		private bool Branch => Projectile.ai[1] == 1; //If this is true, it's a branch of the main stream. This means it has a smaller starting ball + can't make further branches

		public override string Texture => AssetDirectory.DungeonItem + "Cloudstrike";

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.timeLeft = 60;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.extraUpdates = 14;
			Projectile.penetrate = -1;
			Projectile.hide = true;
			Projectile.usesLocalNPCImmunity = true;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Electro Shock");
		}

		public override void OnSpawn(IEntitySource source)
		{
			if (!Branch && !Miniature)
				mousePos = Main.MouseWorld;

			oldRotation = (Main.MouseWorld - Owner.Center).ToRotation();
		}

		public override void AI()
		{
			if (!initialized)
			{
				startPoint = Projectile.Center;

				if (Main.netMode != NetmodeID.Server)
					ManageCaches();

				initialized = true;

				if (!Branch && !Miniature)
				{
					Projectile.timeLeft = (int)(Math.Sqrt(ChargeSqrt) * 20) + 45;
					Projectile.penetrate = 4 + (int)(Charge / 10);
				}

				if (Charge < 10 && !(Branch || Miniature))
					followPlayer = true;

				oldPlayerPos = Owner.Center;
			}

			if (Main.netMode != NetmodeID.Server && (Projectile.timeLeft % 4 == 0 || Projectile.timeLeft <= 25))
			{
				if (Projectile.timeLeft % 2 == 0)
					ManageCaches();
				ManageTrails();
			}

			if (Projectile.timeLeft > 36 && !Miniature)
				Owner.itemTime = Owner.itemAnimation = (int)(ChargeSqrt + 1) * 3;

			if (Main.netMode != NetmodeID.Server)
			{
				foreach (Vector2 point in cache)
				{
					Lighting.AddLight(point, baseColor.ToVector3() * (float)Math.Sqrt(ChargeSqrt) * 0.3f * Fade);
				}

				for (int k = 1; k < cache2.Count; k++)
				{
					if (Main.rand.NextBool(40 * (Projectile.extraUpdates + 1)))
					{
						Vector2 prevPos = k == 1 ? startPoint : cache2[k - 1];
						Dust.NewDustPerfect(prevPos + new Vector2(0, 30), ModContent.DustType<CloudstrikeGlowLine>(), Vector2.Normalize(cache2[k] - prevPos) * Main.rand.NextFloat(-3, -2), 0, baseColor * (Power / 30f), 0.5f);
					}
				}
			}

			if (Projectile.timeLeft <= 25)
			{
				Projectile.velocity = Vector2.Zero;
				Projectile.extraUpdates = 0;
				return;
			}

			oldVel = Projectile.velocity / 6;

			CalculateTarget();

			CalculateVelocity();

			if (Main.myPlayer == Projectile.owner)
			{
				if (Main.rand.NextBool((int)Charge + 50) && !(Branch || Miniature)) //damaging, large branches
				{
					var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.7f, 0.7f)), ModContent.ProjectileType<CloudstrikeShot>(), Projectile.damage, Projectile.knockBack, Owner.whoAmI, Charge, 1);
					proj.timeLeft = (int)((Projectile.timeLeft - 25) * 0.75f) + 25;

					var modProj = proj.ModProjectile as CloudstrikeShot;
					modProj.mousePos = proj.Center + proj.velocity * 30;
					modProj.followPlayer = followPlayer;

					// These post creation force syncs are pretty bad performance/visuals wise
					// but its a symptom of using the same projectile in wildly different ways instead of a helper for the visuals
					// And would require a more thorough rewrite to fix
					NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj.whoAmI);
				}

				if (Main.rand.NextBool(10 + (int)Math.Sqrt(Cloudstrike.MAXCHARGE + 2 - Charge)) && !(Branch || Miniature)) //small, non damaging branches
				{
					var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)), ModContent.ProjectileType<CloudstrikeShot>(), 0, 0, Owner.whoAmI, 1, 1);
					proj.timeLeft = Math.Min(Main.rand.Next(40, 70), Projectile.timeLeft);

					var modProj = proj.ModProjectile as CloudstrikeShot;
					modProj.mousePos = proj.Center + proj.velocity * 30;
					modProj.followPlayer = followPlayer;

					NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj.whoAmI);
				}
			}
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (Projectile.timeLeft < 25 || hitTargets.Contains(target))
				return false;

			return base.CanHitNPC(target);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Owner.TryGetModPlayer(out StarlightPlayer starlightPlayer);
			starlightPlayer.SetHitPacketStatus(shouldRunProjMethods: true);

			hitTargets.Add(target);
			Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<CloudstrikeCircleDust>(), Vector2.Zero, 0, default, (float)Math.Pow(ChargeSqrt, 0.3f));

			for (int i = 0; i < 20; i++)
			{
				Dust.NewDustPerfect(target.Center + new Vector2(0, 30), ModContent.DustType<Dusts.GlowLine>(), Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat() * 6, 0, new Color(100, 150, 200) * (Power / 30f), 0.5f);
			}

			for (int j = 0; j < 6; j++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<CloudstrikeCircleDust>(), Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat() * 3, 0, default, (float)Math.Pow(ChargeSqrt, 0.3f) * 0.3f);
			}

			Projectile.localNPCImmunity[target.whoAmI] = 2;
			target.immune[Projectile.owner] = 2;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.timeLeft > 25)
			{
				ManageCaches();
				Projectile.timeLeft = 25;
			}

			return false;
		}

		private void ManageCaches(bool addPoint = false)
		{
			if (Main.netMode == NetmodeID.Server)
				return;

			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 50; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			if (Projectile.timeLeft > 25 || addPoint)
				cache.Add(Projectile.Center);

			while (cache.Count > 50)
			{
				cache.RemoveAt(0);
			}

			cache2 = new List<Vector2>();
			for (int i = 0; i < cache.Count; i++)
			{
				Vector2 point = cache[i];
				Vector2 nextPoint = i == cache.Count - 1 ? Projectile.Center + oldVel : cache[i + 1];
				Vector2 dir = Vector2.Normalize(nextPoint - point).RotatedBy(Main.rand.NextBool() ? -1.57f : 1.57f);

				if (i > cache.Count - 3 || dir == Vector2.Zero)
					cache2.Add(point);
				else
					cache2.Add(point + dir * Main.rand.NextFloat(5) * (float)Math.Sqrt(ChargeSqrt));
			}
		}

		private void ManageTrails()
		{
			if (Main.netMode == NetmodeID.Server)
				return;

			int sparkMult = Miniature ? 6 : 1;
			if (trail is null || trail.IsDisposed)
			{
				trail = new Trail(Main.instance.GraphicsDevice, 50, new NoTip(), factor => thickness * sparkMult * Main.rand.NextFloat(0.75f, 1.25f) * 16 * (float)Math.Pow(ChargeSqrt, 0.7f), factor =>
							{
								if (factor.X > 0.99f)
									return Color.Transparent;

								return new Color(160, 220, 255) * Fade * 0.1f * EaseFunction.EaseCubicOut.Ease(1 - factor.X);
							});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + oldVel;

			if (trail2 is null || trail2.IsDisposed)
			{
				trail2 = new Trail(Main.instance.GraphicsDevice, 50, new NoTip(), factor => thickness * sparkMult * 3 * (float)Math.Pow(ChargeSqrt, 0.7f) * Main.rand.NextFloat(0.55f, 1.45f), factor =>
							{
								float progress = EaseFunction.EaseCubicOut.Ease(1 - factor.X);
								return Color.Lerp(baseColor, endColor, EaseFunction.EaseCubicIn.Ease(1 - progress)) * Fade * progress;
							});
			}

			trail2.Positions = cache2.ToArray();
			trail2.NextPosition = Projectile.Center;
		}
		public void DrawPrimitives()
		{
			if (followPlayer)
			{
				UpdatePosition();
				UpdateRotation();
			}

			if (trail != null)
			{
				trail.Positions = cache.ToArray();
				trail.NextPosition = Projectile.Center + oldVel;
			}

			if (trail2 != null)
			{
				trail2.Positions = cache2.ToArray();
				trail2.NextPosition = Projectile.Center;
			}

			Effect effect = Filters.Scene["LightningTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);

			trail?.Render(effect);
			trail2?.Render(effect);
		}

		public void DrawAdditive(SpriteBatch sb)
		{
			if (followPlayer)
			{
				UpdatePosition();
				UpdateRotation();
			}

			Vector2 point1 = startPoint;

			if (point1 == Vector2.Zero)
				return;

			if (Branch)
				return;

			Texture2D tex = Assets.Keys.GlowSoft.Value;

			Color color = new Color(200, 230, 255) * Fade;
			for (int i = 0; i < ChargeSqrt; i++)
				sb.Draw(tex, startPoint - Main.screenPosition, null, color, 0, tex.Size() / 2, (float)MathHelper.Lerp(1, 2, ChargeSqrt / (float)Math.Sqrt(Cloudstrike.MAXCHARGE)) * ((Branch || Miniature) ? 0.25f : 0.5f), SpriteEffects.None, 0f);
		}

		private void CalculateTarget()
		{
			NPC temptarget = Main.npc.Where(x => x.active && !x.townNPC && !x.immortal && !x.dontTakeDamage && !x.friendly && !hitTargets.Contains(x) && x.Distance(Projectile.Center) < Reach).OrderBy(x => x.Distance(Projectile.Center)).FirstOrDefault();

			if (temptarget != default && Main.rand.NextBool((int)Math.Sqrt(temptarget.Distance(Projectile.Center)) + 1))
				target = temptarget;

			if (hitTargets.Contains(target))
				target = default;
		}

		private void CalculateVelocity()
		{
			Vector2 rotToBe;
			float distance;
			if (target != default)
			{
				Vector2 dir = target.Center - Projectile.Center;
				distance = dir.Length();
				rotToBe = Vector2.Normalize(dir);
			}
			else
			{
				if (Branch || Miniature)
				{
					if (Main.rand.NextBool(4))
						mousePos = Projectile.Center + Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.7f, 0.7f)) * 30;
				}
				else if (Main.rand.NextBool(6))
				{
					curve = Main.rand.NextFloat(-0.4f, 0.4f);
				}

				Vector2 dir;
				if (!reachedMouse)
				{
					dir = mousePos - Projectile.Center;

					Vector2 pToM = mousePos - Owner.Center;
					Vector2 pToL = Projectile.Center - Owner.Center;
					if (pToL.Length() > pToM.Length())
						reachedMouse = true;
				}
				else
				{
					dir = mousePos - Owner.Center;
				}

				distance = dir.Length();
				rotToBe = Vector2.Normalize(dir).RotatedBy(curve);
			}

			if (Miniature)
			{
				Vector2 hostCenter = host == default ? Owner.Center : host.Center;
				Vector2 dir2 = hostCenter + Main.rand.NextVector2Circular(12, 12) - Projectile.Center;
				distance = dir2.Length();
				rotToBe = Vector2.Normalize(dir2).RotatedBy(curve * 3);
			}

			float rotDifference = ((rotToBe.ToRotation() - Projectile.velocity.ToRotation()) % 6.28f + 9.42f) % 6.28f - 3.14f;

			float lerper = MathHelper.Lerp(0.55f, 0.35f, MathHelper.Min(1, distance / 300f));
			if (Miniature)
				lerper /= 3;
			float rot = MathHelper.Lerp(Projectile.velocity.ToRotation(), Projectile.velocity.ToRotation() + rotDifference, lerper);
			Projectile.velocity = rot.ToRotationVector2() * velocityMult;
		}

		private void UpdatePosition()
		{
			if (!followPlayer)
				return;
			Vector2 center = Owner.Center;
			if (oldPlayerPos != center)
			{
				Vector2 offset = center - oldPlayerPos;
				oldPlayerPos = center;

				startPoint += offset;
				Projectile.Center += offset;

				if (Main.netMode != NetmodeID.Server)
				{
					if (cache != null)
					{
						for (int i = 0; i < cache.Count; i++)
							cache[i] += offset;
					}

					if (cache2 != null)
					{
						for (int i = 0; i < cache2.Count; i++)
							cache2[i] += offset;
					}
				}
			}
		}

		private void UpdateRotation()
		{
			if (!followPlayer)
				return;

			if (Owner.itemTime != 1)
				return;

			Owner.TryGetModPlayer(out ControlsPlayer controlsPlayer);
			controlsPlayer.mouseRotationListener = true;

			float rot = (controlsPlayer.mouseWorld - Owner.Center).ToRotation();
			if (rot != oldRotation)
			{
				float difference = rot - oldRotation;

				startPoint = (startPoint - Owner.Center).RotatedBy(difference) + Owner.Center;
				Projectile.Center = (Projectile.Center - Owner.Center).RotatedBy(difference) + Owner.Center;

				if (Main.netMode != NetmodeID.Server)
				{
					if (cache != null)
					{
						for (int i = 0; i < cache.Count; i++)
							cache[i] = (cache[i] - Owner.Center).RotatedBy(difference) + Owner.Center;
					}

					if (cache2 != null)
					{
						for (int i = 0; i < cache2.Count; i++)
							cache2[i] = (cache2[i] - Owner.Center).RotatedBy(difference) + Owner.Center;
					}
				}

				oldRotation = rot;
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.WriteVector2(mousePos);
			writer.Write(oldRotation);
			writer.Write(followPlayer);
			writer.Write(Projectile.timeLeft);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			mousePos = reader.ReadVector2();
			oldRotation = reader.ReadSingle();
			followPlayer = reader.ReadBoolean();
			Projectile.timeLeft = reader.ReadInt32();
		}
	}
	class CloudstrikeCircleDust : ModDust
	{
		public override string Texture => "StarlightRiver/Assets/Keys/GlowSoft";

		public override void OnSpawn(Dust dust)
		{
			dust.color = Color.Transparent;
			dust.fadeIn = 0;
			dust.noLight = false;
			dust.frame = new Rectangle(0, 0, 64, 64);
			//dust.rotation = Main.rand.NextFloat(6.28f);
			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
		}

		public override bool Update(Dust dust)
		{
			if (dust.color == Color.Transparent)
				dust.position -= Vector2.One * 32 * dust.scale;

			//dust.rotation += dust.velocity.Y * 0.1f;
			dust.position += dust.velocity;

			dust.color = new Color(200, 230, 255);
			dust.shader.UseColor(dust.color * (1 - dust.alpha / 255f));

			dust.alpha += 18;
			if (dust.velocity == Vector2.Zero)
				dust.alpha += 7;

			Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.4f * dust.scale);

			if (dust.alpha > 255)
				dust.active = false;

			return false;
		}
	}

	class CloudstrikeGlowLine : ModDust
	{
		public override string Texture => AssetDirectory.VitricBoss + "RoarLine";

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color * MathHelper.Min(1, dust.fadeIn / 20f);
		}

		public override void OnSpawn(Dust dust)
		{
			dust.fadeIn = 0;
			dust.noLight = false;
			dust.frame = new Rectangle(0, 0, 8, 128);

			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData is null)
			{
				dust.position -= new Vector2(4, 64) * dust.scale;
				dust.customData = 1;
			}

			dust.rotation = dust.velocity.ToRotation() + 1.57f;
			dust.position += dust.velocity;

			dust.velocity *= 0.98f;
			dust.color *= 0.95f;

			dust.shader.UseColor(dust.color);
			dust.fadeIn += 2;

			Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.6f);

			if (dust.fadeIn > 60)
				dust.active = false;

			return false;
		}
	}
}