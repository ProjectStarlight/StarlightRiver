using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Buffs
{
	class Squash : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Buffs + name;
            return true;
        }

        public override void SetDefaults()
        {
            DisplayName.SetDefault("Pancaked");
            Description.SetDefault("You're flat now.");
            Main.debuff[Type] = true;
        }

        public override void Update(Player Player, ref int buffIndex)
        {
            Player.velocity.X *= 0.9f;
        }
    }
}
