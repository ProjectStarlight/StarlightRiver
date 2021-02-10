using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Dusts
{
    public class Piss : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "GasChaos";
            return true;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.color = new Color(Main.rand.Next(100, 255), Main.rand.Next(100, 255), Main.rand.Next(100, 255));
            dust.noGravity = true;
            dust.noLight = false;
            dust.alpha = 180;
        }

        public override bool Update(Dust dust)
        {
            dust.velocity = new Vector2(Main.rand.Next(-4, 4), Main.rand.Next(-4, 4));
            dust.position += dust.velocity;
            dust.rotation += dust.velocity.X;
            Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), 0.05f, 0.15f, 0.2f);
            dust.scale += 0.03f;
            if (dust.scale > 3.0f)
                dust.active = false;
            return false;
        }
    }
}