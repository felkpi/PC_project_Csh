using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace STAS_60
{
    public partial class View_Alarm : UserControl
    {
        Model model;
        public View_Alarm(Model model)
        {
            InitializeComponent();
            this.model = model;
        }
    }
}
