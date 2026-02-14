using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Recom3Uplnk
{
    public partial class FormMapView : Form
    {
        public FormMapView()
        {
            InitializeComponent();
        }

        private void FormMapView_Shown(object sender, EventArgs e)
        {
            this.Text = "Uplink Map Viewer";
            this.Icon = Properties.Resources.ic_launcher;
        }
    }
}
