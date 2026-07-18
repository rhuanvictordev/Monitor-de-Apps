using MonitorDeApps.Model;
using MonitorDeApps.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonitorDeApps
{
    public partial class Form1 : Form
    {
        public List<Programa> listaProgramas;
        public GestorDeArquivos _gestorDeArquivos;
        
        public Form1(GestorDeArquivos gest)
        {
            InitializeComponent();
            _gestorDeArquivos = gest;

            if (_gestorDeArquivos.JsonExiste())
                listaProgramas = _gestorDeArquivos.ObterProgramas();

            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            WindowState = FormWindowState.Minimized;
            
            timer1.Interval = 1000;
            timer1.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.Visible = true;
            dataGridView1.Font = new Font(dataGridView1.Font.FontFamily, 7);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Hide();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            VerificarApps();
        }

        public void VerificarApps()
        {
            foreach (Programa programa in listaProgramas)
            {
                if (programa.Name == "Exemplo_Windows (nao apagar)" || programa.Name == "Exemplo_Java (nao apagar)")
                {
                    continue;
                }

                // Controle de intervalo individual
                if ((DateTime.Now - programa.VerifiedAt).TotalMilliseconds < (programa.CheckIntervalSeconds * 1000))
                {
                    continue;
                }

                dataGridView1.Rows.Add(programa.Name, programa.ExePath, "Verificar", "-", DateTime.Now.ToString("dd-MM-yy HH:mm:ss"), $"{programa.CheckIntervalSeconds} seg");
                programa.VerifiedAt = DateTime.Now;

                Debug.WriteLine("--------------------------------");
                Debug.WriteLine($"Programa: {programa.Name}");

                bool programaRodando = false;

                if (programa.ApplicationType == "JAVA")
                {
                    programaRodando = ProcessoJavaRodando();

                    Debug.WriteLine($"Java rodando: {programaRodando}");
                }

                else
                {
                    string nomeProcesso = Path.GetFileNameWithoutExtension(programa.ExePath);

                    Debug.WriteLine($"Processo procurado: {nomeProcesso}");
                    Debug.WriteLine($"Caminho esperado: {programa.ExePath}");

                    var processos = Process.GetProcessesByName(nomeProcesso);

                    Debug.WriteLine($"Quantidade encontrada: {processos.Length}");

                    

                    if (processos.Count(p => p.MainWindowHandle != IntPtr.Zero) > programa.MaxProcessQuantity)
                    {
                        Debug.WriteLine($"Encerrando processos do programa: {programa.ExePath}");
                        foreach (var processo in processos)
                        {
                            processo.CloseMainWindow();
                        }
                        dataGridView1.Rows.Add(programa.Name, programa.ExePath, "Fechar", $"{processos.Length} Encontrados",DateTime.Now.ToString("dd-MM-yy HH:mm:ss"), $"{programa.CheckIntervalSeconds} seg");
                        return;
                    }
                    
                    
                    foreach (Process processo in processos)
                    {
                        using (processo)
                        {
                            try
                            {
                                string caminhoReal = processo.MainModule.FileName;
                                Debug.WriteLine($"Caminho real: {caminhoReal}");


                                if (string.Equals(caminhoReal, programa.ExePath, StringComparison.OrdinalIgnoreCase))
                                {
                                    programaRodando = true;
                                    Debug.WriteLine("PROGRAMA RODANDO!");
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Erro lendo processo: {ex.Message}");
                            }
                        }
                    }
                }


                Debug.WriteLine($"Está rodando: {programaRodando}");


                if (!programaRodando)
                {
                    Debug.WriteLine($"ABRINDO UPDATER: {programa.Name}");

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = programa.UpdaterExePath,
                        WindowStyle = ProcessWindowStyle.Minimized
                    };

                    Process.Start(psi);
                    dataGridView1.Rows.Add(programa.Name,programa.ExePath,"Iniciar", "Fechado", DateTime.Now.ToString("dd-MM-yy HH:mm:ss"), $"{programa.CheckIntervalSeconds} seg");
                }
            }
        }

        public bool ProcessoJavaRodando()
        {
            return Process.GetProcessesByName("javaw").Length > 0;
        }


        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void logsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.Visible = true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var opcao = MessageBox.Show("Deseja parar o monitoramento?", "Monitor de Apps", MessageBoxButtons.OKCancel);
            if (opcao == DialogResult.OK)
            {
                Application.Exit();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
