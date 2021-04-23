using Microsoft.Xna.Framework;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Abilities;

namespace StarlightRiver.Content.Abilities.Faeflame
{
    public class Wisp : Ability
    {
        public override string Texture => "StarlightRiver/Assets/Abilities/Faeflame";
        public override float ActivationCostDefault => 0.5f;
        public override Color Color => new Color(255, 247, 126);

        public float Speed { get; set; }

        private const float diffTolerance = 5;

        private bool safe => User.Stamina > 0;

        private Rectangle oldHitbox;

        private const int size = 10;

        public override void Reset()
        {
            Speed = 5;
        }

        public override void OnActivate()
        {
            Player.mount.Dismount(Player);
            for (int k = 0; k <= 50; k++)
                Dust.NewDust(Player.Center - new Vector2(Player.height / 2, Player.height / 2), Player.height, Player.height, DustType<GoldWithMovement>(), Main.rand.Next(-20, 20), Main.rand.Next(-20, 20), 0, default, 1.2f);
        }

        public override void UpdateActive()
        {
            Player.gravity = 0;
            Player.maxFallSpeed = Speed;
            Player.frozen = true;

            // Local velocity update
            if (Player.whoAmI == Main.myPlayer)
            {
                Player.velocity = (Main.MouseScreen - Helper.ScreenSize / 2) / 20;

                if (Main.netMode != NetmodeID.SinglePlayer && (Player.position - Player.oldPosition).LengthSquared() > diffTolerance * diffTolerance)
                    // TODO let's not send every single control every 5 pixels someday
                    NetMessage.SendData(MessageID.PlayerControls);
            }
            if (Player.velocity.LengthSquared() > Speed * Speed)
                Player.velocity = Vector2.Normalize(Player.velocity) * Speed;

            // Why doesn't vanilla reset player width automatically??
            // I have to do this because of it
            Player.width = Player.defaultWidth;

            // Set dimensions to size
            oldHitbox = Player.Hitbox;
            //Player.position.X += Player.width - size; // also can't do this for some reason
            Player.position.Y += Player.height - size;
            Player.width = size;
            Player.height = size;

            Lighting.AddLight(Player.Center, 0.15f, 0.15f, 0f);

            UpdateEffects();

            // If it's safe and the player wants to continue, sure
            User.Stamina -= 1 / 60f;

            bool control = StarlightRiver.Instance.AbilityKeys.Get<Wisp>().Current;

            // Ok abort
            if (!safe || !control)
                AttemptDeactivate();
        }

        protected virtual void UpdateEffects()
        {
            int type = safe ? DustType<GoldNoMovement>() : DustType<Dusts.Void>();
            for (int k = 0; k <= 2; k++)
                Dust.NewDust(Player.Center - new Vector2(4, 4), 8, 8, type);
        }

        private void AttemptDeactivate()
        {
            bool canExit = SafeExit(out Vector2 safeSpot);
            if (canExit)
            {
                Deactivate();
                Player.TopLeft = safeSpot;
                Player.width = oldHitbox.Width;
                Player.height = oldHitbox.Height;
            }
            else if (!safe)
                Player.AddBuff(BuffType<Claustrophobia>(), 2);
        }

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            layers.ForEach(p => p.visible = false);
        }

        public override void OnExit()
        {
            if (Player.velocity.X != 0)
                Player.direction = Math.Sign(Player.velocity.X);

            for (int k = 0; k <= 30; k++)
                Dust.NewDust(Player.Center - new Vector2(Player.height / 2, Player.height / 2), Player.height, Player.height, DustType<GoldWithMovement>(), Main.rand.Next(-5, 5), Main.rand.Next(-5, 5), 0, default, 1.2f);
        }

        public bool SafeExit(out Vector2 topLeft)
        {
            var oldTopLeft = oldHitbox.TopLeft();
            topLeft = oldTopLeft;

            // If we can just exit where we are, then go ahead.
            if (!Collision.SolidCollision(oldTopLeft, oldHitbox.Width, oldHitbox.Height))
                return true;

            // Otherwise, search for a fitting space.
            for (var x = oldTopLeft.X - 16; x <= oldTopLeft.X + 16; x += 16)
                for (var y = oldTopLeft.Y - 16; y <= oldTopLeft.Y + 16; y += 16)
                {
                    topLeft = new Vector2(x, y);
                    if (!Collision.SolidCollision(topLeft, oldHitbox.Width, oldHitbox.Height))
                        return true;
                }
            return false;
        }

        public override bool HotKeyMatch(TriggersSet triggers, AbilityHotkeys abilityKeys)
        {
            return abilityKeys.Get<Wisp>().Current;
        }
    }
}