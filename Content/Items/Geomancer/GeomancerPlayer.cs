using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Geomancer
{
	public enum StoredGem
	{
		Diamond,
		Ruby,
		Sapphire,
		Emerald,
		Amethyst,
		Topaz,
		None,
		All
	}

	public class GeomancerPlayer : ModPlayer
	{
		public bool SetBonusActive = false;

		public StoredGem storedGem = StoredGem.None;

		public bool DiamondStored = false;
		public bool RubyStored = false;
		public bool EmeraldStored = false;
		public bool SapphireStored = false;
		public bool TopazStored = false;
		public bool AmethystStored = false;

		public int timer = -1;
		public int rngProtector = 0;

		public int allTimer = 150;
		public float ActivationCounter = 0;

		static Item rainbowDye;
		static bool rainbowDyeInitialized = false;
		public static int shaderValue = 0;
		public static int shaderValue2 = 0;

		public override void Load()
		{
			StarlightPlayer.PreDrawEvent += PreDrawGlowFX;
		}

		public override void Unload()
		{
			rainbowDye = null;
		}

		private void PreDrawGlowFX(Player Player, SpriteBatch spriteBatch)
		{
			if (!Player.GetModPlayer<GeomancerPlayer>().SetBonusActive)
				return;

			if (!CustomHooks.PlayerTarget.canUseTarget)
				return;

			float fadeOut = 1;
			if (allTimer < 60)
				fadeOut = allTimer / 60f;

			spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			Effect effect = Filters.Scene["RainbowAura"].GetShader().Shader;

			if (Player.GetModPlayer<GeomancerPlayer>().storedGem == StoredGem.All)
			{

				float sin = (float)Math.Sin(Main.GameUpdateCount / 10f);
				float opacity = 1.25f - (sin / 2 + 0.5f) * 0.8f;

				effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.03f);
				effect.Parameters["uOpacity"].SetValue(opacity);
				effect.CurrentTechnique.Passes[0].Apply();

				for (int k = 0; k < 6; k++)
				{
					Vector2 dir = Vector2.UnitX.RotatedBy(k / 6f * 6.28f) * (5.5f + sin * 2.2f);
					Color color = Color.White * (opacity - sin * 0.1f) * 0.9f;

					spriteBatch.Draw(CustomHooks.PlayerTarget.Target, CustomHooks.PlayerTarget.getPlayerTargetPosition(Player.whoAmI) + dir, CustomHooks.PlayerTarget.getPlayerTargetSourceRectangle(Player.whoAmI), color * 0.25f * fadeOut);
				}
			}
			else if (Player.GetModPlayer<GeomancerPlayer>().ActivationCounter > 0)
			{
				float sin = Player.GetModPlayer<GeomancerPlayer>().ActivationCounter;
				float opacity = 1.5f - sin;

				Color color = GetArmorColor(Player) * (opacity - sin * 0.1f) * 0.9f;

				effect.Parameters["uColor"].SetValue(color.ToVector3());
				effect.Parameters["uOpacity"].SetValue(sin);
				effect.CurrentTechnique.Passes[1].Apply();

				for (int k = 0; k < 6; k++)
				{
					Vector2 dir = Vector2.UnitX.RotatedBy(k / 6f * 6.28f) * (sin * 8f);

					spriteBatch.Draw(CustomHooks.PlayerTarget.Target, CustomHooks.PlayerTarget.getPlayerTargetPosition(Player.whoAmI) + dir, CustomHooks.PlayerTarget.getPlayerTargetSourceRectangle(Player.whoAmI), Color.White * 0.25f);
				}
			}

			spriteBatch.End();

			SamplerState samplerState = Main.DefaultSamplerState;

			if (Player.mount.Active)
				samplerState = Terraria.Graphics.Renderers.LegacyPlayerRenderer.MountedSamplerState;

			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		public override void ResetEffects()
		{
			if (!rainbowDyeInitialized)
			{
				rainbowDyeInitialized = true;
				rainbowDye = new Item();
				rainbowDye.SetDefaults(ModContent.ItemType<RainbowCycleDye>());
				shaderValue = rainbowDye.dye;

				var rainbowDye2 = new Item();
				rainbowDye2.SetDefaults(ModContent.ItemType<RainbowCycleDye2>());
				shaderValue2 = rainbowDye2.dye;
			}

			if (!SetBonusActive)
			{
				storedGem = StoredGem.None;
				DiamondStored = false;
				RubyStored = false;
				EmeraldStored = false;
				SapphireStored = false;
				TopazStored = false;
				AmethystStored = false;
			}

			SetBonusActive = false;

			/*if (DiamondStored && RubyStored && EmeraldStored && SapphireStored && TopazStored && AmethystStored)
            {
                DiamondStored = false;
                RubyStored = false;
                EmeraldStored = false;
                SapphireStored = false;
                TopazStored = false;
                AmethystStored = false;

                storedGem = StoredGem.All;

                allTimer = 150;
            }*/
		}

		public override void PreUpdate()
		{
			if (!SetBonusActive)
				return;

			timer--;

			BarrierPlayer shieldPlayer = Player.GetModPlayer<BarrierPlayer>();
			if ((storedGem == StoredGem.Topaz || storedGem == StoredGem.All) && Player.ownedProjectileCounts[ModContent.ProjectileType<TopazShield>()] == 0 && shieldPlayer.maxBarrier - shieldPlayer.barrier < 100)
				Projectile.NewProjectile(Player.GetSource_ItemUse(Player.armor[0]), Player.Center, Vector2.Zero, ModContent.ProjectileType<TopazShield>(), 10, 7, Player.whoAmI);

			if (storedGem == StoredGem.All)
			{
				allTimer--;
				if (allTimer < 0)
					storedGem = StoredGem.None;
			}

			ActivationCounter -= 0.03f;
			Lighting.AddLight(Player.Center, GetArmorColor(Player).ToVector3());
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!SetBonusActive)
				return;

			if (proj.DamageType != DamageClass.Magic)
				return;

			int odds = Math.Max(1, 15 - rngProtector);
			if ((hit.Crit || target.life <= 0) && storedGem != StoredGem.All)
			{
				rngProtector++;
				if (Main.rand.NextBool(odds))
				{
					rngProtector = 0;
					SpawnGem(target, Player.GetModPlayer<GeomancerPlayer>());
				}
			}

			int critRate = Math.Min(Player.HeldItem.crit, 4);
			critRate += (int)(100 * Player.GetCritChance(DamageClass.Magic));

			if (Main.rand.Next(100) <= critRate && (storedGem == StoredGem.Sapphire || storedGem == StoredGem.All))
			{
				int numStars = Main.rand.Next(3) + 1;
				for (int i = 0; i < numStars; i++) //Doing a loop so they spawn separately
				{
					Item.NewItem(target.GetSource_Loot(), new Rectangle((int)target.position.X, (int)target.position.Y, target.width, target.height), ModContent.ItemType<SapphireStar>());
				}
			}

			if ((storedGem == StoredGem.Diamond || storedGem == StoredGem.All) && hit.Crit)
			{
				int extraDamage = target.defense / 2;
				extraDamage += (int)(proj.damage * 0.2f * (target.life / (float)target.lifeMax));
				CombatText.NewText(target.Hitbox, new Color(200, 200, 255), extraDamage);

				if (target.type != NPCID.TargetDummy)
					target.life -= extraDamage;

				target.HitEffect(0, extraDamage);
			}

			if (Main.rand.Next(100) <= critRate && (storedGem == StoredGem.Emerald || storedGem == StoredGem.All))
				Item.NewItem(target.GetSource_Loot(), new Rectangle((int)target.position.X, (int)target.position.Y, target.width, target.height), ModContent.ItemType<EmeraldHeart>());

			if ((storedGem == StoredGem.Ruby || storedGem == StoredGem.All) && Main.rand.NextFloat() > 0.3f && proj.type != ModContent.ProjectileType<RubyDagger>())
				Projectile.NewProjectile(Player.GetSource_ItemUse(Player.armor[0]), Player.Center, Main.rand.NextVector2Circular(7, 7), ModContent.ProjectileType<RubyDagger>(), (int)(proj.damage * 0.3f) + 1, hit.Knockback, Player.whoAmI, target.whoAmI);

			if (storedGem == StoredGem.Amethyst || storedGem == StoredGem.All && target.GetGlobalNPC<GeoNPC>().amethystDebuff < 400)
			{
				if (Main.rand.NextBool(Math.Max(10 / Player.HeldItem.useTime * (int)Math.Pow(target.GetGlobalNPC<GeoNPC>().amethystDebuff, 0.3f) / 2, 1)))
				{
					Projectile.NewProjectile(
						Player.GetSource_ItemUse(Player.armor[0]),
						target.position + new Vector2(Main.rand.Next(target.width), Main.rand.Next(target.height)),
						Vector2.Zero,
						ModContent.ProjectileType<AmethystShard>(),
						0,
						0,
						Player.whoAmI,
						target.GetGlobalNPC<GeoNPC>().amethystDebuff,
						target.whoAmI);
					target.GetGlobalNPC<GeoNPC>().amethystDebuff += 100;
				}
			}
		}

		private static void SpawnGem(NPC target, GeomancerPlayer modPlayer)
		{
			int ItemType = -1;
			var ItemTypes = new List<int>();

			if (!modPlayer.AmethystStored)
				ItemTypes.Add(ModContent.ItemType<GeoAmethyst>());

			if (!modPlayer.TopazStored)
				ItemTypes.Add(ModContent.ItemType<GeoTopaz>());

			if (!modPlayer.EmeraldStored)
				ItemTypes.Add(ModContent.ItemType<GeoEmerald>());

			if (!modPlayer.SapphireStored)
				ItemTypes.Add(ModContent.ItemType<GeoSapphire>());

			if (!modPlayer.RubyStored)
				ItemTypes.Add(ModContent.ItemType<GeoRuby>());

			if (!modPlayer.DiamondStored)
				ItemTypes.Add(ModContent.ItemType<GeoDiamond>());

			if (ItemTypes.Count == 0)
				return;

			ItemType = ItemTypes[Main.rand.Next(ItemTypes.Count)];

			Item.NewItem(target.GetSource_Loot(), new Rectangle((int)target.position.X, (int)target.position.Y, target.width, target.height), ItemType, 1);
		}

		public static void PickOldGem(Player Player)
		{
			GeomancerPlayer modPlayer = Player.GetModPlayer<GeomancerPlayer>();
			var gemTypes = new List<StoredGem>();

			if (modPlayer.AmethystStored)
				gemTypes.Add(StoredGem.Amethyst);

			if (modPlayer.TopazStored)
				gemTypes.Add(StoredGem.Topaz);

			if (modPlayer.SapphireStored)
				gemTypes.Add(StoredGem.Sapphire);

			if (modPlayer.RubyStored)
				gemTypes.Add(StoredGem.Ruby);

			if (modPlayer.EmeraldStored)
				gemTypes.Add(StoredGem.Emerald);

			if (modPlayer.DiamondStored)
				gemTypes.Add(StoredGem.Diamond);

			if (gemTypes.Count == 0)
				modPlayer.storedGem = StoredGem.None;
			else
				modPlayer.storedGem = gemTypes[Main.rand.Next(gemTypes.Count)];
		}

		public static Color GetArmorColor(Player Player)
		{
			StoredGem storedGem = Player.GetModPlayer<GeomancerPlayer>().storedGem;

			return storedGem switch
			{
				StoredGem.All => Main.hslToRgb((float)Main.timeForVisualEffects * 0.005f % 1, 1f, 0.5f),
				StoredGem.Amethyst => Color.Purple,
				StoredGem.Topaz => Color.Yellow,
				StoredGem.Emerald => Color.Green,
				StoredGem.Sapphire => Color.Blue,
				StoredGem.Diamond => Color.Cyan,
				StoredGem.Ruby => Color.Red,
				_ => Color.White,
			};
		}
	}

	public class GeoNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public int amethystDebuff;

		public override bool PreAI(NPC NPC)
		{
			if (amethystDebuff > 0)
				amethystDebuff--;

			return base.PreAI(NPC);
		}

		public override void UpdateLifeRegen(NPC NPC, ref int damage)
		{
			if (NPC.lifeRegen > 0)
			{
				NPC.lifeRegen = 0;
			}

			NPC.lifeRegen -= amethystDebuff / 50;
			if (damage < amethystDebuff / 150)
			{
				damage = amethystDebuff / 150;
			}
		}
	}
}