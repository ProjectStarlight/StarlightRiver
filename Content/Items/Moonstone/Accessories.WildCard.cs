using StarlightRiver.Content.Items.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Items.Moonstone
{
	internal class WildCard : CursedAccessory
	{
		public override string Texture => AssetDirectory.Debug;

		public override void Load()
		{
			StarlightPlayer.ModifyHitNPCEvent += RandomizeType;
			StarlightPlayer.ModifyHitNPCWithProjEvent += RandomizeTypeProj;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Wild card");
			Tooltip.SetDefault("Cursed : All damage you deal has a randomly assaigned damage type\nPicks between melee, ranged, magic, and summon damage");
		}

		private void RandomizeType(Player player, Item Item, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (Equipped(player))
				modifiers.ModifyHitInfo += Randomize;
		}

		private void RandomizeTypeProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (Equipped(player))
			{
				var type = DamageClass.Melee;
				switch (Main.rand.Next(4))
				{
					case 0: type = DamageClass.Melee; break;
					case 1: type = DamageClass.Ranged; break;
					case 2: type = DamageClass.Magic; break;
					case 3: type = DamageClass.Summon; break;
				}

				proj.DamageType = type;
			}
		}

		private void Randomize(ref NPC.HitInfo info)
		{
			var type = DamageClass.Melee;
			switch(Main.rand.Next(4))
			{
				case 0: type = DamageClass.Melee; break;
				case 1: type = DamageClass.Ranged; break;
				case 2: type = DamageClass.Magic; break;
				case 3: type = DamageClass.Summon; break;
			}

			info.DamageType = type;
		}
	}
}
