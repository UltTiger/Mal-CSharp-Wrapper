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

            //Console.WriteLine(api.GetWebDataRaw("http://www.google.com/"));

            

            bool val = api.ValidateUser();
            Console.WriteLine("User valid: " + val);
        }
    }
}
