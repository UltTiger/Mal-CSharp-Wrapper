using MalAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalCL
{
    class Program
    {
        static void Main(string[] args)
        {
            MalAPI.MalAPI api = new MalAPI.MalAPI();
            Console.WriteLine(api.ToString());
        }
    }
}
