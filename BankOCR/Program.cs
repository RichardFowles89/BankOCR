using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankOCR
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string file = @"C:\Users\seren\Desktop\BankOCRTest\BankData.txt";

                if (args.Length > 0)
                    file = args[0];

                string output = Path.GetDirectoryName(file) + @"\Accounts.txt";

                FileProcessor fileProcessor = new FileProcessor();
                fileProcessor.Read(file);
                fileProcessor.Process();
                fileProcessor.Write(output);
            }
            catch (Exception ex)
            {
                //Handle and log error
            }
        }
    }
}
