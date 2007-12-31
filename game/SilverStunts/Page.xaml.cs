using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Browser;

namespace SilverStunts
{
	#region Support classes for scripting
	[Scriptable]
	public class PrintConsoleEventArgs : EventArgs
	{
		[Scriptable]
		public string message { get; set; }

		public PrintConsoleEventArgs(string message)
		{
			this.message = message;
		}
	}

	[Scriptable]
	public class ShowWorldXAMLEventArgs : EventArgs
	{
		[Scriptable]
		public string xaml { get; set; }

		public ShowWorldXAMLEventArgs(string xaml)
		{
			this.xaml = xaml;
		}
	}

	[Scriptable]
	public class EntitiesSourceEventArgs : EventArgs
	{
		[Scriptable]
		public string source { get; set; }

		public EntitiesSourceEventArgs(string source)
		{
			this.source = source;
		}
	}
	#endregion

	[Scriptable]
	public class Page
	{
		public static Page Current;
		Canvas content;
		Canvas continueInfo;

		public Game game;
		public Stats stats;
		public Help help;

		LevelDescriptor[] levels = {
			new LevelDescriptor("intro"),
			new LevelDescriptor("uramp"),
			new LevelDescriptor("cavanagh"),
			new LevelDescriptor("jezeq"),
			new LevelDescriptor("hill"),
			new LevelDescriptor("stairs")
		};
		int currentLevel = 0;

		int counter = 0;

		Timer timer = new Timer();
		Keyboard keyboard = new Keyboard();

		public Level level;
		public Level Level { get { return level;  } }

		public int renderTick;
		int physicsTick;
		int inputTick;

		DateTime deathTime;
		bool anyKeyRestart = false;

		public void Init(Canvas root)
		{
			Current = this;
			this.content = root;

			// register the scriptable endpoints
			WebApplication.Current.RegisterScriptableObject("page", this);

			game = new Game(root);
			stats = new Stats(root);
			help = new Help(root);

			level = new Level("__empty__", game.world, game.foreground, game.background);

			Canvas gamehost = root.FindName("gamehost") as Canvas;
			gamehost.Children.Add(game);

			Canvas statshost = root.FindName("statshost") as Canvas;
			statshost.Children.Add(stats);

			Canvas helphost = root.FindName("helphost") as Canvas;
			helphost.Children.Add(help);

			continueInfo = root.FindName("continue") as Canvas;

			timer.Attach(root, "game-loop", new Timer.TickDelegate(GameTick), new TimeSpan(TimeSpan.TicksPerSecond/120)); 

			keyboard.Init(root, new Keyboard.KeyEventDelegate(KeyPress));

			SwitchLevel(levels[currentLevel]);
		}

		void KeyPress(KeyboardEventArgs e, bool down)
		{
			if (down && HandleFrameworkKey(e)) return;
			game.ProcessKey(e, down);
		}

		void ResetTicks()
		{
			renderTick = 0;
			physicsTick = 0;
			inputTick = 0;
			deathTime = DateTime.MaxValue;
		}

		void StartTimers()
		{
			timer.Start();
		}

		void StopTimers()
		{
			timer.Stop();
		}


		public enum ConsoleOutputKind
		{
			Interactive = 0,
			Output = 1,
			Error = 2,
			Exception = 3
		}

		public void PrintConsole(string s, ConsoleOutputKind kind)
		{
			s = s.Replace("<", "&lt;");
			s = s.Replace(">", "&gt;");
			s = s.Replace("\n", "<br/>");
			string color = "white";
			switch (kind)
			{
				case ConsoleOutputKind.Output: color = "lightblue"; break;
				case ConsoleOutputKind.Error: color = "yellow"; break;
				case ConsoleOutputKind.Exception: color = "red"; break;
			}
			s = "<font color=\"" + color + "\">" + s + "</font>";
			if (PrintConsoleEvent != null) PrintConsoleEvent(this, new PrintConsoleEventArgs(s));
		}

		public void ShowWorldXAML()
		{
			if (ShowWorldXAMLEvent != null)
			{
				string xaml = level.RenderXAML();
				ShowWorldXAMLEvent(this, new ShowWorldXAMLEventArgs(xaml));
			}
		}

		public void RequestEntitiesSource()
		{
			if (RequestEntitiesSourceEvent != null)
			{
				EntitiesSourceEventArgs args = new EntitiesSourceEventArgs("");
				RequestEntitiesSourceEvent(this, args);
				if (args.source.Length>0) level.EntitiesSource = args.source;
			}
		}

		public void PublishEntitiesSource()
		{
			if (PublishEntitiesSourceEvent != null)
			{
				PublishEntitiesSourceEvent(this, new EntitiesSourceEventArgs(level.EntitiesSource));
			}
		}

		public void OpenScriptEditor()
		{
			if (OpenScriptEditorEvent != null)
			{
				OpenScriptEditorEvent(this, new EventArgs());
			}
		}

		public void UpdateIDE()
		{
			if (UpdateIDEEvent != null)
			{
				UpdateIDEEvent(this, new EventArgs());
			}
		}

		// async
		public void DownloadLevel(LevelDescriptor ld)
		{
			ResourceDownloader rd = new ResourceDownloader("levels");
			rd.AddItem(ld.Name, "foreground.xaml");
			rd.AddItem(ld.Name, "background.xaml");
			rd.AddItem(ld.Name, "entities.py");
			rd.AddItem(ld.Name, "logic.py");
			rd.AllCompleted += new EventHandler(rd_AllCompleted);
			rd.AnyFailed += new EventHandler(rd_AnyFailed);

			rd.Start();
		}

		void rd_AnyFailed(object sender, EventArgs e)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void rd_AllCompleted(object sender, EventArgs e)
		{
			LevelReady(sender as ResourceDownloader);
		}

		public void SwitchLevel(LevelDescriptor ld)
		{
			StopTimers();
			game.DisableSimulation();
			level.Clear();

			DownloadLevel(ld);
		}

		public void LevelReady(ResourceDownloader rd)
		{
			level = new Level(levels[currentLevel].Name, game.world, game.foreground, game.background);

			Downloader foreground = rd.FindItem(levels[currentLevel].Name, "foreground.xaml");
			level.ForegroundSource = foreground.GetResponseText("");
			Downloader background = rd.FindItem(levels[currentLevel].Name, "background.xaml");
			level.BackgroundSource = background.GetResponseText("");
			Downloader entities = rd.FindItem(levels[currentLevel].Name, "entities.py");
			level.EntitiesSource = entities.GetResponseText("");
			Downloader logic = rd.FindItem(levels[currentLevel].Name, "logic.py");
			level.LogicSource = logic.GetResponseText("");

			RestartLevel();
			UpdateIDE();

			StartTimers();
		}

		public void RestartLevel()
		{
			level.Reset();
			game.AttachToLevel(level);
			game.EnableSimulation();
			ResetTicks();
		}

		public void LevelFailed()
		{

		}

		public bool HandleFrameworkKey(KeyboardEventArgs e)
		{
			if (anyKeyRestart)
			{
				RestartLevel();
				return true;
			}

			if (e.Key == 2 || e.PlatformKeyCode == 192) // TAB
			{
				currentLevel++;
				if (currentLevel >= levels.Length) currentLevel = 0;

				SwitchLevel(levels[currentLevel]);
				return true;
			}

			if (e.Key == 3) // ENTER
			{
				RestartLevel();
				return true;
			}

			if (e.Key == 9) // SPACE
			{
				if (game.editor.Active)
				{
					game.DisableEditor();
				}
				else
				{
					game.EnableEditor();
				}
				return true;
			}

			if (e.Key == 56) // F1
			{
				help.Visible = !help.Visible;
				return true;
			}

			if (e.Key == 57) // F2
			{
				stats.Visible = !stats.Visible;
				return true;
			}

			if (e.Key == 59 || e.Key == 58) // F4 or F3 (for IE)
			{
				OpenScriptEditor();
				return true;
			}

			return false;
		}

		public void GameTick(TimeSpan timeElapsed)
		{
			counter++;

			game.ProcessInputs(keyboard.keys);
			inputTick++;
			game.Simulate();
			physicsTick++;
			level.Tick(physicsTick, (int)timeElapsed.TotalMilliseconds);

			if (counter % 2 == 0)
			{
				level.UpdateVisuals();
				game.UpdateScrolling();
				renderTick++;
			}

			HandleContinueMessage();
		}

		private void HandleContinueMessage()
		{
			// show "continue" screen if needed
			DateTime time = DateTime.Now;
			if (level.actor.IsDead() && deathTime == DateTime.MaxValue) deathTime = time;
			if (!game.editor.Active && level.actor.IsDead() && (time - deathTime).TotalMilliseconds > 1500)
			{
				continueInfo.Visibility = Visibility.Visible;
				anyKeyRestart = true;
			}
			else
			{
				continueInfo.Visibility = Visibility.Collapsed;
				anyKeyRestart = false;
			}
		}

		#region Scriptable interface

		[Scriptable]
		public event EventHandler PrintConsoleEvent;
		[Scriptable]
		public event EventHandler ShowWorldXAMLEvent;
		[Scriptable]
		public event EventHandler RequestEntitiesSourceEvent;
		[Scriptable]
		public event EventHandler PublishEntitiesSourceEvent;
		[Scriptable]
		public event EventHandler OpenScriptEditorEvent;
		[Scriptable]
		public event EventHandler UpdateIDEEvent;

		[Scriptable]
		public bool EvalExpression(string line, bool force)
		{
			return level.EvalExpression(line, force);
		}

		[Scriptable]
		public bool EvalEntities(string code)
		{
			level.EntitiesSource = code;
			RestartLevel();
			return true;
		}

		[Scriptable]
		public bool EvalLogic(string code)
		{
			level.LogicSource = code;
			RestartLevel();
			return true;
		}

		[Scriptable]
		public string GetEntitiesSource()
		{
			return level.EntitiesSource;
		}

		[Scriptable]
		public string GetLogicSource()
		{
			return level.LogicSource;
		}

		[Scriptable]
		public string GetForegroundSource()
		{
			return level.ForegroundSource;
		}

		[Scriptable]
		public string GetBackgroundSource()
		{
			return level.BackgroundSource;
		}

		[Scriptable]
		public bool EvalForeground(string xaml)
		{
			level.ForegroundSource = xaml;
			RestartLevel();
			return true;
		}

		[Scriptable]
		public bool EvalBackground(string xaml)
		{
			level.BackgroundSource = xaml;
			RestartLevel();
			return true;
		}

		#endregion
	}
}
