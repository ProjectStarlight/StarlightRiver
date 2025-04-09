using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	internal class Talismans : BaseTalisman<TalismansProjectile, TalismansBuff>
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Talismans");
			Tooltip.SetDefault("Sticks to enemies\nIgnites enemies with 6 stacks");
		}

		public override void SafeSetDefaults()
		{
			Item.useTime = 5;
			Item.useAnimation = 15;
			Item.reuseDelay = 20;
			Item.damage = 5;
			Item.mana = 4;
			Item.shootSpeed = 10;
		}

		public override void AddRecipes()
		{
			Recipe.Create(Type, 100)
				.AddIngredient(ItemID.Silk, 4)
				.AddIngredient(ItemID.FallenStar, 1)
				.Register();
		}
	}

	internal class TalismansProjectile : BaseTalismanProjectile<TalismansBuff>
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 20;
			ProjectileID.Sets.TrailingMode[Type] = 2;

			base.SetStaticDefaults();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (!hitTile)
			{
				for (int k = 0; k < 10; k += 2)
				{
					Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Misc/TalismansProjectile").Value;
					Vector2 pos = Projectile.oldPos[k] - Main.screenPosition + new Vector2(14, 11);
					var frame = new Rectangle(0, Projectile.frame * 22, 28, 22);

					Main.spriteBatch.Draw(tex, pos, frame, lightColor * (1 - k / 10f) * 0.2f, Projectile.oldRot[k], new Vector2(14, 11), 0.8f, 0, 0);
				}
			}

			return true;
		}
	}

	internal class TalismansBuff : BaseTalismanBuff
	{
		public override string Name => "TalismansBuff";

		public override string DisplayName => "Talismans";

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override bool Debuff => true;

		public TalismansBuff() : base()
		{
			threshold = 6;
		}

		public override BuffStack GenerateDefaultStack(int duration)
		{
			var stack = new BuffStack
			{
				duration = duration
			};
			return stack;
		}

		public override void OnMaxStacks(NPC npc)
		{
			npc.AddBuff(BuffID.OnFire, 600);
			npc.SimpleStrikeNPC(20, 0, true, 0, DamageClass.Magic);
		}
	}
}