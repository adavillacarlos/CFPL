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
        Lexer lexer;
        int errorLexer;
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
            string input = richTextBox1.Text;
            lexer = new Lexer(fastColoredTextBox1.Text);
            errorLexer = lexer.Analyze();
            List<Tokens> t = new List<Tokens>(lexer.Tokens);
            interpreter = new Interpreter(lexer.Tokens,input);
            errorInterpreter = interpreter.Parse();

            if (errorLexer==0 && errorInterpreter == 0)
            {
                richTextBox1.Text = "";
                richTextBox2.Text = "Compiled Successfully";
                richTextBox3.Text = "";
                foreach (string a in interpreter.OutputMessages)
                {
                  
                   richTextBox3.Text += a;
                    
                }
            } else
            {
                richTextBox1.Text = ""; 
                richTextBox2.Text = "";
                richTextBox3.Text = "";
                foreach (string a in lexer.ErrorMessages)
                {
                    richTextBox2.Text += a + "\n";
                }
                foreach (string a in interpreter.ErrorMessages)
                {
                    richTextBox2.Text += a + "\n";
                }
                
            }
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Hello World"); 
        }

        private void fastColoredTextBox1_Load(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
