using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interpreter_Library;

namespace Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            string code = @"
program p
{
declaration
double x = 3,14;
double x1 = 2;
double y = -5;
double t;
double z1;
evaluation
t = 3 + 2;
z1 = x + x1;
output
print(z1);
}";
            MainClass mc = new MainClass();
            mc.CompileIt(code);
        }
    }
}
