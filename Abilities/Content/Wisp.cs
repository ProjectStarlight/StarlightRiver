using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Dusts;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Void = StarlightRiver.Dusts.Void;

namespace StarlightRiver.Abilities.Content
{
    public class Wisp : Ability
    {
        public override string Texture => "StarlightRiver/Pickups/Faeflame";
        public override bool Available => base.Available && User.Stamina > 1;

        private bool safe => User.Stamina > 1 / 60f;

        public override void OnActivate()
        {
            for (int k = 0; k <= 50; k++)
            {
                Dust.NewDust(Player.Center - new Vector2(Player.height / 2, Player.height / 2), Player.height, Player.height, DustType<Gold2>(), Main.rand.Next(-20, 20), Main.rand.Next(-20, 20), 0, default, 1.2f);
            }
        }

        public override void UpdateActive()
        {
            Player.maxFallSpeed = 999;
            Player.gravity = 0;
            Player.velocity = Vector2.Normalize(new Vector2
                (
                Main.screenPosition.X + Main.mouseX - Player.Hitbox.Center.X,
                Main.screenPosition.Y + Main.mouseY - Player.Hitbox.Center.Y
                )) * 5 + new Vector2(0.25f, 0.25f);

            const int size = 14;

            // Set dimensions to 14x14
            if (Player.Hitbox.Width != Player.Hitbox.Height && Player.Hitbox.Width != size)
            Player.Hitbox = InflateTo(Player.Hitbox, size, size);

            Lighting.AddLight(Player.Center, 0.15f, 0.15f, 0f);

            // If it's safe and the player wants to, sure
            if (safe && StarlightRiver.Instance.AbilityKeys.Get<Wisp>().Current)
                User.Stamina -= 1 / 60f;
            // Ok abort
            else
                AttemptDeactivate();

            if (Active)
                UpdateEffects();
        }

        protected virtual void UpdateEffects()
        {
            int type = safe ? DustType<Gold>() : DustType<Void>();
            for (int k = 0; k <= 2; k++)
            {
                Dust.NewDust(Player.Center - new Vector2(4, 4), 8, 8, type);
            }
        }

        private void AttemptDeactivate()
        {
            bool canExit = SafeExit(out Vector2 safeSpot);
            if (canExit)
            {
                Deactivate();
                Player.TopLeft = safeSpot;
            }
            else if (!safe) SquishDamage();
        }

        private void SquishDamage()
        {
            //if (timer-- <= 0)
            //{
            //    Player.lifeRegen = 0;
            //    Player.lifeRegenCount = 0;
            //    Player.statLife -= 5;
            //    if (Player.statLife <= 0)
            //    {
            //        Player.KillMe(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(Player.name + " couldn't maintain their form"), 0, 0);
            //    }
            //    Main.PlaySound(SoundID.NPCHit13, Player.Center);
            //}
            // TODO make this a buff?
        }

        private static Rectangle InflateTo(Rectangle rectangle, int width, int height)
        {
            rectangle.X -= (width - rectangle.Width) / 2;
            rectangle.Y -= (height - rectangle.Height) / 2;
            rectangle.Width = width;
            rectangle.Height = height;
            return rectangle;
        }

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            layers.ForEach(p => p.visible = false);
        }

        public override void OnExit()
        {
            Player.Hitbox = InflateTo(Player.Hitbox, Player.defaultWidth, Player.defaultHeight);
            Player.direction = Math.Sign(Player.velocity.X);

            for (int k = 0; k <= 30; k++)
            {
                Dust.NewDust(Player.Center - new Vector2(Player.height / 2, Player.height / 2), Player.height, Player.height, DustType<Gold2>(), Main.rand.Next(-5, 5), Main.rand.Next(-5, 5), 0, default, 1.2f);
            }
        }

        public bool SafeExit(out Vector2 topLeft)
        {
            int i = (int)(Player.Left.X / 16);
            int j = (int)(Player.Top.Y / 16);
            for (int x = i - 1; x <= i + 1; x++)
            {
                for (int y = j - 2; y <= j + 2; y++)
                {
                    bool safe = !Collision.SolidTiles(x, x + 1, y, y + 2);
                    if (safe)
                    {
                        topLeft = new Vector2(x, y) * 16;
                        return true;
                    }
                }
            }
            topLeft = default;
            return false;
        }

        public override bool HotKeyMatch(TriggersSet triggers, AbilityHotkeys abilityKeys)
        {
            return abilityKeys.Get<Wisp>().Current;
        }
    }
}