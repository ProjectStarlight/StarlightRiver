﻿using StarlightRiver.Content.Abilities;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	internal class TentacleHurtbox : ModNPC, IHintable
	{
		public static Tentacle tentacleToAssign;

		public Tentacle tentacle;

		public SquidBoss Parent => tentacle.Parent;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			NPC.width = 80;
			NPC.height = 80;
			NPC.lifeMax = 500;
			NPC.damage = 0;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0f;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.netAlways = true;
		}

		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
		{
			NPC.lifeMax = Main.masterMode ? (int)(1000 * bossAdjustment) : (int)(750 * bossAdjustment);
		}

		public override void OnSpawn(IEntitySource source)
		{
			tentacle = tentacleToAssign;
		}

		public override void AI()
		{
			if (tentacle is null || !tentacle.NPC.active)
			{
				NPC.active = false;
				return;
			}

			if (Parent is null || !Parent.NPC.active)
			{
				NPC.active = false;
				return;
			}

			NPC.realLife = Parent.NPC.whoAmI;

			NPC.Hitbox = tentacle.GetDamageHitbox();

			NPC.dontTakeDamage = tentacle.State != 0;

			if (Parent.NPC.life < Parent.NPC.lifeMax - tentacle.NPC.lifeMax * 4)
				NPC.dontTakeDamage = true;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			return false;
		}

		public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
		{
			modifiers.FinalDamage *= 1.4f;
		}

		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			modifiers.FinalDamage *= 1.4f;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(tentacle.NPC.whoAmI);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			tentacle = Main.npc[reader.ReadInt32()].ModNPC as Tentacle;
		}

		public string GetHint()
		{
			return "Its protecting the main body!";
		}
	}
}