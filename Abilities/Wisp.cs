using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Dusts;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities
{
    [DataContract]
    public class Wisp : Ability
    {
        public bool exit = false;

        public Wisp(Player player) : base(1, player)
        {
        }

        public override Texture2D Texture => GetTexture("StarlightRiver/Pickups/Faeflame");

        public override void OnCast()
        {
            AbilityHandler mp = User.GetModPlayer<AbilityHandler>();

            Timer = mp.StatStamina * 60;

            for (int k = 0; k <= 50; k++)
            {
                Dust.NewDust(User.Center - new Vector2(User.height / 2, User.height / 2), User.height, User.height, DustType<Gold2>(), Main.rand.Next(-20, 20), Main.rand.Next(-20, 20), 0, default, 1.2f);
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

            if (Timer % 60 == 0 && Timer >= 0) { mp.StatStamina--; }

            if (StarlightRiver.Wisp.JustReleased)
            {
                exit = true;
            }

            if (exit || mp.StatStamina < 1)
            {
                OnExit();
            }
        }

        public override void UpdateEffects()
        {
            if (Timer > -10)
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
                User.Hitbox = new Rectangle((int)User.position.X, (int)User.position.Y - 16, 20, 42);

                for (int k = 0; k <= 30; k++)
                {
                    Dust.NewDust(User.Center - new Vector2(User.height / 2, User.height / 2), User.height, User.height, DustType<Gold2>(), Main.rand.Next(-5, 5), Main.rand.Next(-5, 5), 0, default, 1.2f);
                }
                Active = false;
            }
            else if (Timer < 0 && Timer % 10 == 0)
            {
                User.statLife -= 5;
                if (User.statLife <= 0)
                {
                    User.KillMe(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(User.name + " couldn't maintain their form"), 0, 0);
                }
                Main.PlaySound(SoundID.PlayerHit, User.Center);
            }
        }

        public bool TestExit()
        {
            int cleartiles = 0;
            for (int x2 = (int)(User.position.X / 16); x2 <= (int)(User.position.X / 16) + 1; x2++)
            {
                for (int y2 = (int)(User.position.Y / 16) - 3; y2 <= (int)(User.position.Y / 16) - 1; y2++)
                {
                    if (Main.tile[x2, y2].collisionType == 0) { cleartiles++; }
                }
            }
            return cleartiles >= 6;
        }
    }
}