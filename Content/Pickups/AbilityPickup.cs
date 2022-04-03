using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Packets;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Pickups
{
	internal abstract class AbilityPickup : ModNPC, IDrawAdditive
    {
        /// <summary>
        /// Indicates if the pickup should be visible in-world. Should be controlled using clientside vars.
        /// </summary>
        private bool Visible => CanPickup(Main.LocalPlayer);

        public virtual bool Fancy => true;

        public sealed override void SetDefaults()
        {
            SafeSetDefaults();

            NPC.width = 32;
            NPC.height = 32;
            NPC.lifeMax = 2;
            NPC.damage = 1;
            NPC.dontTakeDamage = true;
            NPC.dontCountMe = true;
            NPC.immortal = true;
            NPC.noGravity = true;
            NPC.aiStyle = -1;
            NPC.friendly = false;
        }

        public virtual void SafeSetDefaults() { }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            crit = false;
            knockback = 0;
            damage = 0;
        }

        public override bool CheckActive() => false;

        public sealed override bool? CanBeHitByItem(Player Player, Item Item) => false;

        public sealed override bool? CanBeHitByProjectile(Projectile Projectile) => false;

        /// <summary>
        /// The clientside visual dust that this pickup makes when in-world
        /// </summary>
        public virtual void Visuals() { }

        /// <summary>
        /// The clientside visual dust taht this pickup makes when being picked up, relative to a timer.
        /// </summary>
        /// <param name="timer">The progression along the animation</param>
        public virtual void PickupVisuals(int timer) { }

        /// <summary>
        /// What happens to the Player internally when they touch the pickup. This is deterministically synced.
        /// </summary>
        /// <param name="Player"></param>
        public virtual void PickupEffects(Player Player) { }

        /// <summary>
        /// If the Player should be able to pick this up or not.
        /// </summary>
        public abstract bool CanPickup(Player Player);

        public virtual Color GlowColor => Color.White;

        public sealed override void AI()
        {
            StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>(); //the local Player since ability pickup visuals are clientside

            if (Visible)
            {
                Visuals();

                if (!Fancy) 
                    return;

                if (Vector2.Distance(Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2), NPC.Center) <= Main.screenWidth / 2 + 100) //shader
                {
                    float timer = Math.Abs((float)Math.Sin(StarlightWorld.rottime));
                    Filters.Scene.Activate("Shockwave", NPC.Center).GetShader()
                        .UseProgress(Main.screenWidth / (float)Main.screenHeight)
                        .UseIntensity(500 + 200 * (timer))
                        .UseDirection(new Vector2(0.005f + timer * 0.03f, 1 * 0.008f - timer * 0.004f));
                }

                if (Vector2.Distance(Main.LocalPlayer.Center, NPC.Center) < 200f) //music handling
                {
                    for (int k = 0; k < Main.musicFade.Length; k++)
                    {
                        if (k == Main.curMusic) Main.musicFade[k] = Vector2.Distance(Main.LocalPlayer.Center, NPC.Center) / 200f;
                    }
                }
            }

            Main.blockInput = false;
            if (mp.PickupTarget?.whoAmI == NPC.whoAmI)
            {
                PickupVisuals(mp.PickupTimer); //if the Player is picking this up, clientside only also
                Main.blockInput = true;
                // TODO sync it so they're not floating? idk
            }
        }

        public sealed override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            StarlightPlayer mp = target.GetModPlayer<StarlightPlayer>();

            if (CanPickup(target) && target.Hitbox.Intersects(NPC.Hitbox))
            {
                PickupEffects(target);
                mp.PickupTarget = NPC;

                AbilityProgress packet = new AbilityProgress(target.whoAmI, target.GetHandler());
                packet.Send();
            }

            return false;
        }

		public sealed override bool? CanHitNPC(NPC target)
		{
            return false;
		}

		public sealed override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (Visible)
            {
                Texture2D tex = Request<Texture2D>(Texture).Value;
                Vector2 pos = NPC.Center - Main.screenPosition + new Vector2(0, (float)Math.Sin(StarlightWorld.rottime) * 5);
                spriteBatch.Draw(tex, pos, tex.Frame(), Color.White, 0, tex.Size() / 2, 1, 0, 0);
            }

            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/RiftCrafting/Glow0").Value;
                Vector2 pos = NPC.Center - Main.screenPosition + new Vector2(0, (float)Math.Sin(StarlightWorld.rottime) * 5);

                spriteBatch.Draw(tex, pos, tex.Frame(), GlowColor * 0.3f, 0, tex.Size() / 2, 1, 0, 0);
                spriteBatch.Draw(tex, pos, tex.Frame(), GlowColor * 0.5f, 0, tex.Size() / 2, 0.6f, 0, 0);
            }
        }
    }
}