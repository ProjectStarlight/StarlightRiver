using StarlightRiver.Core;
using Terraria;

namespace StarlightRiver.Content.Buffs
{
	public class ForestTonic : SmartBuff
    {
        public ForestTonic() : base("Forest Tonic", "Immunity to poision\nSlowly regenerate life", false) { }

        public override string Texture => AssetDirectory.Buffs + "ForestTonic";

        public override void Update(Player Player, ref int buffIndex)
        {
            Player.lifeRegen += 2;
            if (Player.poisoned) Player.poisoned = false;
        }
    }
}