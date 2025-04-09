using StarlightRiver.Content.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.NPCs.Crimson
{
	internal class GestaltCell : ModNPC
	{
		public Rectangle arena;

		public override string Texture => AssetDirectory.CrimsonNPCs + Name;

		public ref float Timer => ref NPC.ai[0];
		public ref float CellCount => ref NPC.ai[1];
		public ref float State => ref NPC.ai[2];
		public bool Leader
		{
			get => NPC.ai[3] == 1;
			set => NPC.ai[3] = value ? 1 : 0;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gestalt Cell");
		}

		public override void SetDefaults()
		{
			NPC.lifeMax = 100;
			NPC.damage = 15;
			NPC.defense = 7;
			NPC.width = 43;
			NPC.height = 38;
		}

		public void GetTarget()
		{
			foreach(Player player in Main.ActivePlayers)
			{
				if (player.Hitbox.Intersects(arena) && !player.dead) ;
			}
		}

		public override void AI()
		{
			// Debug arena set
			if (arena == default)
				arena = new Rectangle((int)NPC.Center.X - 400, (int)NPC.Center.Y - 500, 800, 500);
		}

		/// <summary>
		/// Called on a follower cell to merge it into the leader
		/// </summary>
		public void MergeIntoLeader()
		{
			
		}

		/// <summary>
		/// Called on the leader to accept a merge from a follower
		/// </summary>
		public void MergeWithFollower()
		{

		}

		public void OneCellBehavior()
		{

		}

		public void MultiCellMovement()
		{

		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			return base.PreDraw(spriteBatch, screenPos, drawColor);
		}
	}
}
