using StarlightRiver.Core.Systems.ExposureSystem;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Buffs
{
	internal class Psychosis : StackableBuff
	{
		public override string Name => "Psychosis";

		public override string DisplayName => "Psychosis";

		public override string Tooltip => "Deals 4 damage per second and increases expsoure by 1%";

		public override string Texture => AssetDirectory.Buffs + Name;

		public override bool Debuff => true;

		public override int MaxStacks => 20;

		public override BuffStack GenerateDefaultStack(int duration)
		{
			var stack = new BuffStack
			{
				duration = duration
			};
			return stack;
		}

		public override void PerStackEffectsNPC(NPC npc, BuffStack stack)
		{
			npc.lifeRegen -= 8;
			npc.GetGlobalNPC<ExposureNPC>().ExposureMultAll += 0.01f;
		}

		public override void PerStackEffectsPlayer(Player player, BuffStack stack)
		{
			player.lifeRegen -= 8;
			player.GetModPlayer<ExposurePlayer>().exposureMult += 0.01f;
		}
	}
}