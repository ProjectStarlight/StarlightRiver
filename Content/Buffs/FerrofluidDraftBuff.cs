using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Buffs
{
	public class FerrofluidDraftBuff : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Buffs + name;
            return true;
        }

        public override void SetDefaults()
        {
            DisplayName.SetDefault("Ferrofluid Draft");
            Description.SetDefault("Nearby Items gravitate towards you");
        }

        public override void Update(Player Player, ref int buffIndex)
        {
            for (int k = 0; k < Main.Item.Length; k++)
            {
                if (Vector2.Distance(Main.Item[k].Center, Player.Center) <= 800)
                {
                    Main.Item[k].velocity += Vector2.Normalize(Player.Center - Main.Item[k].Center) * 3f;
                    Main.Item[k].velocity = Vector2.Normalize(Main.Item[k].velocity) * 12;
                }
            }
        }
    }
}