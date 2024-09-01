using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Haunted
{

	/// <summary>
	/// This system acts as a handler for the echo chain damage graph
	/// </summary>
	class EchochainSystem : ModSystem
	{
		// These represent the graph for the echo chain's connection. Essentially
		// every NPC acts as a node, and are connected by edges which are created
		// when NPCs are struck by the weapon. Damage sharing is handled via a
		// graph traversal, and chain expiration happens as a simple iteration over
		// the edges collection and removing expired edges.
		public static List<EchochainNode> nodes = new();
		public static List<EchochainEdge> edges = new();

		public const int MAX_EDGE_TIMER = 300; // the maximum timer for each edge.

		public const int MAX_CHAIN_COUNT = 30; // 30 CHAINS, not enemies who can be chained. Due to multiple chains going between groups of enemies 30 chains is split between around 10-20 enemies in practice.

		public static int[] hitCooldowns = new int[Main.maxPlayers]; // per player hit cooldowns
		public override void Load()
		{
			StarlightNPC.OnHitByItemEvent += OnHitChains;
			StarlightNPC.OnHitByProjectileEvent += OnHitChainsProj;

			On_Main.DrawNPCs += DrawEdges;
		}

		/// <summary>
		/// Calls the DrawEdge() method for all active edges, in the Main.DrawNPCs() hook
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		/// <param name="behindTiles"></param>
		/// <exception cref="NotImplementedException"></exception>
		private void DrawEdges(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
		{
			foreach (EchochainEdge edge in edges)
			{
				edge.DrawEdge(Main.spriteBatch);
			}

			orig(self, behindTiles);
		}

		/// <summary>
		/// Triggers a traversal over the graph to damage all enemies connected to the hit enemy
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <param name="hit"></param>
		/// <param name="damageDone"></param>
		private void OnHitChains(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			if (!item.CountsAsClass(DamageClass.Summon) || hitCooldowns[player.whoAmI] > 0)
				return;

			EchochainNode startNode = nodes.FirstOrDefault(n => n.npc == npc);

			if (startNode == null)
				return;

			EchochainEdge edge = startNode.edges.FirstOrDefault();

			if (edge == default)
				return;

			player.GetModPlayer<StarlightPlayer>().SetHitPacketStatus(false);

			float mult = MathHelper.Lerp(0.5f, 0.25f, edge.timer / (float)MAX_EDGE_TIMER);

			Traverse(npc, (n) =>
			{
				if (Main.myPlayer == player.whoAmI)
					n.SimpleStrikeNPC((int)(hit.SourceDamage * mult), 0);

				for (int i = 0; i < 4; i++)
				{
					Dust.NewDustPerfect(n.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(5f, 5f), 0, new Color(120, 255, 40), 0.65f);

					Dust.NewDustPerfect(n.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2CircularEdge(5f, 5f), 0, new Color(120, 255, 40), 0.65f);
				}

				Dust.NewDustPerfect(n.Center, ModContent.DustType<EchochainBurstDust>(), Vector2.Zero, 0, default, Main.rand.NextFloat(0.5f, 0.75f));

				Helper.PlayPitched("Magic/Shadow1", 0.5f, 0f, npc.Center);
				CameraSystem.shake += 2;
			}, true);

			hitCooldowns[player.whoAmI] = 15;
		}

		/// <summary>
		/// Triggers a traversal over the graph to damage all enemies connected to the hit enemy
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="projectile"></param>
		/// <param name="hit"></param>
		/// <param name="damageDone"></param>
		private void OnHitChainsProj(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (ProjectileID.Sets.IsAWhip[projectile.type] || !projectile.CountsAsClass(DamageClass.Summon) || hitCooldowns[projectile.owner] > 0) // only minions should proc the chain hits
				return;

			EchochainNode startNode = nodes.FirstOrDefault(n => n.npc == npc);

			if (startNode == null)
				return;

			EchochainEdge edge = startNode.edges.FirstOrDefault();

			if (edge == default)
				return;

			Main.player[projectile.owner].GetModPlayer<StarlightPlayer>().SetHitPacketStatus(false);

			float mult = MathHelper.Lerp(0.5f, 0.25f, edge.timer / (float)MAX_EDGE_TIMER);

			Traverse(npc, (n) =>
			{
				if (Main.myPlayer == projectile.owner)
					n.SimpleStrikeNPC((int)(hit.SourceDamage * mult), 0);

				for (int i = 0; i < 4; i++)
				{
					Dust.NewDustPerfect(n.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(5f, 5f), 0, new Color(120, 255, 40), 0.65f);

					Dust.NewDustPerfect(n.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2CircularEdge(5f, 5f), 0, new Color(120, 255, 40), 0.65f);
				}

				Dust.NewDustPerfect(n.Center, ModContent.DustType<EchochainBurstDust>(), Vector2.Zero, 0, default, Main.rand.NextFloat(0.5f, 0.75f));

				Helper.PlayPitched("Magic/Shadow1", 0.5f, 0f, npc.Center);

			}, true);

			hitCooldowns[projectile.owner] = 15;
		}

		public override void PreUpdateDusts()
		{
			List<EchochainEdge> edgesToRemove = new();

			foreach (EchochainEdge edge in edges)
			{
				edge.UpdateEdge();
				// removes ALL chains in which theyre timer has ran out, one of their npcs is inactive, one of their npcs is too far away from another, or somehow both of the edges npcs are the same.
				if (edge.timer <= 0 || !edge.end.npc.active || edge.end == null || !edge.start.npc.active || edge.start == null || Vector2.Distance(edge.start.npc.Center, edge.end.npc.Center) > 500f || edge.start == edge.end)
				{
					edgesToRemove.Add(edge);
				}
			}

			// this prevents "Collection was modified, enumeration may not execute"
			foreach (EchochainEdge edge in edgesToRemove)
			{
				edges.Remove(edge);
				edge.DestroyEdge();
			}

			for (int i = 0; i < hitCooldowns.Length; i++)
			{
				if (hitCooldowns[i] > 0)
					hitCooldowns[i]--;
			}
		}

		/// <summary>
		/// This function will create appropriate nodes and edges for NPCs when they are
		/// struck by the weapon. All NPCs passed not currently connected will have connections
		/// added to them.
		/// </summary>
		/// <param name="npcsToAdd"></param>
		public static void AddNPCS(List<NPC> npcsToAdd)
		{
			List<EchochainNode> list = new();

			for (int i = 0; i < npcsToAdd.Count; i++)
			{
				NPC npc = npcsToAdd[i];

				if (npc.active) // don't add dead npcs to the list silly!
				{
					// First we check if a node already exists for this NPC, and if not, add it
					EchochainNode node = nodes.FirstOrDefault(n => n.npc == npc);

					if (node == null)
					{
						node = new EchochainNode(new List<EchochainEdge>(), npc);
						nodes.Add(node);
					}

					list.Add(node);
				}
			}

			// Now we check all unordered permutations of 2 NPCs, and generate chains between
			// them if they do not already exist
			for (int i = 0; i < list.Count; i++)
			{
				for (int j = i; j < list.Count; j++)
				{
					// This is likely very slightly inefficient over performing the equality
					// check here, but this is sparsely run and this is far more readable

					int[] frames = new int[24];
					for (int a = 0; a < frames.Length; a++) // populate array
					{
						frames[a] = Main.rand.Next(3);
					}

					var potential = new EchochainEdge(list[i], list[j], MAX_EDGE_TIMER, frames);

					if (!edges.Any(n => n.Equals(potential)) && list[i] != list[j] && edges.Count <= MAX_CHAIN_COUNT) // check if it already exists, and if we are trying to add an edge that consists of the same npc, twice
					{
						edges.Add(potential);

						// We append the refferences to this edge to the nodes it is connecting aswell
						nodes.First(n => n == list[i]).edges.Add(potential);
						nodes.First(n => n == list[j]).edges.Add(potential);

						for (int d = 0; d < 20; d++)
						{
							Dust.NewDustPerfect(Vector2.Lerp(list[i].npc.Center, list[j].npc.Center, d / 20f), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(3f, 3f), 0, new Color(150, 200, 20), 0.5f);
						}
					}
				}
			}
		}

		/// <summary>
		/// Triggers a DFS traversal of the graph starting with the node containing the given
		/// NPC, and performing the given action on any NPC connected.
		/// </summary>
		/// <param name="start">The NPC where the traversal should start from. If they are not present in the node colleciton, nothing happens</param>
		/// <param name="action">The action to be performed on every connected NPC</param>
		/// <param name="ignoreFirst">If the NPC passed should ignore the action passed or not</param>
		public static void Traverse(NPC start, Action<NPC> action, bool ignoreFirst = false)
		{
			EchochainNode startNode = nodes.FirstOrDefault(n => n.npc == start);

			// If the start isnt in the graph, give up
			if (startNode is null)
				return;

			Stack<EchochainNode> stack = new();
			HashSet<EchochainNode> visited = new();

			stack.Push(startNode);

			while (stack.Count > 0)
			{
				EchochainNode node = stack.Pop();

				if (!visited.Contains(node))
				{
					if (!(ignoreFirst && node == startNode))
						action(node.npc);

					foreach (EchochainEdge edge in node.edges)
					{
						EchochainNode adjNode = edge.start == node ? edge.end : edge.start;

						if (!visited.Contains(adjNode))
							stack.Push(adjNode);
					}

					visited.Add(node);
				}
			}
		}

		/// <summary>
		/// Helper Method which performs a DFS traversal of the graph, starting at start.
		/// </summary>
		/// <param name="start">The NPC where the traversal should start from. If they are not present in the node collection, nothing happens</param>
		public static void ResetTimers(NPC start)
		{
			EchochainNode startNode = nodes.FirstOrDefault(n => n.npc == start);

			// If the start isnt in the graph, give up
			if (startNode is null)
				return;

			Stack<EchochainNode> stack = new();
			HashSet<EchochainNode> visited = new();

			stack.Push(startNode);

			while (stack.Count > 0)
			{
				EchochainNode node = stack.Pop();

				if (!visited.Contains(node))
				{
					foreach (EchochainEdge edge in node.edges)
					{
						edge.timer = MAX_EDGE_TIMER; // reset the timer

						EchochainNode adjNode = edge.start == node ? edge.end : edge.start;

						if (!visited.Contains(adjNode))
							stack.Push(adjNode);
					}

					visited.Add(node);
				}
			}
		}

		/// <summary>
		/// Removes an NPC from the node list, and disconnects all edges connected to it.
		/// </summary>
		/// <param name="toRemove">The NPC to be removed</param>
		public static void RemoveNPC(NPC toRemove)
		{
			nodes.RemoveAll(n => n.npc == toRemove);
			edges.RemoveAll(n => n.start.npc == toRemove || n.end.npc == toRemove);
		}
	}

	/// <summary>
	/// Represents an edge in the echo chain graph, essentially a chain between two NPCs
	/// </summary>
	class EchochainEdge
	{
		public EchochainNode start;
		public EchochainNode end;
		public int timer;
		public int fadeInTimer;

		public int[] chainFrames; // frames for the chain link are randomized upon creation

		internal List<Vector2> cache;
		internal Trail trail;
		public EchochainEdge(EchochainNode start, EchochainNode end, int timer, int[] chainFrames)
		{
			this.start = start;
			this.end = end;
			this.timer = timer;
			this.chainFrames = chainFrames;
			fadeInTimer = 30;
		}

		public void UpdateEdge()
		{
			timer--;
			if (fadeInTimer > 0)
				fadeInTimer--;

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public void DestroyEdge()
		{
			start.edges.Remove(this);
			end.edges.Remove(this);

			for (int d = 0; d < 20; d++)
			{
				Dust.NewDustPerfect(Vector2.Lerp(start.npc.Center, end.npc.Center, d / 20f), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(3f, 3f), 0, new Color(150, 200, 20), 0.5f);
			}
		}

		public void DrawEdge(SpriteBatch spriteBatch)
		{
			if (!start.npc.active || !end.npc.active || start.npc == null || end.npc == null)
				return;

			DrawPrimitives(spriteBatch);

			Texture2D tex = Assets.Items.Haunted.EchochainWhipChain.Value;
			Texture2D texGlow = Assets.Items.Haunted.EchochainWhipChain_Glow.Value;
			Texture2D texBlur = Assets.Items.Haunted.EchochainWhipChain_Blur.Value;
			Texture2D bloomTex = Assets.Keys.GlowAlpha.Value;
			Vector2 chainStart = start.npc.Center;
			Vector2 chainEnd = end.npc.Center;

			float rotation = chainStart.DirectionTo(chainEnd).ToRotation() + MathHelper.PiOver2;

			float distance = Vector2.Distance(chainStart, chainEnd);

			float fadeIn = 1f - fadeInTimer / 30f;

			for (int i = 0; i < (distance / 22); i += 1)
			{
				var pos = Vector2.Lerp(chainStart, chainEnd, i * 22 / distance);

				spriteBatch.Draw(bloomTex, pos - Main.screenPosition, null, new Color(100, 200, 10, 0) * 0.1f, 0f, bloomTex.Size() / 2f, 0.5f, 0f, 0f);

				Rectangle frame = tex.Frame(verticalFrames: 5, frameY: chainFrames[i]);
				spriteBatch.Draw(tex, pos - Main.screenPosition, frame, Color.White * fadeIn, rotation, frame.Size() / 2f, 1f, 0f, 0f);

				frame = texBlur.Frame(verticalFrames: 5, frameY: chainFrames[i]);
				spriteBatch.Draw(texBlur, pos - Main.screenPosition, frame, Color.White with { A = 0 } * 0.5f * fadeIn, rotation, frame.Size() / 2f, 1f, 0f, 0f);
			}

			spriteBatch.Draw(bloomTex, chainStart - Main.screenPosition, null, new Color(100, 200, 10, 0) * 0.1f * fadeIn, 0f, bloomTex.Size() / 2f, 0.45f, 0f, 0f);
			spriteBatch.Draw(bloomTex, chainEnd - Main.screenPosition, null, new Color(100, 200, 10, 0) * 0.1f * fadeIn, 0f, bloomTex.Size() / 2f, 0.45f, 0f, 0f);
		}

		#region Primitive Drawing
		private void ManageCaches()
		{
			cache = new List<Vector2>();

			for (int i = 0; i < 10; i++)
			{
				cache.Add(Vector2.Lerp(start.npc.Center, end.npc.Center, i / 10f));
			}
		}

		private void ManageTrail()
		{
			if (trail is null || trail.IsDisposed)
			{
				trail = new Trail(Main.instance.GraphicsDevice, 10, new NoTip(), factor => 12.5f, factor =>
							{
								if (factor.X >= 0.85f)
									return Color.Transparent;

								return new Color(100, 200, 10) * 0.3f * factor.X;
							});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = cache[9];
		}

		public void DrawPrimitives(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * -0.025f);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.FireTrail.Value);

			trail?.Render(effect);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["sampleTexture"].SetValue(Assets.EnergyTrail.Value);

			trail?.Render(effect);

			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}
		#endregion Primitive Drawing

		/// <summary>
		/// Checks the equality of two edges. Since this graph is undirected, if the order is inversed
		/// it should still count as equal to that edge.
		/// </summary>
		/// <param name="obj">The other edge to compare to</param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj is EchochainEdge edge)
			{
				return
					start == edge.start && end == edge.end ||
					start == edge.end && end == edge.start;
			}

			return false;
		}

		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Represents a node in the echo chain graph, essentially a wrapper around an NPC
	/// </summary>
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
}