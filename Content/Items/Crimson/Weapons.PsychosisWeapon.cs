using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Breacher;
using StarlightRiver.Content.Items.Dungeon;
using StarlightRiver.Content.Items.UndergroundTemple;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Crimson
{
	public class PsychosisWeapon : ModItem
	{
		public float shootRotation;
		public int shootDirection;

		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public override void SetStaticDefaults()
		{
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;

			DisplayName.SetDefault("Neurosis & Psychosis");
			Tooltip.SetDefault(
				"<left> to fire bullets infused with {{BUFF:Neurosis}}\n" +
				"<right> to fire bullets infused with {{BUFF:Psychosis}}\n" +
				"Hitting enemies with the opposing effect causes a BRAIN BLAST, scaling with stacks"
				);
		}

		public override void SetDefaults()
		{
			Item.damage = 25;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 21;
			Item.useAnimation = 21;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 2f;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 5, 0, 0);
			Item.shoot = ProjectileID.PurificationPowder;
			Item.shootSpeed = 15f;
			Item.useAmmo = AmmoID.Bullet;
			Item.autoReuse = true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.TheUndertaker);
			recipe.AddIngredient(ModContent.ItemType<DendriteBar>(), 10);
			recipe.AddIngredient(ModContent.ItemType<ImaginaryTissue>(), 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}

		public override bool CanUseItem(Player Player)
		{
			shootRotation = (Player.Center - Main.MouseWorld).ToRotation();
			shootDirection = (Main.MouseWorld.X < Player.Center.X) ? -1 : 1;

			return base.CanUseItem(Player);
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-6, 0);
		}

		public override bool AltFunctionUse(Player Player)
		{
			return true;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			if (player.altFunctionUse == 2)
			{
				
			}
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			Vector2 itemPosition = CommonGunAnimations.SetGunUseStyle(player, Item, shootDirection, -10f, new Vector2(52f, 28f), new Vector2(-40f, 4f));

			float animProgress = 1f - player.itemTime / (float)player.itemTimeMax;

			if (animProgress >= 0.5f)
			{
				float lerper = (animProgress - 0.5f) / 0.5f;
				Dust.NewDustPerfect(itemPosition + new Vector2(50f, -10f * player.direction).RotatedBy(player.compositeFrontArm.rotation + 1.5707964f * player.gravDir), DustID.Smoke, Vector2.UnitY * -2f, (int)MathHelper.Lerp(210f, 200f, lerper), default, MathHelper.Lerp(0.5f, 1f, lerper)).noGravity = true;
			}
		}

		public override void UseItemFrame(Player player)
		{
			CommonGunAnimations.SetGunUseItemFrame(player, shootDirection, shootRotation, -0.2f, true, new Vector2(0.3f, 0.7f));
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Color c = player.altFunctionUse == 2 ? Red() : Blue();

			Vector2 barrelPos = position + new Vector2(60f, -20f * player.direction).RotatedBy(velocity.ToRotation());

			Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<FlareBreacherSmokeDust>(), velocity * 0.025f, 50, new Color(255, 0, 0), 0.1f);

			Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<FlareBreacherSmokeDust>(), velocity * 0.05f, 150, new Color(255, 0, 0), 0.2f);

			Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<FlareBreacherSmokeDust>(), velocity * 0.05f, 150, new Color(100, 100, 100), 0.2f);

			for (int i = 0; i < 4; i++)
			{
				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), DustID.Torch, velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.25f), 0, default, 1.2f).noGravity = true;

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), 
					ModContent.DustType<PixelatedGlow>(), velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.2f), 0, c, 0.2f).noGravity = true;

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f),
					ModContent.DustType<PixelatedEmber>(), velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.2f), 0, c, 0.2f).customData = -player.direction;

				Vector2 vel = velocity.SafeNormalize(Vector2.One);

				Dust.NewDustPerfect(barrelPos, DustID.Blood, vel.RotatedByRandom(0.5f) * Main.rand.NextFloat(5f), 50, default, 1.25f).noGravity = true;

				Dust.NewDustPerfect(barrelPos, DustID.Blood, vel.RotatedByRandom(0.5f) * Main.rand.NextFloat(5f), 50, default, 1.25f).fadeIn = 1f;

				Dust.NewDustPerfect(barrelPos, DustID.Blood, vel.RotatedByRandom(0.75f) * Main.rand.NextFloat(10f), 50, default, 1.5f).noGravity = true;

				Dust.NewDustPerfect(barrelPos, DustID.Blood, vel.RotatedByRandom(0.75f) * Main.rand.NextFloat(10f), 50, default, 1.5f).fadeIn = 1f;

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2CircularEdge(5f, 5f), ModContent.DustType<SmokeDustColor_Alt>(),
					vel.RotatedByRandom(0.25f) * Main.rand.NextFloat(2f), Main.rand.Next(120, 200), new Color(101, 13, 13), Main.rand.NextFloat(0.3f, 0.5f)).noGravity = true;

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2CircularEdge(5f, 5f), ModContent.DustType<SmokeDustColor_Alt>(),
					vel.RotatedByRandom(0.25f) * Main.rand.NextFloat(2f), Main.rand.Next(120, 200), new Color(215, 29, 29), Main.rand.NextFloat(0.5f, 0.7f)).noGravity = true;

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2CircularEdge(5f, 5f), ModContent.DustType<SmokeDustColor_Alt>(),
					vel.RotatedByRandom(0.25f) * Main.rand.NextFloat(2f) * Main.rand.NextFloat(), Main.rand.Next(120, 200), new Color(150, 15, 15), Main.rand.NextFloat(0.7f, 0.8f)).noGravity = true;

				Dust.NewDustPerfect(barrelPos, ModContent.DustType<GraveBlood>(), vel.RotatedByRandom(0.75f) * Main.rand.NextFloat(6f), 50, default, 2f).fadeIn = 1f;

				Dust.NewDustPerfect(barrelPos, ModContent.DustType<GraveBlood>(), vel.RotatedByRandom(1.5f) * Main.rand.NextFloat(9f), 50, default, 2f).fadeIn = 1f;
			}

			Vector2 flashPos = barrelPos - new Vector2(5f, 0f).RotatedBy(velocity.ToRotation());

			//Dust.NewDustPerfect(flashPos, ModContent.DustType<FlareBreacherMuzzleFlashDust>(), Vector2.Zero, 0, default, 0.75f).rotation = velocity.ToRotation();

			//SoundHelper.PlayPitched("Guns/FlareFire", 0.6f, Main.rand.NextFloat(-0.1f, 0.1f), position);
			CameraSystem.shake += 2;
			Item.noUseGraphic = true;

			Projectile p = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, velocity * 2, type, damage, knockback, player.whoAmI);

			PsychosisWeaponGlobalProjectile gp = p.GetGlobalProjectile<PsychosisWeaponGlobalProjectile>();

			if (player.altFunctionUse == 2)
				gp.psychosis = true;
			else
				gp.neurosis = true;

			return false;
		}

		public static Color Blue()
		{
			float r = 0.2f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.03f;
			float g = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 2f) * 0.05f;
			float b = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 4f) * 0.03f;
			return new Color(r, g, b, 0);
		}

		public static Color Red()
		{
			float r = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.03f;
			float g = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 2f) * 0.05f;
			float b = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 4f) * 0.03f;
			return new Color(r, g, b, 0);
		}
	}

	public class PsychosisWeaponGlobalProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		public bool psychosis = false;
		public bool neurosis = false;

		int timer;

		static Color DrawColor(bool psych) => psych ? PsychosisWeapon.Red() : PsychosisWeapon.Blue();

		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (psychosis)
			{
				StackableBuff b = InstancedBuffNPC.GetInstance<Neurosis>(target);

				if (b != null && target.HasBuff(b.BackingType))
				{
					Main.NewText("Cleared Neurosis, stack count: " + b.stacks.Count());
					target.DelBuff(target.FindBuffIndex(b.BackingType));
					(b as Neurosis).stacks.Clear();
				}
				else
				{
					BuffInflictor.Inflict<Psychosis>(target, 240);
				}

				for (int i = 0; i < 5; i++)
				{
					Color c = DrawColor(psychosis);

					Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(5f, 5f),
						ModContent.DustType<PixelatedGlow>(), Main.rand.NextVector2Circular(2f, 2f), 0, c, 0.2f).noGravity = true;

					Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(5f, 5f),
						ModContent.DustType<PixelatedEmber>(), Main.rand.NextVector2CircularEdge(2f, 2f), 0, c, 0.2f).customData = Main.rand.NextBool() ? -1 : 1;
				}
			}
			else if (neurosis)
			{
				StackableBuff b = InstancedBuffNPC.GetInstance<Psychosis>(target);

				if (b != null && target.HasBuff(b.BackingType))
				{
					Main.NewText("Cleared Psychosis, stack count: " + b.stacks.Count());
					target.DelBuff(target.FindBuffIndex(b.BackingType));
					(b as Psychosis).stacks.Clear();				
				}
				else
				{
					BuffInflictor.Inflict<Neurosis>(target, 240);
				}

				for (int i = 0; i < 5; i++)
				{
					Color c = DrawColor(psychosis);

					Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(5f, 5f),
						ModContent.DustType<PixelatedGlow>(), Main.rand.NextVector2Circular(2f, 2f), 0, c, 0.2f).noGravity = true;

					Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(5f, 5f),
						ModContent.DustType<PixelatedEmber>(), Main.rand.NextVector2CircularEdge(2f, 2f), 0, c, 0.2f).customData = Main.rand.NextBool() ? -1 : 1;
				}
			}
		}

		public override void AI(Projectile projectile)
		{
			if (psychosis || neurosis)
			{
				timer++;
				if (timer % 2 == 0)
				{
					Dust.NewDustPerfect(projectile.Center + new Vector2(0f, 1.5f * (float)Math.Sin(timer)).RotatedBy(projectile.velocity.ToRotation()) * 9f, ModContent.DustType<Dusts.PixelatedEmber>(),
						projectile.velocity * 0.05f, 0, DrawColor(psychosis), 0.2f * (1 + (float)Math.Sin(timer / 2)));
				}		
			}			
		}
	}
}
