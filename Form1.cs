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
        public List<String> programasIniciando = new List<string>();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_SHOWMINIMIZED = 2;

        public Form1(GestorDeArquivos gest)
        {
            InitializeComponent();
            _gestorDeArquivos = gest;

            if (_gestorDeArquivos.JsonExiste())
            {
                listaProgramas = _gestorDeArquivos.ObterProgramas();
            }

            notifyIcon1.ContextMenuStrip = contextMenuStrip1;

            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            timer1.Interval = 1000;
            timer1.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.Visible = true;
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

                    if (processos.Length > programa.MaxProcessQuantity)
                    {
                        Debug.WriteLine($"Encerrando processos do programa: {programa.ExePath}");
                        foreach (var processo in processos)
                        {
                            processo.CloseMainWindow();
                        }
                        Thread.Sleep(5000);
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
    }
}
