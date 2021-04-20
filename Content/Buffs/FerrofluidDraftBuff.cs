using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

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
            Description.SetDefault("Nearby items gravitate towards you");
        }

        public override void Update(Player player, ref int buffIndex)
        {
            for (int k = 0; k < Main.item.Length; k++)
            {
                if (Vector2.Distance(Main.item[k].Center, player.Center) <= 800)
                {
                    Main.item[k].velocity += Vector2.Normalize(player.Center - Main.item[k].Center) * 3f;
                    Main.item[k].velocity = Vector2.Normalize(Main.item[k].velocity) * 12;
                }
            }
        }
    }
}