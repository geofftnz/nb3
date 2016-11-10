using NAudio.Wave;
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
        //private IWavePlayer output = null;
        private Player.Player player = null;

        public Launcher()
        {
            InitializeComponent();
        }

        private void LaunchButton_Click(object sender, EventArgs e)
        {
            LaunchVis();
        }

        private void LaunchVis()
        {
            if (visHost == null)
            {
                visHost = new Vis.VisHost();
                visHost.Closed += VisHost_Closed;
                visHost.Player = player;

                visHost.Run(60);
            }
        }

        private void VisHost_Closed(object sender, EventArgs e)
        {
            DisposeVisHost();
        }


        private void Launcher_FormClosed(object sender, FormClosedEventArgs e)
        {
            DisposeVisHost();
            DestroyPlayer();
        }

        private void DisposeVisHost()
        {
            if (visHost != null)
            {
                visHost.Dispose();
                visHost = null;
            }
        }

        private void Launcher_Load(object sender, EventArgs e)
        {
            CreatePlayer();

            //LaunchVis();
        }

        private void CreatePlayer()
        {
            DestroyPlayer();
            player = new Player.Player(()=> new DirectSoundOut(70));
            //player.SpectrumReady += Player_SpectrumReady;

            if (visHost != null)
            {
                visHost.Player = player;
            }
        }

        private void DestroyPlayer()
        {
            if (player != null)
            {
                //player.SpectrumReady -= Player_SpectrumReady;
                player.Dispose();
                player = null;
            }
        }


        //private void Player_SpectrumReady(object sender, Player.FftEventArgs e)
        //{
            //if (visHost != null)
            //{
                //visHost.AddSample(e.Sample);
            //}
        //}

        #region PlaylistView

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

        private void PlaylistView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            /*
            foreach (ListViewItem item in PlaylistView.SelectedItems)
            {
                MessageBox.Show(item.Text);
            }*/
            if (PlaylistView.SelectedItems.Count > 0)
            {
                ListViewItem item = PlaylistView.SelectedItems[0];

                player.Open(item.Text);
                player.Play();
            }

        }

        #endregion

    }
}
