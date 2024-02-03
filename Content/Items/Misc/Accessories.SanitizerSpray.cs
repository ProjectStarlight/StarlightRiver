using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Content.Items.Misc
{
	public class SanitizerSpray : SmartAccessory
	{
		// 30 Tiles.
		private const float transferRadius = 480;

		// 5 Seconds.
		private const int transferredBuffDuration = 300;

		public override string Texture => AssetDirectory.MiscItem + Name;

		public SanitizerSpray() : base("Sanitizer Spray", "Critical strikes have a 25% chance to transfer parts of your debuffs to nearby enemies") { }

		public override void Load()
		{
			StarlightPlayer.OnHitNPCEvent += OnHitNPCAccessory;
			StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProjAccessory;
		}

		public override void Unload()
		{
			StarlightPlayer.OnHitNPCEvent -= OnHitNPCAccessory;
			StarlightPlayer.OnHitNPCWithProjEvent -= OnHitNPCWithProjAccessory;
		}

		private void OnHit(Player Player, bool crit)
		{
			if (Equipped(Player) && crit && Main.rand.NextFloat() < 0.25f)
				TransferRandomDebuffToNearbyEnemies(Player);
		}

		private void OnHitNPCAccessory(Player Player, Item Item, NPC target, NPC.HitInfo info, int damageDone)
		{
			OnHit(Player, info.Crit);
		}

		private void OnHitNPCWithProjAccessory(Player Player, Projectile proj, NPC target, NPC.HitInfo info, int damageDone)
		{
			OnHit(Player, info.Crit);
		}

		public static void TransferRandomDebuffToNearbyEnemies(Player Player)
		{
			var activeDebuffIds = new List<int>();

			for (int i = 0; i < Player.MaxBuffs; i++)
			{
				if (Helper.IsValidDebuff(Player, i))
					activeDebuffIds.Add(Player.buffType[i]);
			}

			if (activeDebuffIds.Count() < 1)
				return;

			int type = activeDebuffIds[Main.rand.Next(activeDebuffIds.Count() - 1)];

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];

				if (npc.CanBeChasedBy() && Vector2.DistanceSquared(Player.Center, npc.Center) < transferRadius * transferRadius)
					npc.AddBuff(type, transferredBuffDuration);
			}
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(silver: 50);
		}
	}
}