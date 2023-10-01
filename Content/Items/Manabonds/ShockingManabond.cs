using StarlightRiver.Content.Items.Magnet;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Manabonds
{
	internal class ShockingManabond : Manabond
	{
		public override string Texture => AssetDirectory.ManabondItem + Name;

		public ShockingManabond() : base("Shocking Manabond", "Your minions can store 40 mana\nYour minions siphon 6 mana per second from you untill full\nYour minions spend 12 mana to attack with chain lightning occasionally\nChain lightning inflicts overcharged, decreasing defense") { }

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 15);
			Item.rare = ItemRarityID.Orange;
		}

		public override void MinionAI(Projectile minion, ManabondProjectile mp)
		{
			if (mp.timer % 60 == 0 && mp.mana >= 12 && mp.target != null)
			{
				mp.mana -= 12;

				if (Main.myPlayer == minion.owner)
				{
					Shock.parentToAssign = minion;
					Shock.initialTargetToAssign = mp.target;
					Projectile.NewProjectileDirect(minion.GetSource_FromThis(), minion.Center, Vector2.Zero, ModContent.ProjectileType<Shock>(), 12, 0.25f, minion.owner);
				}
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<BasicManabond>(), 1);
			recipe.AddIngredient(ModContent.ItemType<ChargedMagnet>(), 1);
			recipe.AddTile(TileID.Bookcases);
			recipe.Register();
		}
	}

	internal class Shock : ModProjectile, IDrawAdditive
	{
		public Projectile parent;

		public static Projectile parentToAssign;
		public static NPC initialTargetToAssign;

		public readonly List<Vector2> nodes = new();
		public readonly List<NPC> targets = new();

		private bool spawnSoundPerformed = false;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 15;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Shock Bolt");
		}

		public override void OnSpawn(IEntitySource source)
		{
			parent = parentToAssign;
			targets.Add(initialTargetToAssign);
		}

		public override void AI()
		{
			if (parent is null)
				return;

			if (!spawnSoundPerformed)
			{
				spawnSoundPerformed = true;
				Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, Projectile.Center);
			}

			if (Projectile.timeLeft == 14)
			{
				for (int k = 0; k < 2; k++)
				{
					NPC target = FindValidTarget(targets[^1].Center);

					if (target != null)
						targets.Add(target);
				}

				if (Main.myPlayer == Projectile.owner)
				{
					foreach (NPC npc in targets)
					{
						npc.SimpleStrikeNPC(Projectile.damage, 0, false, 0, Projectile.DamageType, true);
						npc.AddBuff(ModContent.BuffType<Buffs.Overcharge>(), 60);
					}
				}
			}

			if (Main.netMode != NetmodeID.Server && Main.GameUpdateCount % 2 == 0) //rebuild electricity nodes
			{
				RebuildElectricNodes();
			}
		}

		private void RebuildElectricNodes()
		{
			nodes.Clear();

			for (int i = 0; i < targets.Count; i++)
			{
				Vector2 point1 = i == 0 ? parent.Center : targets[i - 1].Center;
				Vector2 point2 = targets[i].Center;

				int nodeCount = (int)Vector2.Distance(point1, point2) / 45;

				for (int k = 1; k < nodeCount; k++)
				{
					nodes.Add(Vector2.Lerp(point1, point2, k / (float)nodeCount) +
						(k == nodes.Count - 1 ? Vector2.Zero : Vector2.Normalize(point1 - point2).RotatedBy(1.57f) * (Main.rand.NextFloat(2) - 1) * 14));
				}

				nodes.Add(point2);
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(parent.identity);
			writer.Write(targets[0].whoAmI);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			int id = reader.ReadInt32();
			parent = Main.projectile.FirstOrDefault(n => n.active && n.identity == id);

			if (targets.Count > 0)
				targets[0] = Main.npc[reader.ReadInt32()];
			else
				targets.Add(Main.npc[reader.ReadInt32()]);
		}

		public NPC FindValidTarget(Vector2 start)
		{
			foreach (NPC npc in Main.npc)
			{
				if (!npc.active || npc.friendly || !npc.CanBeChasedBy(this) || targets.Contains(npc))
					continue;

				if (Vector2.Distance(start, npc.Center) <= 400 && Collision.CanHit(start, 1, 1, npc.Center, 1, 1))
					return npc;
			}

			return null;
		}

		public void DrawAdditive(SpriteBatch sb)
		{
			if (parent is null)
				return;

			Vector2 point1 = parent.Center;
			Vector2 point2 = Projectile.Center;

			if (point1 == Vector2.Zero || point2 == Vector2.Zero)
				return;

			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value;

			Color color = new Color(200, 230, 255) * (Projectile.timeLeft <= 5 ? Projectile.timeLeft / 5f : 1);

			for (int k = 1; k < nodes.Count; k++)
			{
				Vector2 prevPos = k == 1 ? point1 : nodes[k - 1];

				var target = new Rectangle((int)(prevPos.X - Main.screenPosition.X), (int)(prevPos.Y - Main.screenPosition.Y), (int)Vector2.Distance(nodes[k], prevPos) + 1, 15);
				var target2 = new Rectangle((int)(prevPos.X - Main.screenPosition.X), (int)(prevPos.Y - Main.screenPosition.Y), (int)Vector2.Distance(nodes[k], prevPos) + 1, 6);
				var origin = new Vector2(0, tex.Height / 2);
				float rot = (nodes[k] - prevPos).ToRotation();

				sb.Draw(tex, target, null, color * 0.5f, rot, origin, 0, 0);
				sb.Draw(tex, target2, null, color, rot, origin, 0, 0);

				if (Main.rand.NextBool(30))
					Dust.NewDustPerfect(prevPos + new Vector2(0, 32), ModContent.DustType<Dusts.GlowLine>(), Vector2.Normalize(nodes[k] - prevPos) * Main.rand.NextFloat(-6, -4), 0, new Color(50, 180, 255), 0.5f);
			}

			foreach (NPC target in targets)
			{
				Texture2D tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Dusts/Aurora").Value;
				Texture2D tex3 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;

				sb.Draw(tex2, target.Center - Main.screenPosition, null, color, 0, tex2.Size() / 2f, 0.3f, 0, 0);
				sb.Draw(tex3, target.Center - Main.screenPosition, null, color, 0, tex3.Size() / 2f, 0.5f, 0, 0);

				sb.Draw(tex2, target.Center - Main.screenPosition, null, new Color(0, 150, 255) * (Projectile.timeLeft / 15f), 0, tex2.Size() / 2f, 0.5f + (1 - Projectile.timeLeft / 15f) * 2, 0, 0);
			}

			Color glowColor = new Color(100, 150, 200) * 0.45f * (Projectile.timeLeft <= 5 ? Projectile.timeLeft / 5f : 1);
			sb.Draw(tex, new Rectangle((int)(point1.X - Main.screenPosition.X), (int)(point1.Y - Main.screenPosition.Y), (int)Vector2.Distance(point1, point2), 100), null, glowColor, (point2 - point1).ToRotation(), new Vector2(0, tex.Height / 2), 0, 0);
		}
	}
}