using Microsoft.Xna.Framework;
using StarlightRiver.Dusts;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities
{
    [DataContract]
    // Second Infusion to be made
    public class WispWIP : Wisp
    {
        [DataMember] private new bool exit = false;

        public WispWIP(Player player) : base(player)
        {
        }

        public override void OnCast()
        {
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();

            Active = true;
            Timer = mp.StatStamina * 60 + (int)((1 - mp.StatStaminaRegen / (float)mp.StatStaminaRegenMax) * 60) - 1; //allows the use of fractional stamina

            //Sets the player's stamina if full to prevent spamming the ability to abuse it and to draw the UI correctly.
            if (mp.StatStamina == mp.StatStaminaMax)
            {
                mp.StatStamina--;
                mp.StatStaminaRegen = 1;
            }

            for (int k = 0; k <= 50; k++)
            {
                Dust.NewDust(player.Center - new Vector2(player.height / 2, player.height / 2), player.height, player.height, DustType<Gold2>(), Main.rand.Next(-20, 20), Main.rand.Next(-20, 20), 0, default, 1.2f);
            }
        }

        public override void InUse()
        {
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();

            Timer--;
            player.maxFallSpeed = 999;
            player.gravity = 0;
            player.velocity = Vector2.Normalize(new Vector2
                (
                Main.screenPosition.X + Main.mouseX - player.Hitbox.Center.X,
                Main.screenPosition.Y + Main.mouseY - player.Hitbox.Center.Y
                )) * 5 + new Vector2(0.25f, 0.25f);

            player.Hitbox = new Rectangle(player.Hitbox.X - 7 + 7, player.Hitbox.Y + 21 + 7, 14, 14);

            Lighting.AddLight(player.Center, new Vector3(0.15f, 0.15f, 0f));

            if (Timer % 60 == 0 && Timer > 0) { mp.StatStamina--; }
            else if (Timer > 0)
            {
                mp.StatStaminaRegen = (int)((1 - (Timer + 60) % 60 / 60f) * mp.StatStaminaRegenMax);
            }
            else { mp.StatStaminaRegen = mp.StatStaminaRegenMax; }

            if (StarlightRiver.Wisp.JustReleased)
            {
                exit = true;
            }

            if (exit || (mp.StatStamina < 1 && mp.StatStaminaRegen == mp.StatStaminaRegenMax))
            {
                OnExit();
            }
        }

        public override void UseEffects()
        {
            if (Timer > -1)
            {
                for (int k = 0; k <= 2; k++)
                {
                    Dust.NewDust(player.Center - new Vector2(4, 4), 8, 8, DustType<Gold>());
                }
            }
            else
            {
                for (int k = 0; k <= 2; k++)
                {
                    Dust.NewDust(player.Center - new Vector2(4, 4), 8, 8, DustType<Void>());
                }
            }
        }

        public override void OnExit()
        {
            if (TestExit())
            {
                Timer = 0;
                exit = false;
                player.velocity.X = 0;
                player.velocity.Y = 0;

                for (int k = 0; k <= 30; k++)
                {
                    Dust.NewDust(player.Center - new Vector2(player.height / 2, player.height / 2), player.height, player.height, DustType<Gold2>(), Main.rand.Next(-5, 5), Main.rand.Next(-5, 5), 0, default, 1.2f);
                }
                Active = false;
            }
            else if (Timer < 0)
            {
                player.statLife -= 2;
                if (player.statLife <= 0)
                {
                    player.KillMe(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(player.name + " couldn't maintain their form"), 0, 0);
                }
                if (Timer % 10 == 0) { Main.PlaySound(SoundID.PlayerHit, player.Center); }
            }
        }
    }
}