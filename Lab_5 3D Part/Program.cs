using System;
using System.Diagnostics;
using Lab5;

namespace techdump.opengl
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            new MainWindow().Run(60);
        }
        
    }
}
