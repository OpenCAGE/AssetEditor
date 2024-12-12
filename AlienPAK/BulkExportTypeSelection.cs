using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlienPAK
{
    public partial class BulkExportTypeSelection : Form
    {
        public Action<string> OnTypeSelected;

        public BulkExportTypeSelection()
        {
            InitializeComponent();
        }

        public void SetTypes(List<string> types, string baseTypeToConvert)
        {
            label1.Text = "When exporting, convert " + baseTypeToConvert + " files to:";

            typeSelect.Items.Clear();
            typeSelect.Items.AddRange(types.ToArray());
            typeSelect.SelectedIndex = 0;
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            OnTypeSelected?.Invoke(typeSelect.Items[typeSelect.SelectedIndex].ToString());
            this.Close();
        }
    }
}
