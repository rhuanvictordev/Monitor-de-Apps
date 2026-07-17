using MonitorDeApps.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonitorDeApps.Service
{
    public class GestorDeArquivos : IGestorDeArquivos
    {
        public string basePath = AppContext.BaseDirectory;

        public bool JsonExiste()
        {
            try
            {
                string arquivoJSON = Path.Combine(basePath, "programas.json");
                if (!File.Exists(arquivoJSON))
                {
                    List<Programa> lista = new List<Programa>();
                    lista.Add(new Programa("Exemplo_Windows (nao apagar)", "C:\\pasta\\pastaPrograma\\Programa.exe", "C:\\pasta\\pastaPrograma\\Updater.exe", 120, "WINDOWS", 2));
                    lista.Add(new Programa("Exemplo_Java (nao apagar)", "C:\\pasta\\pastaPrograma\\Programa.exe", "C:\\pasta\\pastaPrograma\\Updater.exe", 120, "JAVA", 1));

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    File.WriteAllText(arquivoJSON, JsonSerializer.Serialize(lista, options));
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
            return false;
        }

        public List<Programa> ObterProgramas()
        {
            try
            {
                string listaString = File.ReadAllText(Path.Combine(basePath, "programas.json"));
                var lista = JsonSerializer.Deserialize<List<Programa>>(listaString);
                return lista;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Aviso");
                return new List<Programa>();
            }
        }
    }
}
