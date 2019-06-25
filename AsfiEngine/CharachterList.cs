using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsfiEngine
{
    public partial class CharachterList : Form
    {
        public CharachterList()
        {
            InitializeComponent();
        }

        private void LoadChars()
        {
            int min = (int)numericUpDown2.Value;
            int max = (int)numericUpDown1.Value;
            for(int i = min; i < max; i++)
            {
                listBox1.Items.Add($"{i} {(char)i}");
            }
        }

        private void CharachterList_Load(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listBox1.SelectedIndex>0)
            {
                richTextBox1.Text = $"{listBox1.Items[listBox1.SelectedIndex]}".Split(' ')[1];
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadChars();
        }
    }
}
