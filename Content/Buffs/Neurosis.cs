using StarlightRiver.Core.Systems.ExposureSystem;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Buffs
{
	internal class Neurosis : StackableBuff
	{
		public override string Name => "Neurosis";

		public override string DisplayName => "Neurosis";

		public override string Tooltip => "Decreases damage dealt by 2%";

		public override string Texture => AssetDirectory.Debug;

		public override bool Debuff => true;

		public override int MaxStacks => 20;

		public override void SafeLoad()
		{
			On_Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += WeakenProj;
			On_Projectile.NewProjectile_IEntitySource_Vector2_Vector2_int_int_float_int_float_float_float += WeakenProj2;
			StarlightNPC.ModifyHitPlayerEvent += WeakenDamage;
		}

		private int WeakenProj(On_Projectile.orig_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float orig, IEntitySource spawnSource, float X, float Y, float SpeedX, float SpeedY, int Type, int Damage, float KnockBack, int Owner, float ai0, float ai1, float ai2)
		{
			if (spawnSource is EntitySource_Parent source && source.Entity is NPC npc && InstancedBuffNPC.GetInstance<Neurosis>(npc) != null)
			{
				var instance = InstancedBuffNPC.GetInstance<Neurosis>(npc);
				var mult = 1 - instance.stacks.Count * 0.02f;
				Damage = (int)(Damage * mult);				
			}

			return orig(spawnSource, X, Y, SpeedX, SpeedY, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
		}

		private int WeakenProj2(On_Projectile.orig_NewProjectile_IEntitySource_Vector2_Vector2_int_int_float_int_float_float_float orig, IEntitySource spawnSource, Vector2 position, Vector2 velocity, int Type, int Damage, float KnockBack, int Owner, float ai0, float ai1, float ai2)
		{
			if (spawnSource is EntitySource_Parent source && source.Entity is NPC npc && InstancedBuffNPC.GetInstance<Neurosis>(npc) != null)
			{
				var instance = InstancedBuffNPC.GetInstance<Neurosis>(npc);
				var mult = 1 - instance.stacks.Count * 0.02f;
				Damage = (int)(Damage * mult);
			}

			return orig(spawnSource, position, velocity, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
		}

		private void WeakenDamage(NPC NPC, Player target, ref Player.HurtModifiers modifiers)
		{
			var instance = InstancedBuffNPC.GetInstance<Neurosis>(NPC);

			if (instance != null)
				modifiers.FinalDamage *= 1 - instance.stacks.Count * 0.02f; 
		}

		public override void PerStackEffectsPlayer(Player player, BuffStack stack)
		{
			player.GetDamage(DamageClass.Generic) -= 0.02f;
		}

		public override BuffStack GenerateDefaultStack(int duration)
		{
			var stack = new BuffStack();
			stack.duration = duration;
			return stack;
		}
	}
}
