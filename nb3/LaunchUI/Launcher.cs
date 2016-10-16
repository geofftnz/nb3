using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nb3.LaunchUI
{
    public partial class Launcher : Form
    {
        private Vis.VisHost visHost = null;

        public Launcher()
        {
            InitializeComponent();
        }

        private void LaunchButton_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("TODO: Show renderer panel");
            if (visHost == null)
            {
                visHost = new Vis.VisHost();
                visHost.Closed += VisHost_Closed;

                visHost.Run(60);
            }
        }

        private void VisHost_Closed(object sender, EventArgs e)
        {
            DisposeVisHost();
        }

        private void PlaylistView_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            e.Action = DragAction.Continue;
        }

        private void PlaylistView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void PlaylistView_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            //MessageBox.Show("TODO: Dropped: " + string.Join(",", files));
            foreach (var file in files)
            {
                PlaylistView.Items.Add(new ListViewItem(file));
            }
        }

        private void Launcher_FormClosed(object sender, FormClosedEventArgs e)
        {
            DisposeVisHost();
        }

        private void DisposeVisHost()
        {
            if (visHost != null)
            {
                visHost.Dispose();
                visHost = null;
            }
        }
    }
}
