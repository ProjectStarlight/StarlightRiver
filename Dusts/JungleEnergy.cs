using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Dusts
{
    public class JungleEnergy : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.color.R = 200;
            dust.color.G = 255;
            dust.color.B = 80;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += 0.05f;
            dust.color.R--;

            if (dust.fadeIn == 0) dust.alpha -= 2;
            else dust.alpha += 2;

            if (dust.alpha <= 175) dust.fadeIn = 1;

            if (dust.alpha > 255)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class JungleEnergyFollow : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Dusts/JungleEnergy";
            return true;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.color.R = 140;
            dust.color.G = 255;
            dust.color.B = 80;
            dust.alpha = 150;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * (dust.alpha / 255f);
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is Player)
            {
                Player player = dust.customData as Player;
                Abilities.AbilityHandler mp = player.GetModPlayer<Abilities.AbilityHandler>();

                dust.position = player.Center + dust.velocity;
                dust.rotation += 6.28f / Abilities.Smash.ChargeTime;

                if (mp.smash.Active && mp.smash.Timer <= Abilities.Smash.ChargeTime) dust.alpha = (int)(mp.smash.Timer / (float)Abilities.Smash.ChargeTime * 255f);

                if (mp.smash.Timer > Abilities.Smash.ChargeTime || !mp.smash.Active) dust.alpha -= 10;
                if (dust.alpha <= 0) dust.active = false;
            }
            else
            {
                dust.active = false;
            }

            return false;
        }
    }
}