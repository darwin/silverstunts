﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Browser;
using System.IO;

using Physics;
using Physics.Composites;
using Physics.Constraints;
using Physics.Primitives;
using Physics.Surfaces;

using IronPython;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Text;

namespace SilverStunts
{
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

    [Scriptable]
    public class Page
    {
        public static Page Current;
        Canvas root;
        const int MAX_LEVELS = 6;
        LevelDescriptor[] levels = { 
                                       new LevelDescriptor("uramp"), 
                                       new LevelDescriptor("cavanagh"), 
                                       new LevelDescriptor("jezeq"), 
                                       new LevelDescriptor("intro"), 
                                       new LevelDescriptor("hill"), 
                                       new LevelDescriptor("stairs") 
                                   };
        int counter = 0;
        int currentLevel = 0;
        bool inTick = false;
        public Game game;
        public Stats stats;
        public Help help;
        Canvas topline;
        Canvas bottomline;
        Canvas continueInfo;
        Storyboard timer;
        HtmlTimer htmlTimer;
        public Level level;
        public Level Level { get { return level;  } }

        int renderTick;
        int physicsTick;
        int inputTick;
        
        bool[] keys = new bool[100];
        bool[] platformKeys = new bool[256];

        DateTime lastStatsTime = DateTime.Now;
        DateTime levelTime;
        DateTime deadTime;
        int lastStatsRenderTick = 0;
        bool anyKeyRestart = false;
      
        public void Init(Canvas root)
        {
            Current = this;
            this.root = root;
            UseHTMLTimer();

            // register the scriptable endpoints
            WebApplication.Current.RegisterScriptableObject("page", this);

            timer = root.FindName("timer") as Storyboard;
            
            game = new Game();
            stats = new Stats();
            help = new Help();

            level = new Level("__empty__", game.world, game.foreground, game.background);

            Canvas gamehost = root.FindName("gamehost") as Canvas;
            gamehost.Children.Add(game);

            Canvas statshost = root.FindName("statshost") as Canvas;
            statshost.Children.Add(stats);

            Canvas helphost = root.FindName("helphost") as Canvas;
            helphost.Children.Add(help);

            topline = root.FindName("topline") as Canvas;
            bottomline = root.FindName("bottomline") as Canvas;
            continueInfo = root.FindName("continue") as Canvas;

            InitKeyboard(root);

            SwitchLevel(levels[currentLevel]);
        }

        private void InitKeyboard(Canvas root)
        {
            for (int i = 0; i < 100; i++) keys[i] = false;
            root.KeyDown += OnKeyDown;
            root.KeyUp += OnKeyUp;
        }

        void ResetTicks()
        {
            renderTick = 0;
            physicsTick = 0;
            inputTick = 0;
            lastStatsRenderTick = 0;
            levelTime = DateTime.Now;
            deadTime = DateTime.MaxValue;
        }

        void StartTimer()
        {
            if (htmlTimer!=null)        
            {
                if (!htmlTimer.Enabled) htmlTimer.Start();
                return;
            }
            timer.Begin();
        }

        void StopTimer()
        {
            if (htmlTimer!=null)        
            {
                htmlTimer.Stop();
                return;
            }
            timer.Stop();
        }

        void UseHTMLTimer()
        {
            htmlTimer = new HtmlTimer();
            htmlTimer.Interval = 30;
            htmlTimer.Tick += new EventHandler(Tick);
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
            StopTimer();
            while (inTick) { }
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

            StartTimer();
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
                //ImageBrush ib = (ImageBrush)root.FindName("top");
                if (game.editor.Active)
                {
                    //ib.ImageSource = new Uri("Graphics/01.png", UriKind.Relative);
                    game.DisableEditor();
                }
                else
                {
                    //ib.ImageSource = new Uri("Graphics/02.png", UriKind.Relative);
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
                topline.Visibility = !stats.Visible ? Visibility.Visible : Visibility.Collapsed;
                return true;
            }

            if (e.Key == 59 || e.Key == 58) // F4 or F3 (for IE)
            {
                OpenScriptEditor();
                return true;
            }

            return false;
        }

        public void UpdateKeyState(KeyboardEventArgs e, bool press)
        {
            if (e.PlatformKeyCode <= 255)
            {
                platformKeys[e.PlatformKeyCode] = press;
            }

            if (e.Key <= 100)
            {
                keys[e.Key] = press;
            }
        }

        public void OnKeyDown(Object o, KeyboardEventArgs e)
        {
            UpdateKeyState(e, true);
            if (HandleFrameworkKey(e)) return;
            game.ProcessKey(e, true);
        }

        public void OnKeyUp(Object o, KeyboardEventArgs e)
        {
            UpdateKeyState(e, false);
            game.ProcessKey(e, false);
        }

        public void Tick(object sender, EventArgs e)
        {
            inTick = true;

            DateTime time = DateTime.Now;
            TimeSpan span = time - lastStatsTime;
            
            counter++;

            game.ProcessInputs(keys);
            inputTick++;
            game.Simulate();
            physicsTick++;
            level.Tick(physicsTick, (int)span.TotalMilliseconds);

            if (counter % 2 == 0)
            {
                level.UpdateBinders();
                game.UpdateScrolling();
                renderTick++;
            }

            inTick = false;
            if (level.actor.IsDead() && deadTime == DateTime.MaxValue)
            {
                deadTime = time;
            }

            // little hack - show "continue" screen
            if (!game.editor.Active && level.actor.IsDead() && (time - deadTime).TotalMilliseconds > 1500)
            {
                continueInfo.Visibility = Visibility.Visible;
                anyKeyRestart = true;
            }
            else
            {
                continueInfo.Visibility = Visibility.Collapsed;
                anyKeyRestart = false;
            }
            StartTimer();
        }

        internal string GetStats()
        {
            DateTime time = DateTime.Now;
            TimeSpan span = time - lastStatsTime;
            StringBuilder s = new StringBuilder();
            
            double elapsed = span.TotalMilliseconds / 1000.0f;
            double fps = (renderTick - lastStatsRenderTick) / elapsed;
            s.AppendLine(String.Format("Page: FPS = {0:00.00} RenderTick={1:000000} InputTick={2:000000} PhysicsTick={3:000000}", fps, renderTick, inputTick, physicsTick));
            s.AppendLine(level.GetStats());
            s.AppendLine(game.GetStats());
            s.AppendLine(level.physics.GetStats());

            lastStatsRenderTick = renderTick;
            lastStatsTime = time;
            return s.ToString();
        }
    }
}