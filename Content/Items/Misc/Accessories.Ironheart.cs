using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Items.SteampunkSet;
using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class Ironheart : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public Ironheart() : base("Ironheart", "Melee damage generates decaying barrier and defense") { }

		public override void Load()
		{
			StarlightPlayer.OnHitNPCEvent += OnHit;
			StarlightProjectile.OnHitNPCEvent += OnHitProjectile;
		}

		public override void Unload()
		{
			StarlightPlayer.OnHitNPCEvent -= OnHit;
			StarlightProjectile.OnHitNPCEvent -= OnHitProjectile;
		}

		private void OnHitProjectile(Projectile Projectile, NPC target, NPC.HitInfo info, int damageDone)
		{
			Player Player = Main.player[Projectile.owner];

			if (Projectile.DamageType == DamageClass.Melee && Equipped(Player))
				Player.GetModPlayer<StarlightPlayer>().SetIronHeart(damageDone);
		}

		private void OnHit(Player Player, Item Item, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (Equipped(Player))
				Player.GetModPlayer<StarlightPlayer>().SetIronHeart(damageDone);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<AncientGear>(), 5);
			recipe.AddIngredient(ItemID.DemoniteBar, 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<AncientGear>(), 5);
			recipe.AddIngredient(ItemID.CrimtaneBar, 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class IronheartBuff : ModBuff
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ironheart");
			Description.SetDefault("you have decaying extra barrier and defense");
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
		}

		public override void Update(Player Player, ref int buffIndex)
		{
			StarlightPlayer mp = Player.GetModPlayer<StarlightPlayer>();

			float level;

			if (mp.ironheartTimer < 1)
			{
				mp.ironheartTimer += 0.01f;
				level = mp.ironheartLevel;
				Player.GetModPlayer<BarrierPlayer>().dontDrainOvercharge = true;
			}
			else
			{
				mp.ironheartTimer *= 1.02f;
				level = mp.ironheartLevel + 1 - mp.ironheartTimer;
				Player.GetModPlayer<BarrierPlayer>().overchargeDrainRate = (int)(2.2f * mp.ironheartTimer);
			}

			//Main.NewText(level + " | " + mp.ironheartTimer);
			//Main.NewText(level);
			if (level < 0.001f)
			{
				Player.ClearBuff(Type);
				mp.ResetIronHeart();
			}

			Player.statDefense += (int)level;

			Player.buffTime[buffIndex] = (int)level * 60;//visual time value
		}
	}
}

namespace StarlightRiver.Core
{
	public partial class StarlightPlayer : ModPlayer
	{
		public const int IronheartMaxLevel = 15;
		public const int IronheartMaxDamage = 75;

		public int ironheartLevel = 0;
		public float ironheartTimer = 0;

		public void SetIronHeart(int damage)
		{
			shouldSendHitPacket = true;

			int buffType = ModContent.BuffType<IronheartBuff>();

			if (!Player.HasBuff(buffType))
				ResetIronHeart();

			int level = Math.Min(damage, IronheartMaxDamage) / 12;

			if (level > 0 && ironheartLevel < IronheartMaxLevel)//if level was increased
			{
				Player.GetModPlayer<BarrierPlayer>().barrier += ((ironheartLevel += level) > IronheartMaxLevel ?
					level - (ironheartLevel - IronheartMaxLevel) : level) * 2;

				ironheartLevel = ironheartLevel > IronheartMaxLevel ? IronheartMaxLevel : ironheartLevel;//caps value
				Player.AddBuff(buffType, 1);
			}
		}

		public void ResetIronHeart()
		{
			ironheartLevel = 0;
			ironheartTimer = 0;
		}
	}
}