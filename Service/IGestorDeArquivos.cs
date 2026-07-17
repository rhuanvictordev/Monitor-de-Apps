using MonitorDeApps.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorDeApps.Service
{
    public interface IGestorDeArquivos
    {
        bool JsonExiste();
        List<Programa> ObterProgramas();
    }
}
