using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Browser;
using System.Text;

namespace SilverStunts
{
    public class Stats : Control
    {
        public ClipCanvas content;
        
        public bool Visible
        { 
            get {
                return content.Visibility == Visibility.Visible; 
            } 
            set {
                if (value) statsTimer.Start(); else statsTimer.Stop();
                content.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            } 
        }

        Timer statsTimer = new Timer();
        TextBlock pageStats;
        int lastRenderTick = 0;

        public Stats(Canvas parent)
        {
            Stream s = this.GetType().Assembly.GetManifestResourceStream("SilverStunts.Stats.xaml");
            content = (ClipCanvas)this.InitializeFromXaml(new StreamReader(s).ReadToEnd());

            pageStats = content.FindName("pageStats") as TextBlock;
            statsTimer.Attach(content, "stats-timer", new Timer.TickDelegate(Tick), new TimeSpan(TimeSpan.TicksPerSecond/2));

            content.UpdateLayout();
        }

        public void Tick(TimeSpan elapsedTime)
        {
            pageStats.Text = BuildStats(elapsedTime);
        }

        string BuildStats(TimeSpan elapsedTime)
        {
            Page page = Page.Current;

            // compute FPS
            double elapsedSeconds = elapsedTime.TotalMilliseconds / 1000.0f;
            double fps = (page.renderTick - lastRenderTick) / elapsedSeconds;
            lastRenderTick = page.renderTick;

            // build stats string
            StringBuilder s = new StringBuilder();
            s.AppendLine(String.Format("\nPage: FPS = {0:00.00} RenderTick={1:000000}", fps, page.renderTick));
            s.AppendLine(page.level.GetStats());
            s.AppendLine(page.game.GetStats());
            s.AppendLine(page.level.physics.GetStats());

            return s.ToString();
        }

    }
}
