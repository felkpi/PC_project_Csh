using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace STAS_60
{
    public partial class View_Graphik : UserControl
    {
        Model model;
        public View_Graphik(Model model)
        {
            InitializeComponent();
            this.model = model;
        }
    }
}
