using StarlightRiver.Content.Buffs;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace StarlightRiver.Content.Items.Permafrost
{
	internal class UrnFreeze : SmartBuff
	{
		public override string Texture => AssetDirectory.PermafrostItem + Name;

		public UrnFreeze() : base("Urn Freeze", "Greatly increased defense\nBut you cant move!", true, false) { }

		public override void Update(Player player, ref int buffIndex)
		{
			player.frozen = true;
			player.statDefense += 40;
		}
	}
}
