using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Packets;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using Terraria.GameContent.Bestiary;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Pickups
{
	internal abstract class AbilityPickup : Dummy
	{
		/// <summary>
		/// Indicates if the pickup should be visible in-world. Should be controlled using clientside vars.
		/// </summary>
		protected bool Visible => CanPickup(Main.LocalPlayer);

		public virtual bool Fancy => true;

		public override bool DoesCollision => true;

		public virtual Asset<Texture2D> Texture => Assets.Default;

		public AbilityPickup(int validType) : base(validType, 16, 16) { }

		public virtual void SafeSetDefaults() { }

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

		public override void Update()
		{
			StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>(); //the local Player since ability pickup visuals are clientside

			if (Visible)
			{
				Visuals();

				if (!Fancy)
					return;

				if (Vector2.Distance(Main.LocalPlayer.Center, Center) < 200f) //music handling
				{
					for (int k = 0; k < Main.musicFade.Length; k++)
					{
						if (k == Main.curMusic)
							Main.musicFade[k] = Vector2.Distance(Main.LocalPlayer.Center, Center) / 200f;
					}
				}
			}

			Main.blockInput = false;

			if (mp.pickupTarget == this)
			{
				PickupVisuals(mp.pickupTimer); //if the Player is picking this up, clientside only also
				Main.blockInput = true;
				// TODO sync it so they're not floating? idk
			}
		}

		public override void Collision(Player player)
		{
			StarlightPlayer mp = player.GetModPlayer<StarlightPlayer>();

			if (CanPickup(player))
			{
				PickupEffects(player);
				mp.pickupTarget = this;

				var packet = new AbilityProgress(player.whoAmI, player.GetHandler());
				packet.Send();
			}
		}

		public override void PostDraw(Color lightColor)
		{
			if (Visible)
			{
				Tile a = Parent;
				int b = width;
				Texture2D tex = Texture.Value;
				Texture2D glow = Assets.Keys.GlowAlpha.Value;

				Vector2 pos = Center - Main.screenPosition + new Vector2(0, (float)Math.Sin(StarlightWorld.visualTimer) * 5);
				Main.spriteBatch.Draw(tex, pos, tex.Frame(), Color.White, 0, tex.Size() / 2, 1, 0, 0);

				Color color = GlowColor;
				color.A = 0;

				Main.spriteBatch.Draw(glow, pos, glow.Frame(), color * 0.3f, 0, glow.Size() / 2, 1, 0, 0);
				Main.spriteBatch.Draw(glow, pos, glow.Frame(), color * 0.5f, 0, glow.Size() / 2, 0.6f, 0, 0);
			}
		}
	}
}