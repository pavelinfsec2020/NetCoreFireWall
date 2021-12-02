using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using FireWall_Core;
namespace WindowsFirewall
{
    public partial class MainWindow : Form
    {

        private int _maxSynPackets;
        private IPAddress _IPv4;
        private FireWall _fireWall;
        private int[] _securedPorts;
        private Dictionary<string, string> _arpTable = new Dictionary<string, string>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private bool CheckEntriesData()
        {
           
            if (!IPAddress.TryParse(textBox1.Text, out _IPv4))
            {
                MessageBox.Show(
                    "IP адрес имеет неверный формат!",
                    "Ошибка!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                return false;
            }
            else if (!Int32.TryParse(textBox2.Text, out _maxSynPackets))
            {
                MessageBox.Show(
                    "Максимальное число syn пакетов должно быть натуральным числом!",
                    "Ошибка!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                return false;
            }
            else if (!ReadScannedPorts())
            {
               
                return false;
            }
            else if (!ReadArpTable())
            {

                return false;
            }
            return true;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (CheckEntriesData())
            {
                _fireWall = new FireWall(
                    _IPv4,
                    _maxSynPackets,
                    _securedPorts,
                    _arpTable
                    );

                ShowNetInterfaces();
            }
            
        }
        private void ShowNetInterfaces()
        {
            listBox1.Items.Clear();
            string[] netNames = _fireWall.GetNetInterfacesNames();
            foreach (string s in netNames)
            {
                listBox1.Items.Add(s);              
            }
        }
        private bool ReadScannedPorts()
        {
            _securedPorts = new int[richTextBox1.Lines.Length];

            for (int i = 0; i <_securedPorts.Length; i++)
            {
                if (!Int32.TryParse(richTextBox1.Lines[i].Trim(), out _securedPorts[i]))
                {
                    MessageBox.Show(
                        "Неверный формат введенного порта! \n Cтрока " + (i+1) +"\n Значение:"+ richTextBox1.Lines[i].Trim(),
                        "Ошибка!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                        );
                    return false;
                }
                    
            }
               return true;
            
           
        }
        private string[] SliceIpAndMacString(string ipAndMac)
        {
            string[] pair = ipAndMac.Split(';');
            return pair;
        }
        private bool   ReadArpTable()
        {
            _arpTable.Clear();         
 
            for (int i = 0; i < richTextBox2.Lines.Length; i++)
            { 
                string[] pair = SliceIpAndMacString(richTextBox2.Lines[i].Trim());
               
                if(pair.Length==2)
                {
                    _arpTable.Add(pair[0], pair[1]);
                }    
                else
                {
                    MessageBox.Show(
                        "arp-таблица содержит ошибки! \n Cтрока " + (i+1) +"\n Значение: "+ richTextBox2.Lines[i].Trim(),
                        "Ошибка!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                        );
                    return false;
                }
            }

            return true ;
        }

        private void WriteEventInLog(string eventsBody)
        {
            string line = String.Format("[{0}] {1} \n ", DateTime.Now,eventsBody);
            richTextBox3.Text += line;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                MessageBox.Show(
                        "Не выбран сетевой интерфейс!",
                        "Ошибка!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                        );

            }
            else
            {
                _fireWall.SetNetInterface(listBox1.SelectedIndex);
             
                if (!_fireWall.IsStarted)
                {
                    _fireWall.StartFireWall();
                    WriteEventInLog("Межсетевой экран запущен!");
                }
                else
                {
                    MessageBox.Show(
                         "Межсетевой экран уже запушен!",
                         "Внимание!",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Information
                         );
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_fireWall != null)
            {
                if (_fireWall.IsStarted)
                {
                    _fireWall.StopFireWall();
                    WriteEventInLog("Межсетевой экран остановлен!");
                    
                }
            }
               
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

        }
    }
}
