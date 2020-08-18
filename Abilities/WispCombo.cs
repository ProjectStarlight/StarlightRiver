using Microsoft.Xna.Framework;
using StarlightRiver.Dusts;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities
{
    [DataContract]
    // Combo Infusion to be made
    public class WispCombo : WispHoming
    {
        [DataMember] private new bool exit = false;

        public WispCombo(Player player) : base(player)
        {
        }

        public override void OnCast()
        {
            AbilityHandler mp = User.GetModPlayer<AbilityHandler>();

            Active = true;
            Timer = mp.StatStamina * 60 + (int)((1 - mp.StatStaminaRegen / (float)mp.StatStaminaRegenMax) * 60) - 1; //allows the use of fractional stamina

            //Sets the player's stamina if full to prevent spamming the ability to abuse it and to draw the UI correctly.
            if (mp.StatStamina == mp.StaminaMax)
            {
                mp.StatStamina--;
                mp.StatStaminaRegen = 1;
            }

            for (int k = 0; k <= 50; k++)
            {
                Dust.NewDust(User.Center - new Vector2(User.height / 2, User.height / 2), User.height, User.height, DustType<Gold2>(), Main.rand.Next(-20, 20), Main.rand.Next(-20, 20), 0, Color.Red, 1.2f);
            }
        }

        public override void UpdateActive()
        {
            AbilityHandler mp = User.GetModPlayer<AbilityHandler>();

            Timer--;
            User.maxFallSpeed = 999;
            User.gravity = 0;
            User.velocity = Vector2.Normalize(new Vector2
                (
                Main.screenPosition.X + Main.mouseX - User.Hitbox.Center.X,
                Main.screenPosition.Y + Main.mouseY - User.Hitbox.Center.Y
                )) * 5 + new Vector2(0.25f, 0.25f);

            User.Hitbox = new Rectangle(User.Hitbox.X - 7 + 7, User.Hitbox.Y + 21 + 7, 14, 14);

            Lighting.AddLight(User.Center, new Vector3(0.15f, 0.15f, 0f));

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

        public override void UpdateEffects()
        {
            if (Timer > -1)
            {
                for (int k = 0; k <= 2; k++)
                {
                    Dust.NewDust(User.Center - new Vector2(4, 4), 8, 8, DustType<Gold>());
                }
            }
            else
            {
                for (int k = 0; k <= 2; k++)
                {
                    Dust.NewDust(User.Center - new Vector2(4, 4), 8, 8, DustType<Void>());
                }
            }
        }

        public override void OnExit()
        {
            if (TestExit())
            {
                Timer = 0;
                exit = false;
                User.velocity.X = 0;
                User.velocity.Y = 0;

                for (int k = 0; k <= 30; k++)
                {
                    Dust.NewDust(User.Center - new Vector2(User.height / 2, User.height / 2), User.height, User.height, DustType<Gold2>(), Main.rand.Next(-5, 5), Main.rand.Next(-5, 5), 0, default, 1.2f);
                }
                Active = false;
            }
            else if (Timer < 0)
            {
                User.statLife -= 2;
                if (User.statLife <= 0)
                {
                    User.KillMe(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(User.name + " couldn't maintain their form"), 0, 0);
                }
                if (Timer % 10 == 0) { Main.PlaySound(SoundID.PlayerHit, User.Center); }
            }
        }

        public new bool TestExit()
        {
            int cleartiles = 0;
            for (int x2 = (int)(User.position.X / 16); x2 <= (int)(User.position.X / 16) + 1; x2++)
            {
                for (int y2 = (int)(User.position.Y / 16) - 2; y2 <= (int)(User.position.Y / 16); y2++)
                {
                    if (Main.tile[x2, y2].collisionType == 0) { cleartiles++; }
                }
            }
            return cleartiles == 6;
        }
    }
}