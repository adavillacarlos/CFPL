using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CFPL
{
    public partial class Form1 : Form
    {
        String filePath;
        Scanner scanner;
        int errorScanner;
        Interpreter interpreter;
        int errorInterpreter; 

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text Files (.txt)|*.txt";
            ofd.Title = "Open a File...";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(ofd.FileName);
                filePath = ofd.FileName; 
                fastColoredTextBox1.Text = sr.ReadToEnd();
                sr.Close();

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog svf = new SaveFileDialog();
            svf.Filter = "Text Files (.txt)|*.txt";
            svf.Title = "Save File...";
            System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath);
            sw.Write(fastColoredTextBox1.Text);
            sw.Close();
          
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog svf = new SaveFileDialog();
            svf.Filter = "Text Files (.txt)|*.txt";
            svf.Title = "Save File...";
            if (svf.ShowDialog() == DialogResult.OK)
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(svf.FileName);
                sw.Write(fastColoredTextBox1.Text);
                sw.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            scanner = new Scanner(fastColoredTextBox1.Text);
            errorScanner = scanner.Process();
            List<Tokens> t = new List<Tokens>(scanner.Tokens);
            interpreter = new Interpreter(scanner.Tokens);
            errorInterpreter = interpreter.Parse();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Hello World"); 
        }
    }
}
