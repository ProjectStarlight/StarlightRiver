using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Enums;
using Terraria.GameContent.Creative;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Haunted
{
	public class EchochainWhip : ModItem
	{
		public override string Texture => AssetDirectory.HauntedItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Echochain");
			Tooltip.SetDefault("Chains enemies together, sharing summon damage between them\nHold and release <right> to chain enemies near you to the ground, chaining all effected enemies");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToWhip(ModContent.ProjectileType<EchochainWhipProjectile>(), 15, 3f, 3.75f, 40);
			Item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(gold: 1));
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();

			recipe.AddIngredient(ItemID.GoldBar, 15);
			recipe.AddIngredient<VengefulSpirit>(10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();

			recipe.AddIngredient(ItemID.PlatinumBar, 15);
			recipe.AddIngredient<VengefulSpirit>(10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class EchochainWhipProjectile : BaseWhip
	{
		public List<NPC> hitTargets = new();
		public override string Texture => AssetDirectory.HauntedItem + Name;

		public EchochainWhipProjectile() : base("Echochain", 20, 1.25f, new Color(0, 255, 0)) { }

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			hitTargets.Add(target);
		}

		public override void Kill(int timeLeft)
		{

		}
	}

	class EchochainEdge
	{
		public EchochainNode start;
		public EchochainNode end;
		public int timer;

		public EchochainEdge(EchochainNode start, EchochainNode end, int timer)
		{
			this.start = start;
			this.end = end;
			this.timer = timer;
		}
	}

	class EchochainNode
	{
		public List<EchochainEdge> edges;
		public NPC npc;

		public EchochainNode(List<EchochainEdge> edges, NPC npc)
		{
			this.edges = edges;
			this.npc = npc;
		}
	}

	class EchochainSystem : ModSystem
	{
		public static List<EchochainNode> nodes = new();
		public static List<EchochainEdge> edges = new();

		public override void PostUpdateEverything()
		{
			
		}

		public static void AddNPCS(List<NPC> npcsToAdd)
		{
			List<EchochainNode> list = new();

			for (int i = 0; i < npcsToAdd.Count; i++)
			{
				NPC npc = npcsToAdd[i];

				EchochainNode node = nodes.FirstOrDefault(n => n.npc == npc);

				if (node == null)
				{
					node = new EchochainNode(new List<EchochainEdge>(), npc);
					nodes.Add(node);
				}

				list.Add(node);
			}

			for (int i = 0; i < list.Count; i++)
			{
				
			}
		}
	}

	class EchochainGlobalNPC : GlobalNPC
	{
		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			/*EchochainChain chainToUse = null;

			foreach (EchochainChain chain in EchochainSystem.chains)
			{
				if (chain.chainedTargets.Contains(npc))
				{
					chainToUse = chain;
					break;
				}
			}

			if (chainToUse != null)
			{
				EchochainChainAttack attack = new(60, npc, chainToUse.chainedTargets);
				EchochainSystem.attacks.Add(attack);
			}*/
		}
	}
}