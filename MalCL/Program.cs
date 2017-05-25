using MalAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MalCL
{
    class Program
    {
        static void Main(string[] args)
        {
            MalAPI.MalAPI api = new MalAPI.MalAPI();
            Console.WriteLine(api.ToString());

            //Console.WriteLine(api.GetWebDataRaw("http://www.google.com/"));

            string[] creds = File.ReadAllLines("../../../debugCreds.txt");
            api.SetCredentials(creds[0], creds[1]);

            bool val = api.ValidateUser();
            Console.WriteLine("User valid: " + val);
        }
    }
}
