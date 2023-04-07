using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class SpearBook : SmartAccessory, IPostLoadable
	{
		public int comboState;
		public static Dictionary<int, bool> spearList;

		public SpearBook() : base("Snake Technique", "Allows execution of combos with spears\nRight click to deter enemies with a flurry of stabs") { }

		public override string Texture => AssetDirectory.MiscItem + "SpearBook";

		public override void Load()
		{
			StarlightItem.CanUseItemEvent += OverrideSpearEffects;
		}

		public void PostLoad()
		{
			spearList = new Dictionary<int, bool>();
			Projectile proj = new Projectile();
			for (int i = 0; i < ProjectileLoader.ProjectileCount; i++)
			{
				proj.SetDefaults(i);
				spearList.Add(i, proj.aiStyle == 19);
			}
		}

		public override void Unload()
		{
			StarlightItem.CanUseItemEvent -= OverrideSpearEffects;
		}

		public void PostLoadUnload()
		{
			spearList.Clear();
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Orange;
		}

		private bool OverrideSpearEffects(Item item, Player player)
		{
			if (Equipped(player))
			{
				if (item != player.HeldItem)
					return true;

				if (item.DamageType.Type == DamageClass.Melee.Type && spearList[item.shoot] && item.noMelee)
				{
					if (Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<SpearBookProjectile>() && n.owner == player.whoAmI))
						return false;

					int i = Projectile.NewProjectile(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<SpearBookProjectile>(), item.damage, item.knockBack, player.whoAmI, comboState);
					Projectile proj = Main.projectile[i];

					proj.timeLeft = item.useAnimation * 4;

					if (proj.ModProjectile is SpearBookProjectile)
					{
						var modProj = proj.ModProjectile as SpearBookProjectile;
						modProj.trailColor = ItemColorUtility.GetColor(item.type);
						modProj.texture = TextureAssets.Projectile[item.shoot].Value;
					}

					float pitch = 1 - item.useAnimation / 60f;
					pitch += comboState * 0.1f;

					if (pitch >= 1)
						pitch = 1;

					if (Item.UseSound.HasValue)
						Terraria.Audio.SoundEngine.PlaySound(Item.UseSound.Value, player.Center);

					comboState++;
					comboState %= 4;

					return false;
				}
			}

			return true;
		}
	}

	class SpearBookProjectile : ModProjectile
	{
		enum AttackType : int
		{
			DownSwing,
			Stab,
			Slash,
			UpSwing,
			ChargedStab,
			Flurry
		}

		public Texture2D texture;
		public Color trailColor;

		private float holdout = 0.5f;

		public override string Texture => AssetDirectory.Invisible;
		public Player Owner => Main.player[Projectile.owner];
		
		private AttackType CurrentAttack
		{
			get => (AttackType)Projectile.ai[0];
			set => Projectile.ai[0] = (float)value;
		}
		private ref float timer => ref Projectile.ai[1];
		private ref float targetAngle => ref Projectile.ai[2];

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.scale = 1.5f;
		}

		public override void OnSpawn(IEntitySource source)
		{
			targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
			Projectile.direction = targetAngle > -MathHelper.PiOver2 && targetAngle < MathHelper.PiOver2 ? 1 : -1;
		}

		public override void AI()
		{
			Projectile.Center = Owner.Center;
			Owner.heldProj = Projectile.whoAmI;
			Owner.direction = Projectile.direction;

			switch(CurrentAttack)
			{
				case AttackType.DownSwing:
					DownSwing();
					break;
				case AttackType.Stab:
					DownSwing();
					break;
				case AttackType.Slash:
					DownSwing();
					break;
				case AttackType.UpSwing:
					DownSwing();
					break;
				case AttackType.ChargedStab:
					DownSwing();
					break;
				case AttackType.Flurry:
					DownSwing();
					break;
			}

			timer++;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 start = Owner.Center;
			Vector2 end = Owner.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * Projectile.Size.Length() * holdout;

			return null;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Helper.PlayPitched(Helpers.Helper.IsFleshy(target) ? "Impacts/StabFleshy" : "Impacts/Clink", 1, Main.rand.NextFloat(), Owner.Center);
			CameraSystem.shake += 2;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Vector2 origin;
			SpriteEffects effects;

			if (Projectile.direction < 0)
			{
				origin = new Vector2(texture.Width * (1 - holdout), texture.Height * holdout);
				Projectile.rotation += MathHelper.ToRadians(45f);
				effects = SpriteEffects.FlipHorizontally;
			}
			else
			{
				origin = new Vector2(texture.Width * holdout, texture.Height * holdout);
				Projectile.rotation += MathHelper.ToRadians(135f);
				effects = SpriteEffects.None;

			}

			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, default, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);
			return false;
		}

		private void DownSwing()
		{
			const float swingRange = MathHelper.Pi;
			const int swingTime = 30;
			float initialRotation = targetAngle - Projectile.direction * swingRange / 2;
			
			Projectile.rotation = initialRotation + Projectile.direction * MathHelper.SmoothStep(0, swingRange, timer / swingTime);

			if (timer > swingTime)
				Projectile.Kill();
		}
	}
}
