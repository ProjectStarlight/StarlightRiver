using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.ExposureSystem;
using System.Linq;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	class TempleLensUpgrade : SmartAccessory
	{
		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public TempleLensUpgrade() : base("Truestrike Lens", "Critical strikes expose enemies near the struck enemy\nExposed enemies have 20% increased Exposure on first hit\n+4% critical strike chance\n+10% critical strike damage") { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Orange;
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetCritChance(DamageClass.Generic) += 4;
			Player.GetModPlayer<CritMultiPlayer>().AllCritMult += 0.1f;
		}

		public override void Load()
		{
			StarlightPlayer.OnHitNPCEvent += ModifyHurtLens;
			StarlightProjectile.OnHitNPCEvent += ModifyProjectileLens;
		}

		private void ModifyHurtLens(Player Player, Item Item, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (Equipped(Player) && info.Crit)
				ApplyExposed(target);
		}

		private void ModifyProjectileLens(Projectile Projectile, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (Equipped(Main.player[Projectile.owner]) && info.Crit)
				ApplyExposed(target);
		}

		private void ApplyExposed(NPC target)
		{
			var nearby = Main.npc.Where(n => n.active && n != target && n.Distance(target.Center) < 250 && !n.dontTakeDamage && !n.immortal && !n.friendly).ToList();
			nearby.ForEach(n => n.AddBuff(ModContent.BuffType<Exposed>(), 200));
			nearby.ForEach(n => Exposed.CreateDust(n, false));
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<TempleLens>());
			recipe.AddIngredient(ItemType<Moonstone.MoonstoneBarItem>(), 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	class Exposed : SmartBuff
	{
		public override string Texture => AssetDirectory.Debug;

		public Exposed() : base("Exposed", "Taking extra damage!", true) { }

		public override void Load()
		{
			StarlightNPC.OnHitByItemEvent += DelBuffItem;
			StarlightNPC.OnHitByProjectileEvent += DelBuffProjectile;
		}

		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.GetGlobalNPC<ExposureNPC>().ExposureMultAll += 0.20f;
		}

		private void DelBuffProjectile(NPC NPC, Projectile Projectile, NPC.HitInfo info, int damageDone)
		{
			if (Inflicted(NPC) && !info.Crit)
			{
				NPC.DelBuff(NPC.FindBuffIndex(Type));
				CreateDust(NPC, true);
			}
		}

		private void DelBuffItem(NPC NPC, Player Player, Item Item, NPC.HitInfo info, int damageDone)
		{
			if (Inflicted(NPC) && !info.Crit)
			{
				NPC.DelBuff(NPC.FindBuffIndex(Type));
				CreateDust(NPC, true);
			}
		}

		public static void CreateDust(NPC NPC, bool hit)
		{
			for (int i = 0; i < 14; i++)
			{
				if (hit)
				{
					Vector2 dir = Main.rand.NextVector2CircularEdge(1, 1);
					Dust.NewDustPerfect(NPC.Center + dir * 15, ModContent.DustType<GlowLineFast>(), dir * Main.rand.NextFloat(6), 0, Color.Gold, 0.75f);
				}

				Vector2 dir2 = Main.rand.NextVector2CircularEdge(1, 1);
				Dust.NewDustPerfect(NPC.Center + dir2 * 15, ModContent.DustType<Glow>(), dir2 * Main.rand.NextFloat(6), 0, Color.Gold, 0.55f);
			}
		}
	}
}