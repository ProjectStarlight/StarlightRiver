using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Haunted
{
	internal class SpiritSeal : BaseTalisman<SpiritSealProjectile, SpiritSealBuff>
	{
		public override string Texture => AssetDirectory.HauntedItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Spirit Seal");
			Tooltip.SetDefault("Sticks to enemies\nInflicts shared pain on ALL enemies with any stacks when ONE enemy reaches 7 stacks");
		}

		public override void SafeSetDefaults()
		{
			Item.useTime = 5;
			Item.useAnimation = 15;
			Item.reuseDelay = 20;
			Item.damage = 12;
			Item.mana = 7;
			Item.shootSpeed = 14;
			Item.rare = ItemRarityID.Green;

			spread = 0.3f;
		}

		public override void AddRecipes()
		{
			Recipe.Create(Type, 500)
				.AddIngredient<Talismans>(500)
				.AddIngredient<VengefulSpirit>(1)
				.Register();
		}
	}

	internal class SpiritSealProjectile : BaseTalismanProjectile<SpiritSealBuff>
	{
		public override string Texture => AssetDirectory.HauntedItem + Name;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 20;
			ProjectileID.Sets.TrailingMode[Type] = 2;

			base.SetStaticDefaults();
		}

		public override void AI()
		{
			base.AI();

			Lighting.AddLight(Projectile.Center, new Vector3(0.4f, 0.5f, 0.25f));

			if (Main.rand.NextBool(20))
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Cinder>(), Projectile.velocity * -0.1f, 0, new Color(0.5f, 0.8f, 0.05f));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (!hitTile)
			{
				for (int k = 0; k < 10; k += 2)
				{
					Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Misc/TalismansProjectile").Value;
					Vector2 pos = Projectile.oldPos[k] - Main.screenPosition + new Vector2(14, 11);
					var frame = new Rectangle(0, Projectile.frame * 22, 28, 22);

					Main.spriteBatch.Draw(tex, pos, frame, Color.LimeGreen * (1 - k / 10f) * 0.2f, Projectile.oldRot[k], new Vector2(14, 11), 0.8f, 0, 0);
				}
			}

			return true;
		}
	}

	internal class SpiritSealBuff : BaseTalismanBuff
	{
		public override string Name => "SpiritSealBuff";

		public override string DisplayName => "Spirit Seal";

		public override string Texture => AssetDirectory.HauntedItem + Name;

		public override bool Debuff => true;

		public SpiritSealBuff() : base()
		{
			threshold = 7;
		}

		public override BuffStack GenerateDefaultStack(int duration)
		{
			var stack = new BuffStack
			{
				duration = duration
			};
			return stack;
		}

		public override void OnMaxStacks(NPC npc)
		{
			foreach (NPC other in Main.npc.Where(n => n.active && AnyInflicted(n)))
			{
				other.AddBuff(ModContent.BuffType<SpiritSealDamageShare>(), 600);
			}
		}
	}

	internal class SpiritSealDamageShare : SmartBuff
	{
		public override string Texture => AssetDirectory.HauntedItem + "SpiritSealBuff";

		public SpiritSealDamageShare() : base("Shared pain", "Feeling their hurt!", true, false) { }

		public override void Load()
		{
			StarlightNPC.OnHitByItemEvent += ShareItemHit;
			StarlightNPC.OnHitByProjectileEvent += ShareProjHit;
			StarlightNPC.PostDrawEvent += DrawPainShare;
		}

		public override void Update(NPC npc, ref int buffIndex)
		{
			Lighting.AddLight(npc.Center, new Vector3(0.4f, 0.5f, 0.25f));

			if (Main.rand.NextBool(5))
				Dust.NewDustPerfect(npc.Center, ModContent.DustType<Dusts.Cinder>(), Vector2.Zero, 0, new Color(0.5f, 0.8f, 0.05f));
		}

		private void DrawPainShare(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (Inflicted(npc))
			{
				var tex = Assets.Items.Haunted.SpiritSealBuff.Value;
				var glow = Assets.Keys.GlowAlpha.Value;

				spriteBatch.Draw(tex, npc.Center - Main.screenPosition, null, new Color(0.4f, 0.5f, 0.25f) * 0.8f, 0, tex.Size() / 2f, 2f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f, 0, 0);
				spriteBatch.Draw(glow, npc.Center - Main.screenPosition, null, new Color(0.3f, 0.5f, 0.2f, 0.0f) * 0.8f, 0, glow.Size() / 2f, 0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f, 0, 0);
			}
		}

		private void ShareItemHit(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			if (Inflicted(npc))
			{
				foreach (NPC n in Main.npc.Where(n => n.active && n.HasBuff<SpiritSealDamageShare>()))
				{
					n.SimpleStrikeNPC(damageDone / 2, 0);
				}
			}
		}

		private void ShareProjHit(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (Inflicted(npc))
			{
				foreach (NPC n in Main.npc.Where(n => n.active && n.HasBuff<SpiritSealDamageShare>()))
				{
					n.SimpleStrikeNPC(damageDone / 2, 0);
				}
			}
		}
	}
}