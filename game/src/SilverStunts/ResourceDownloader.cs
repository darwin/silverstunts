using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace SilverStunts
{
    public class ResourceDownloader
    {
        public class Descriptor
        {
            ResourceDownloader parent;
            Downloader downloader;
            public Downloader Downloader { get { return downloader; } }
            IProgressBar progress;

            public Descriptor(ResourceDownloader parent, Downloader downloader, IProgressBar progress)
            {
                this.progress = progress;
                this.downloader = downloader;
                this.parent = parent;

                downloader.Completed += new EventHandler(Completed);
                downloader.DownloadFailed += new ErrorEventHandler(DownloadFailed);
                downloader.DownloadProgressChanged += new EventHandler(DownloadProgressChanged);
            }

            public void Send()
            {
                downloader.Send();
            }
            
            public void DownloadProgressChanged(object sender, EventArgs e)
            {
                if (progress != null) progress.Changed(downloader.DownloadProgress);
            }

            public void DownloadFailed(object sender, ErrorEventArgs e)
            {
                if (progress != null) progress.Failed(downloader.DownloadProgress);
                parent.NotifyFailed(this);
            }

            public void Completed(object sender, EventArgs e)
            {
                if (progress != null) progress.Completed(downloader.DownloadProgress);
                parent.NotifyCompleted(this);
            }
        }

        public string category;
        Dictionary<string, Descriptor> descriptors = new Dictionary<string, Descriptor>();
        public bool Running { get { return running.Count>0; } }
        List<Descriptor> running = new List<Descriptor>();

        public event EventHandler AllCompleted;
        public event EventHandler AnyFailed;

        public ResourceDownloader(string cat)
        {
            category = cat;
        }

        public string UrlResourceName(string cat, string item, string name)
        {
            return /*"http://silverstunts.com/secret/" + */cat + "/" + item + "/" + name; // HACK
        }
        
        public void AddItem(string item, string name)
        {
            AddItem(item, name, null);
        }
        
        public void AddItem(string item, string name, IProgressBar progress)
        {
            if (Running) throw new Exception("Resource Downloader was already started.");

            string resourceUrl = UrlResourceName(category, item, name);

            Downloader downloader = new Downloader(); 
            downloader.Open("GET", new Uri(resourceUrl, UriKind.RelativeOrAbsolute));

            Descriptor descriptor = new Descriptor(this, downloader, progress);
            descriptors.Add(resourceUrl, descriptor);
        }

        public Downloader FindItem(string item, string name, string part)
        {
            string resourceUrl = UrlResourceName(category, item, name);
            Descriptor descriptor;
            if (!descriptors.TryGetValue(resourceUrl, out descriptor)) return null;
            return descriptor.Downloader;
        }

        public Downloader FindItem(string item, string name)
        {
            return FindItem(item, name, "");
        }

        public void Start()
        {
            running.Clear();
            foreach (KeyValuePair<string, Descriptor> item in descriptors)
            {
                running.Add(item.Value);
                item.Value.Send();
            }
        }

        public void NotifyCompleted(Descriptor descriptor)
        {
            running.Remove(descriptor);
            if (running.Count == 0)
            {
                if (AllCompleted != null) AllCompleted(this, new EventArgs());
            }
        }

        public void NotifyFailed(Descriptor descriptor)
        {
            running.Remove(descriptor);
            if (AnyFailed != null) AnyFailed(this, new EventArgs());
       }
    }
}
