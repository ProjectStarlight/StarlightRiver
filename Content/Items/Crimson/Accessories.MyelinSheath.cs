using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core.Systems.BarrierSystem;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static tModPorter.ProgressUpdate;

namespace StarlightRiver.Content.Items.Crimson
{
	internal class MyelinSheath : SmartAccessory
	{
		int cooldown = 0;

		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public MyelinSheath() : base("Myelin Sheath", "Swords perform a powerful mind slash after not attacking for 2 seconds\nThe mind slash inflicts 5 stacks of either {{BUFF:Neurosis}} or {{BUFF:Psychosis}}") { }

		public override void Load()
		{
			StarlightItem.CanUseItemEvent += OverrideSwordEffects;
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Green;
			Item.accessory = true;
			Item.width = 32;
			Item.height = 32;

			Item.value = Item.sellPrice(gold: 1);
		}

		public override void SafeUpdateEquip(Player Player)
		{
			if (cooldown > 0)
				cooldown--;
		}

		private bool OverrideSwordEffects(Item item, Player player)
		{
			if (Equipped(player))
			{
				if (item != player.HeldItem)
					return true;

				if ((GetEquippedInstance(player) as MyelinSheath).cooldown <= 0)
				{
					(GetEquippedInstance(player) as MyelinSheath).cooldown = 120;

					if (item.DamageType.Type == DamageClass.Melee.Type && item.pick <= 0 && item.axe <= 0 && item.hammer <= 0 && item.useStyle == ItemUseStyleID.Swing && !item.noMelee)
					{
						if (Main.projectile.Any(n => n.active && (n.type == ModContent.ProjectileType<SwordBookProjectile>() || n.type == ModContent.ProjectileType<SwordBookParry>()) && n.owner == player.whoAmI))
							return false;

						Projectile.NewProjectile(player.GetSource_ItemUse(item), player.Center, player.Center.DirectionTo(Main.MouseWorld), ModContent.ProjectileType<MyelinSlash>(), item.damage, item.knockBack, player.whoAmI);
						SoundHelper.PlayPitched("Effects/HeavyWhoosh", 0.3f, 1.6f, player.Center);

						return false;
					}
				}
				else
				{
					if (item.DamageType.Type == DamageClass.Melee.Type && item.pick <= 0 && item.axe <= 0 && item.hammer <= 0 && item.useStyle == ItemUseStyleID.Swing && !item.noMelee)
						(GetEquippedInstance(player) as MyelinSheath).cooldown = 120;

					return true;
				}
			}

			return true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<DendriteBar>(), 12);
			recipe.AddIngredient(ModContent.ItemType<ImaginaryTissue>(), 6);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	class MyelinSlash : ModProjectile
	{
		public float length;
		public Texture2D texture;

		public float oldRot;
		public Vector2 oldPos;

		private bool hasDoneOnSpawn = false;

		public Item itemSnapshot; //lock in the item on creation incase they bypass the item switching prevention

		public ref float State => ref Projectile.ai[0];

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 30;
		}

		public override void OnSpawn(IEntitySource source)
		{
			SetTextureAndLength();
		}

		private void SetTextureAndLength()
		{
			if (!hasDoneOnSpawn)
			{
				hasDoneOnSpawn = true;
				itemSnapshot = Owner.HeldItem;

				if (Main.netMode != NetmodeID.Server)
				{
					texture = TextureAssets.Item[itemSnapshot.type].Value;
					length = (float)Math.Sqrt(Math.Pow(texture.Width, 2) + Math.Pow(texture.Width, 2)) * itemSnapshot.scale;
				}
			}
		}

		public override void AI()
		{
			Owner.itemAnimation = Owner.itemTime = Projectile.timeLeft; //lock inventory while this is active
			Owner.itemAnimationMax = 0; //make sure the regular weapon holdout doesn't render (makes an invisible super sword so you need to disable onhit elsewhere)

			Owner.heldProj = Projectile.whoAmI;

			SetTextureAndLength();

			if (Projectile.velocity.Length() > 0) // Orient on spawn
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 32f, ModContent.ProjectileType<GraySlash>(), Projectile.damage * 2, 1, Projectile.owner);

				Projectile.rotation = Projectile.velocity.ToRotation();
				oldRot = Projectile.rotation;
				Projectile.velocity *= 0;
			}

			if (Projectile.timeLeft == 25)
			{
				SoundHelper.PlayPitched("Effects/FancySwoosh", 0.7f, 0.5f, Projectile.Center);
			}

			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation);

			int direction = Vector2.UnitX.RotatedBy(oldRot).X > 0 ? 1 : -1;

			Projectile.rotation = oldRot + 2 * direction + Ease(1f - Projectile.timeLeft / 30f) * -4f * direction - 1.57f;

			Projectile.Center = Owner.Center + Vector2.UnitX.RotatedBy(Projectile.rotation);
		}

		public float Ease(float progress)
		{
			return (float)(3.386f * Math.Pow(progress, 3) - 7.259f * Math.Pow(progress, 2) + 4.873f * progress);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			int direction = Vector2.UnitX.RotatedBy(oldRot).X > 0 ? 1 : -1;
			bool flipSprite = direction < 0;

			Vector2 origin = flipSprite ? new Vector2(0, texture.Height) : new Vector2(texture.Width, texture.Height);
			SpriteEffects effects = flipSprite ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			float rot = Projectile.rotation + (flipSprite ? 0 : (float)Math.PI / 2f) + 1.57f * 1.5f;
			Vector2 pos = Projectile.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(rot);

			Main.spriteBatch.Draw(texture, pos, default, lightColor, rot, origin, Projectile.scale, effects, 0);
			return false;
		}
	}

	class GraySlash : ModProjectile
	{
		private List<Vector2> cache;
		private Trail trail;

		public override string Texture => AssetDirectory.Invisible;

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawSlashes;
		}

		private void DrawSlashes(SpriteBatch batch)
		{
			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.type == Type)
				{
					Texture2D tex = Assets.Slash.Value;
					batch.Draw(tex, proj.Center - Main.screenPosition, null, new Color(1f, 1f, 1f, 0) * (proj.timeLeft / 60f), proj.rotation, tex.Size() / 2f, 0.75f, 0, 0);

					Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

					var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
					Matrix view = Main.GameViewMatrix.TransformationMatrix;
					var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

					effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.03f);
					effect.Parameters["repeats"].SetValue(1f);
					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["sampleTexture"].SetValue(Assets.FireTrail.Value);
					(proj.ModProjectile as GraySlash)?.trail?.Render(effect);
				}
			}
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 60;
			Projectile.width = 128;
			Projectile.height = 128;
		}

		public override void AI()
		{
			GraymatterBiome.forceGrayMatter = true;
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.velocity *= 0.965f;

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Main.rand.NextBool())
			{
				for (int k = 0; k < 5; k++)
				{
					BuffInflictor.Inflict<Neurosis>(target, 1200);
				}
			}
			else
			{
				for (int k = 0; k < 5; k++)
				{
					BuffInflictor.Inflict<Psychosis>(target, 1200);
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		protected void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 20; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 20)
			{
				cache.RemoveAt(0);
			}
		}

		protected void ManageTrail()
		{
			trail = new Trail(Main.instance.GraphicsDevice, 20, new NoTip(), factor => factor * 100, factor =>
			{
				float alpha = factor.X;

				if (factor.X == 1)
					alpha = 0;

				alpha *= Projectile.timeLeft / 60f;

				return Color.White * alpha;
			})
			{
				Positions = cache.ToArray(),
				NextPosition = Projectile.Center + Projectile.velocity
			};
		}
	}
}