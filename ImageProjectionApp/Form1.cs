using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SotevNET;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace servidorIV3
{
    public partial class Form1 : Form
    {
        
      
      

        public Form1()
        {
            InitializeComponent();

            // Oculta o formulário da barra de tarefas ao inicializar
          //  this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            notifyIcon1.Visible = true;
            notifyIcon1.Text = "Servidor IV3";
            LabVIEWExports.UDPSend(61557, "INICIAR");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
            timer1.Enabled = true;
            Hide();
            notifyIcon1.Visible = true;
        }


        private void button1_Click(object sender, EventArgs e)
        {
           
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                // Quando o formulário é minimizado, oculte-o da taskbar e exiba o ícone na área de notificação.
                notifyIcon1.Visible = true;
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                // Quando o formulário é restaurado, exiba-o novamente e oculte o ícone na área de notificação.
                notifyIcon1.Visible = false;
            }
        }
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            notifyIcon1.Visible = false;
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            int status = 0;
            string response;
            

            LabVIEWExports.UDPReceiver(61559, out response);

            string[] substrings = response.Split('#');    // PW,OO1#T2

            string programa = substrings[0]+"\r";
          
            string comando = substrings[1]+"\r";

             status =msgTCPIP("192.168.1.104", 8500, programa, out status); // seleciona programa.
            Thread.Sleep(500);

             status =msgTCPIP("192.168.1.104", 8500, comando, out status); // Aguarda resultado de testes.

            if (status==1)
            {             
                LabVIEWExports.UDPSend(61557, "OK");
            }
            else
            {
                LabVIEWExports.UDPSend(61557, "NOK");
            }

            timer1.Enabled = true;
        }

        public int msgTCPIP(string serverIP, int serverPort, string mensagem, out int status)
        {
           string  resposta = "";
            status = 0;
            try
            {



                // Criação do TcpClient e conexão com o servidor
                TcpClient client = new TcpClient();
                client.Connect(serverIP, serverPort);


                byte[] data = Encoding.ASCII.GetBytes(mensagem);

                // byte[] data = Encoding.UTF8.GetBytes(mensagem);

                // Obtém o fluxo de saída do TcpClient
                NetworkStream stream = client.GetStream();

                // Envio da mensagem
                stream.Write(data, 0, data.Length);

                // Aguardar resposta do servidor
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                resposta = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                if (resposta.Contains("NG"))
                {
                    status = 0;
                }
                else
                {
                    status = 1;
                }


                // Fechar os recursos
                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro: " + e.Message);
            }

            return status;

        }
    }
}
