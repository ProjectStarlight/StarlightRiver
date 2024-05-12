using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	internal class Talismans : BaseTalisman<TalismansProjectile, TalismansBuff>
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Talismans");
			Tooltip.SetDefault("Sticks to enemies\nIgnites enemies with 6 stacks");
		}

		public override void SafeSetDefaults()
		{
			Item.useTime = 5;
			Item.useAnimation = 15;
			Item.reuseDelay = 20;
			Item.damage = 5;
			Item.mana = 4;
			Item.shootSpeed = 13;
		}
	}

	internal class TalismansProjectile : BaseTalismanProjectile<TalismansBuff>
	{
		public override string Texture => AssetDirectory.MiscItem + Name;
	}

	internal class TalismansBuff : BaseTalismanBuff
	{
		public override string Name => "TalismansBuff";

		public override string DisplayName => "Talismans";

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override bool Debuff => true;

		public TalismansBuff() : base()
		{
			threshold = 6;
		}

		public override BuffStack GenerateDefaultStack(int duration)
		{
			var stack = new BuffStack();
			stack.duration = duration;
			return stack;
		}

		public override void OnMaxStacks(NPC npc)
		{
			npc.AddBuff(BuffID.OnFire, 600);
			npc.SimpleStrikeNPC(20, 0, true, 0, DamageClass.Magic);
		}
	}
}
