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
using System.Text;

using Physics;
using Physics.Composites;
using Physics.Constraints;
using Physics.Primitives;
using Physics.Surfaces;
using Physics.Util;
using Physics.Base;

namespace SilverStunts
{
    public class Game : Control
    {
        public Editor editor;
        Physics.Engine physics;
        public Level level;

        int cameraX;
        int cameraY;

        public ClipCanvas content;

        public Canvas scroller;
        public Canvas world;
        public Canvas foreground;
        public Canvas background;
        public bool simulationEnabled = false;
                
        public Game()
        {
            Stream s = this.GetType().Assembly.GetManifestResourceStream("SilverStunts.Game.xaml");
            content = this.InitializeFromXaml(new StreamReader(s).ReadToEnd()) as ClipCanvas;

            editor = new Editor(this);

            scroller = content.FindName("scroller") as Canvas;
            world = content.FindName("world") as Canvas;
            foreground = content.FindName("foreground") as Canvas;
            background = content.FindName("background") as Canvas;

            content.UpdateLayout();
        }

        public void Reset()
        {
            editor.Reset();
            ResetScrolling();
            UpdateScrolling();
        }

        public void AttachToLevel(Level level)
        {
            this.level = level;
            this.physics = level.physics;
            Reset();
        }

        public void ResetScrolling()
        {
            cameraX = 500;
            cameraY = 400;
        }

        public void EnableEditor()
        {
            editor.Enable();
        }

        public void DisableEditor()
        {
            editor.Disable();
        }

        public void EnableSimulation()
        {
            simulationEnabled = true;
        }

        public void DisableSimulation()
        {
            simulationEnabled = false;
        }

        public void Simulate()
        {
            if (!simulationEnabled) return;
            physics.TimeStep();
        }

        public void ProcessInputs(bool[] keys)
        {
            if (editor.Active) editor.ProcessInputs(keys);
            if (!simulationEnabled) return;
            if (level!=null) level.ProcessInputs(keys);
        }

        public void ProcessKey(KeyboardEventArgs e, bool pressed)
        {
            // handle editor behavior
            if (editor.Active) editor.ProcessKey(e, pressed);

            // no key processing in game ...
        }

        public void UpdateScrolling()
        {
            // update scroll pane
            if (editor.Active)
            {
                // editor handles scrolling
                editor.UpdateScrolling();
            }
            else
            {
                // game handles scrolling
                Vector apos = level.actor.GetPos();
                Vector dx= new Vector(apos.X - cameraX, apos.Y - cameraY);

                double mag = dx.Magnitude();
                if (mag > 400) dx.Mult(400 / mag);

                cameraX += (int)(dx.X / 2);
                cameraY += (int)(dx.Y / 2);

                TranslateTransform tt = new TranslateTransform();
                tt.X = -(cameraX - 500);
                tt.Y = -(cameraY - 400);
                scroller.RenderTransform = tt;
            }
        }

        public Binder PickSurface(double x, double y)
        {
            return level.PickSurface(x, y);
        }

        internal string GetStats()
        {
            
            return String.Format("Game: CamX={0:0000} CamY={1:0000} World=#{2:0000}",
                cameraX,
                cameraY,
                world.Children.Count
                );

        }

        internal List<Binder> PickSurfaces(double x, double y, double w, double h)
        {
            return level.PickSurfaces(x, y, w, h);
        }
    }
}