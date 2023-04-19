using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Misc
{
	public class DisinfectantWipes : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public DisinfectantWipes() : base("Disinfectant Wipes", "Critical strikes have a 10% chance to reduce all debuff durations by 3 seconds\nDoes not affect potion sickness debuffs") { }

		public override void Load()
		{
			StarlightPlayer.OnHitNPCEvent += OnHitNPC;
			StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProj;
		}

		public override void Unload()
		{
			StarlightPlayer.OnHitNPCEvent -= OnHitNPC;
			StarlightPlayer.OnHitNPCWithProjEvent -= OnHitNPCWithProj;
		}

		private void OnHitNPC(Player Player, Item Item, NPC target, NPC.HitInfo info, int damageDone)
		{
			OnHit(Player, info.Crit);
		}

		private void OnHitNPCWithProj(Player Player, Projectile proj, NPC target, NPC.HitInfo info, int damageDone)
		{
			OnHit(Player, info.Crit);
		}

		private void OnHit(Player Player, bool crit)
		{
			if (Equipped(Player) && crit && Main.rand.NextFloat() < 0.1f)
				ReduceDebuffDurations(Player);
		}

		public static void ReduceDebuffDurations(Player Player)
		{
			for (int i = 0; i < Player.MaxBuffs; i++)
			{
				if (Helper.IsValidDebuff(Player, i))
					Player.buffTime[i] -= 180;
			}
		}
	}
}