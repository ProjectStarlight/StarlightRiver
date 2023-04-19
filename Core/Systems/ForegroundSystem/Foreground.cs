﻿namespace StarlightRiver.Core.Systems.ForegroundSystem
{
	public abstract class Foreground
	{
		protected float opacity = 0;

		public virtual bool Visible => false;

		public virtual bool OverUI => false;

		public Foreground()
		{
			OnLoad();
		}

		public void Render(SpriteBatch spriteBatch)
		{
			if (Visible || opacity > 0)
			{
				Draw(spriteBatch, opacity);

				if (Visible && opacity < 1)
					opacity += 0.05f;

				if (!Visible && opacity > 0)
					opacity -= 0.05f;
			}
		}

		public virtual void Draw(SpriteBatch spriteBatch, float opacity) { }

		public virtual void Reset() { }

		public virtual void OnLoad() { }

		public virtual void Unload() { }
	}

	public abstract class ParticleForeground : Foreground
	{
		public ParticleSystem ParticleSystem { get; set; }

		public ParticleForeground()
		{
			OnLoad();
		}

		public ParticleForeground(ParticleSystem system)
		{
			ParticleSystem = system;
			OnLoad();
		}

		public override void Unload()
		{
			ParticleSystem = null;
		}
	}
}