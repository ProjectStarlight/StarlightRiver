using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.BarrierSystem;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Crimson
{
	internal class NeuralFeather : SmartAccessory
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public NeuralFeather() : base("Neural Feather", "Your arrows explode after a brief duration\nThe explosion inflicts {{BUFF:Psychosis}}") { }

		public override void SafeSetDefaults()
		{
			Item.expert = true;
			Item.rare = ItemRarityID.Expert;
			Item.accessory = true;
			Item.width = 32;
			Item.height = 32;

			Item.value = Item.sellPrice(gold: 2);
		}
	}

	// Because we need to store a timer per arrow. Womp womp.
	internal class NeuralFeatherProjectile : GlobalProjectile
	{
		int detonationTimer;

		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.arrow;
		}

		public override void PostAI(Projectile projectile)
		{
			Player owner = Main.player[projectile.owner];

			if (SmartAccessory.GetEquippedInstance(owner, ModContent.ItemType<NeuralFeather>()) != null)
			{
				detonationTimer++;

				if (detonationTimer > 120 * (projectile.extraUpdates + 1))
					projectile.Kill();
			}
		}

		public override void OnKill(Projectile projectile, int timeLeft)
		{
			Player owner = Main.player[projectile.owner];

			if (SmartAccessory.GetEquippedInstance(owner, ModContent.ItemType<NeuralFeather>()) != null)
			{
				for (int k = 0; k < 8; k++)
					Dust.NewDustPerfect(projectile.Center, ModContent.DustType<Dusts.GraymatterDust>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3), 0, default, Main.rand.NextFloat(0.5f, 1f));

				foreach (NPC npc in Main.npc)
				{
					if (npc.active && npc.CanBeChasedBy(this) && !npc.friendly && Helpers.CollisionHelper.CheckCircularCollision(projectile.Center, 32, npc.Hitbox))
					{
						BuffInflictor.Inflict<Psychosis>(npc, 180);
					}
				}
			}
		}
	}
}