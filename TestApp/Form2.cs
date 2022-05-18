using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApp
{
    public partial class Form2 : Form
    {
        private InpData rpd;

        public Form2(InpData rpd)
        {
            InitializeComponent();
            this.rpd = rpd;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            string[] refList = new string[] { "12", "134a", "22", "32", "245fa", "290", "401A", "402A", "404A",
                "407A", "407C", "407F", "407H", "408A", "409A", "410A", "422B", "422D", "438A", "441A", "447A",
                "448A", "449A", "450A", "452A", "452B", "454A", "454B", "454C", "455A", "466A", "507", "513A",
                "600a", "744 (CO2)", "744A (N2O)", "1234yf", "1234ze" };
            for (int i = 0; i < refList.Length; i++)
                comboBox1.Items.Add(refList[i]);
            comboBox1.SelectedIndex = 0;
        }
        private void buttonOK_Click(object sender, EventArgs e)
        {
            rpd.uom = (radioButton1.Checked) ? 0 : 1;
            rpd.refstr = (string?)comboBox1.SelectedItem;
            rpd.temp = textBox1.Text;
            rpd.press = textBox2.Text;
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                label2.Text = "Temperature (°F):";
                label3.Text = "Pressure (psia):";
            }
            else
            {
                label2.Text = "Temperature (K):";
                label3.Text = "Pressure (kPa):";
            }
        }
    }
}
