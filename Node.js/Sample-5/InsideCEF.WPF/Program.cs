using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsideCEF.WPF
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            var application = new App();
            application.InitializeComponent();
            return application.Run();
        }
    }
}
