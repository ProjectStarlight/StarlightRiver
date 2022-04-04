using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
	public class Stone : ModDust
    {
        public override string Texture => AssetDirectory.Dust + Name;
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = false;
            dust.noLight = false;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity.Y += 0.2f;
            if (Main.tile[(int)dust.position.X / 16, (int)dust.position.Y / 16].HasTile && Main.tile[(int)dust.position.X / 16, (int)dust.position.Y / 16].collisionType == 1)
                dust.velocity *= -0.5f;

            dust.rotation = dust.velocity.ToRotation();
            dust.scale *= 0.99f;
            if (dust.scale < 0.2f)
                dust.active = false;
            return false;
        }
    }

    public class Grass : ModDust
    {
        public override string Texture => AssetDirectory.Dust + Name;
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = false;
            dust.noLight = false;
            dust.color = new Color(28, 216, 94);
        }

        public override bool Update(Dust dust)
        {
            Player Player = Main.LocalPlayer;
            dust.position += dust.velocity;
            dust.velocity.Y += 0.1f;

            if (Player.ZoneSnow) dust.color = new Color(250, 250, 255);
            if (Player.ZoneDesert) dust.color = new Color(241, 228, 131);
            if (Player.ZoneJungle) dust.color = new Color(143, 215, 29);
            if (Player.ZoneGlowshroom) dust.color = new Color(63, 90, 231);
            if (Player.ZoneCorrupt) dust.color = new Color(186, 177, 243);
            if (Player.ZoneCrimson) dust.color = new Color(208, 80, 80);
            if (Player.ZoneHallow) dust.color = new Color(98, 213, 247);

            dust.rotation += 0.1f;
            dust.scale *= 0.98f;
            if (dust.scale < 0.2f)
                dust.active = false;
            return false;
        }
    }
}