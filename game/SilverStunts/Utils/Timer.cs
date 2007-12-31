using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace SilverStunts
{
	// credit for the idea: 
	// http://silverlightrocks.com/community/blogs/silverlight_games_101/archive/2007/05/21/a-better-game-loop.aspx
	public class Timer
	{
		Canvas canvas = null;
		Storyboard storyboard;
		DateTime lastUpdateTime = DateTime.MinValue;
		TimeSpan elapsedTime;

		public delegate void TickDelegate(TimeSpan elapsedTime);
		private event TickDelegate tickEvent;

		public void Attach(Canvas canvas, string name, TickDelegate tickEvent)
		{
			Attach(canvas, name, tickEvent, new TimeSpan());
		}

		public void Attach(Canvas canvas, string name, TickDelegate tickEvent, TimeSpan interval)
		{
			if (this.canvas != null) return;
			this.canvas = canvas;
			this.tickEvent = tickEvent;
			storyboard = new Storyboard(); 
			storyboard.SetValue<string>(Storyboard.NameProperty, name);
			canvas.Resources.Add(storyboard);
			lastUpdateTime = DateTime.Now;
			storyboard.Duration = new Duration(interval);
			storyboard.Completed += new EventHandler(Tick);
		}

		public void Detach()
		{
			if (canvas == null) return;
			storyboard.Stop();
			canvas.Resources.Remove(storyboard);
			canvas = null;
		}

		public void Start()
		{
			storyboard.Begin();
		}

		public void Stop()
		{
			storyboard.Stop();
		}

		void Tick(object sender, EventArgs e)
		{
			elapsedTime = DateTime.Now - lastUpdateTime;
			lastUpdateTime = DateTime.Now;
			if (tickEvent != null) tickEvent(elapsedTime);
			storyboard.Begin();
		}
	}
}
