namespace StarlightRiver.Core.Systems.ForegroundSystem
{
	public abstract class Foreground : ModType
	{
		protected float opacity = 0;

		public virtual bool Visible => false;

		public virtual bool OverUI => false;

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

		public sealed override void Register()
		{

		}
	}
}