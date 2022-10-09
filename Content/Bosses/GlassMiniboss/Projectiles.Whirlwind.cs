using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class Whirlwind : ModProjectile
	{
		public override string Texture => AssetDirectory.Glassweaver + Name;

		public ref float Timer => ref Projectile.ai[0];

		public NPC Parent => Main.npc[(int)Projectile.ai[1]];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Spinning Blades");
		}

		public override void SetDefaults()
		{
			Projectile.width = 200;
			Projectile.height = 100;
			Projectile.hostile = true;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			if (!Parent.active || Parent.type != NPCType<Glassweaver>())
				Projectile.Kill();

			Timer++;

			Projectile.Center = Parent.Center;
			Projectile.velocity = Parent.velocity;

			if (Timer < 50)
				Projectile.rotation = MathHelper.Lerp(Projectile.rotation, Projectile.velocity.ToRotation(), 0.5f);

			Lighting.AddLight(Projectile.Center, Glassweaver.GlassColor.ToVector3());

			if (Timer > 70)
				Projectile.Kill();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Rectangle slashBox = projHitbox;
			slashBox.Inflate(30, 10);

			return Timer > 10 && slashBox.Intersects(targetHitbox);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Math.Abs(Parent.velocity.X - oldVelocity.X) > 0)
				Parent.velocity.X = -oldVelocity.X;

			if (Math.Abs(Parent.velocity.Y - oldVelocity.Y) > 0)
				Parent.velocity.Y = -oldVelocity.Y;

			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Asset<Texture2D> spinTexture = Request<Texture2D>(Texture);

			Color glowColor = Glassweaver.GlassColor * Utils.GetLerpValue(0, 15, Timer, true) * Utils.GetLerpValue(70, 25, Timer, true) * 0.5f;
			glowColor.A = 0;
			Main.EntitySpriteDraw(spinTexture.Value, Parent.Center + Main.rand.NextVector2Circular(16, 1) - Main.screenPosition, null, glowColor, Projectile.rotation, spinTexture.Size() * 0.5f, Projectile.scale * new Vector2(2f, 2f), 0, 0);
			Main.EntitySpriteDraw(spinTexture.Value, Parent.Center + Main.rand.NextVector2Circular(16, 1) - Main.screenPosition, null, glowColor, Projectile.rotation, spinTexture.Size() * 0.5f, Projectile.scale * new Vector2(2f, 2f), 0, 0);
			Main.EntitySpriteDraw(spinTexture.Value, Parent.Center + Main.rand.NextVector2Circular(10, 1) - Main.screenPosition, null, glowColor, Projectile.rotation, spinTexture.Size() * 0.5f, Projectile.scale * new Vector2(2f, 2f), 0, 0);

			return false;
		}
	}
}
