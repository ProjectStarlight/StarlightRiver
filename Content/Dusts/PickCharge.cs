using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.UndergroundTemple;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
	public class PickCharge : ModDust
    {
        public override string Texture => AssetDirectory.Dust + "FireDust";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is int && Main.player[(int)dust.customData].active)
            {
                Player Player = Main.player[(int)dust.customData];
                dust.position = Player.Center + new Vector2(0, Player.gfxOffY) + dust.velocity;
                if (!Main.mouseRight || !(Player.HeldItem.ModItem is TemplePick)) dust.active = false; //RIP multiPlayer TODO: Make this not gay
            }
            else dust.active = false;

            dust.rotation += 0.1f;
            return false;
        }
    }
}