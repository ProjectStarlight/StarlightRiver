using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Enums;

namespace StarlightRiver.Content.Items.Misc
{
    public abstract class BasePointer : ModItem
	{
		private readonly string ColorName;
		private readonly int ProjectileType;
		private readonly string TooltipText;
		private readonly ModRecipe[] Recipes;
		public BasePointer(string colorName, int projectileType, string tooltip = null, ModRecipe[] recipes = null)
        {
			ColorName = colorName;
			ProjectileType = projectileType;
			TooltipText = tooltip;
			Recipes = recipes;
		}

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(ColorName + " Laser Pointer");
			Tooltip.SetDefault("Do not stare directly into beam" + TooltipText == null ? "\n" + TooltipText : string.Empty);
		}

		public override void SetDefaults()
		{
			//item.damage = 1;
			item.noMelee = true;
			item.channel = true;
			item.mana = 0;
			item.width = 6;
			item.height = 18;
			item.useTime = 20;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.useAnimation = 20;
			item.noUseGraphic = true;
			item.shoot = ProjectileType;
			item.value = Item.sellPrice(silver: 3);
		}

		//public override void ModifyTooltips(List<TooltipLine> tooltips) => 
			//tooltips.RemoveAll(x => x.Name == "" || x.Name == "Damage" || x.Name == "CritChance" || x.Name == "Knockback" || x.Name == "Speed");//if this needs to deal damage

		//public override void AutoLightSelect (ref bool dryTorch, ref bool wetTorch, ref bool glowstick) =>
		//	glowstick = true;

        public override void AddRecipes()
        {
			if (Recipes != null)
				foreach (ModRecipe recipe in Recipes)
					recipe.AddRecipe();
		}
    }

	public abstract class BasePointerProjectile : HeldItemProjectile
    {
		private readonly string ColorName;
		private readonly Color Color;
		public BasePointerProjectile(string colorName, Color color) : base(10)
        {
			ColorName = colorName;
			Color = color;
        }

		public override string Texture => AssetDirectory.MiscItem + ColorName + "Pointer_Held";
	}



	public class BluePointer : BasePointer
    {
		public BluePointer() : base("Blue", ModContent.ProjectileType< BluePointerProj>(), "Test Tooltip") { }
    }	

	public class BluePointerProj : BasePointerProjectile
    {
		public BluePointerProj() : base("Blue", Color.Blue) { }
    }

	public class RedPointer : BasePointer
	{
		public RedPointer() : base("Red", ModContent.ProjectileType<RedPointerProj>(), "Test Tooltip") { }
	}

	public class RedPointerProj : BasePointerProjectile
	{
		public RedPointerProj() : base("Red", Color.Red) { }
	}

	//public class LaserBeamProjectile : ModProjectile//old projectile from old mod
	//{
	//	// The maximum charge value
	//	private const float MaxChargeValue = 50f;
	//	//The distance charge particle from the player center
	//	private const float MoveDistance = 60f;

	//	// The actual distance is stored in the ai0 field
	//	// By making a property to handle this it makes our life easier, and the accessibility more readable
	//	public float Distance
	//	{
	//		get { return projectile.ai[0]; }
	//		set { projectile.ai[0] = value; }
	//	}

	//	// The actual charge value is stored in the localAI0 field
	//	public float Charge
	//	{
	//		get { return projectile.localAI[0]; }
	//		set { projectile.localAI[0] = value; }
	//	}

	//	// Are we at max charge? With c#6 you can simply use => which indicates this is a get only property
	//	public bool AtMaxCharge { get { return Charge == MaxChargeValue; } }

	//	public override void SetDefaults()
	//	{
	//		projectile.width = 1;
	//		projectile.height = 1;
	//		projectile.friendly = true;
	//		projectile.penetrate = -1;
	//		projectile.tileCollide = false;
	//		//projectile.magic = true;
	//		projectile.hide = true;
	//	}

	//	public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
	//	{
	//		// We start drawing the laser if we have charged up
	//		if (AtMaxCharge)
	//		{
	//			DrawLaser(spriteBatch, Main.projectileTexture[projectile.type], Main.player[projectile.owner].Center,
	//				projectile.velocity, 10, projectile.damage, -1.57f, 1f, 1000f, new Color(0, 0, 255), (int)MoveDistance);

	//			//color


	//		}
	//		return false;
	//	}

	//	// The core function of drawing a laser
	//	public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default(Color), int transDist = 50)
	//	{
	//		Vector2 origin = start;
	//		float r = unit.ToRotation() + rotation;

	//		#region Draw laser body
	//		for (float i = transDist; i <= Distance; i += step)
	//		{
	//			Color c = new Color(0, 0, 255);
	//			origin = start + i * unit;
	//			spriteBatch.Draw(texture, origin - Main.screenPosition,
	//				new Rectangle(0, 26, 28, 26), i < transDist ? Color.Transparent : c, r,
	//				new Vector2(28 * .5f, 26 * .5f), scale, 0, 0);
	//		}
	//		#endregion

	//		#region Draw laser tail
	//		spriteBatch.Draw(texture, start + unit * (transDist - step) - Main.screenPosition,
	//			new Rectangle(0, 0, 28, 26), new Color(0, 0, 255), r, new Vector2(28 * .5f, 26 * .5f), scale, 0, 0);
	//		#endregion

	//		#region Draw laser head
	//		spriteBatch.Draw(texture, start + (Distance + step) * unit - Main.screenPosition,
	//			new Rectangle(0, 52, 28, 26), new Color(0, 0, 255), r, new Vector2(28 * .5f, 26 * .5f), scale, 0, 0);
	//		#endregion
	//	}

	//	// Change the way of collision check of the projectile
	//	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
	//	{
	//		// We can only collide if we are at max charge, which is when the laser is actually fired
	//		if (AtMaxCharge)
	//		{
	//			Player player = Main.player[projectile.owner];
	//			Vector2 unit = projectile.velocity;
	//			float point = 0f;
	//			// Run an AABB versus Line check to look for collisions, look up AABB collision first to see how it works
	//			// It will look for collisions on the given line using AABB
	//			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), player.Center,
	//				player.Center + unit * Distance, 22, ref point);
	//		}
	//		return false;
	//	}

	//	// Set custom immunity time on hitting an NPC
	//	public override bool? CanHitNPC(NPC target)
	//	{
	//		if (target.type == NPCID.DemonEyeSpaceship || target.type == NPCID.DemonEyeOwl || target.type == NPCID.Eyezor || target.type == NPCID.ServantofCthulhu || target.type == NPCID.EyeofCthulhu || target.type == NPCID.WanderingEye || target.type == NPCID.WallofFleshEye || target.type == NPCID.DemonEye || target.type == NPCID.CataractEye || target.type == NPCID.SleepyEye || target.type == NPCID.DialatedEye || target.type == NPCID.GreenEye || target.type == NPCID.PurpleEye || target.netID == -38 || target.netID == -39 || target.netID == -40 || target.netID == -41 || target.netID == -42 || target.netID == -43)
	//		{
	//			//target.AddBuff(BuffID.Confused, 240);
	//			return base.CanHitNPC(target);
	//		}
	//		return false; // by returning false for all other npc, we can't hit them.
	//	}

	//	public virtual bool CanHitPvpWithProj(Projectile proj, Player target)
	//	{
	//		target.AddBuff(BuffID.Darkness, 960);
	//		return true;
	//	}

	//	// The AI of the projectile
	//	public override void AI()
	//	{
	//		Vector2 mousePos = Main.MouseWorld;
	//		Player player = Main.player[projectile.owner];

	//		#region Set projectile position
	//		// Multiplayer support here, only run this code if the client running it is the owner of the projectile
	//		if (projectile.owner == Main.myPlayer)
	//		{
	//			Vector2 diff = mousePos - player.Center;
	//			diff.Normalize();
	//			projectile.velocity = diff;
	//			projectile.direction = Main.MouseWorld.X > player.position.X ? 1 : -1;
	//			projectile.netUpdate = true;
	//		}
	//		projectile.position = player.Center + projectile.velocity * MoveDistance;
	//		projectile.timeLeft = 2;
	//		int dir = projectile.direction;
	//		player.ChangeDir(dir);
	//		player.heldProj = projectile.whoAmI;
	//		player.itemTime = 2;
	//		player.itemAnimation = 2;
	//		player.itemRotation = (float)Math.Atan2(projectile.velocity.Y * dir, projectile.velocity.X * dir);
	//		#endregion

	//		#region Charging process
	//		// Kill the projectile if the player stops channeling
	//		if (!player.channel)
	//		{
	//			projectile.Kill();
	//		}
	//		else
	//		{
	//			// Do we still have enough mana? If not, we kill the projectile because we cannot use it anymore
	//			if (Main.time % 10 < 1 && !player.CheckMana(player.inventory[player.selectedItem].mana, true))
	//			{
	//				projectile.Kill();
	//			}
	//			Vector2 offset = projectile.velocity;
	//			offset *= MoveDistance - 20;
	//			Vector2 pos = player.Center + offset - new Vector2(10, 10);
	//			if (Charge < MaxChargeValue)
	//			{
	//				Charge = 50f;
	//			}
	//			int chargeFact = (int)(Charge / 20f);
	//			/*Vector2 dustVelocity = Vector2.UnitX * 18f;
	//			dustVelocity = dustVelocity.RotatedBy(projectile.rotation - 1.57f, default(Vector2));
	//			Vector2 spawnPos = projectile.Center + dustVelocity;
	//			for (int k = 0; k < chargeFact + 1; k++)
	//			{
	//				Vector2 spawn = spawnPos + ((float)Main.rand.NextDouble() * 6.28f).ToRotationVector2() * (12f - (chargeFact * 2));
	//				Dust dust = Main.dust[Dust.NewDust(pos, 20, 20, 226, projectile.velocity.X / 2f,
	//					projectile.velocity.Y / 2f, 0, default(Color), 1f)];
	//				dust.velocity = Vector2.Normalize(spawnPos - spawn) * 1.5f * (10f - chargeFact * 2f) / 10f;
	//				dust.noGravity = true;
	//				dust.scale = Main.rand.Next(10, 20) * 0.05f;
	//			}*/
	//		}
	//		#endregion

	//		#region Set laser tail position and dusts
	//		if (Charge < MaxChargeValue) return;
	//		Vector2 start = player.Center;
	//		Vector2 unit = projectile.velocity;
	//		unit *= -1;
	//		for (Distance = MoveDistance; Distance <= 2200f; Distance += 5f)
	//		{
	//			start = player.Center + projectile.velocity * Distance;
	//			if (!Collision.CanHit(player.Center, 1, 1, start, 1, 1))
	//			{
	//				Distance -= 5f;
	//				break;
	//			}
	//		}

	//		Vector2 dustPos = player.Center + projectile.velocity * Distance;
	//		#endregion

	//		//Add lights
	//		DelegateMethods.v3_1 = new Vector3(0f, 0f, 0.8f);
	//		Utils.PlotTileLine(projectile.Center, projectile.Center + projectile.velocity * (Distance - MoveDistance), 26,
	//			DelegateMethods.CastLight);
	//	}

	//	public override bool ShouldUpdatePosition()
	//	{
	//		return false;
	//	}

	//	public override void CutTiles()
	//	{
	//		DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
	//		Vector2 unit = projectile.velocity;
	//		Utils.PlotTileLine(projectile.Center, projectile.Center + unit * Distance, (projectile.width + 16) * projectile.scale, DelegateMethods.CutTiles);
	//	}
	//}
}
