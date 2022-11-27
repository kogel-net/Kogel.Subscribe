using System;
using System.IO;
using System.Text;

namespace Kogel.Subscribe.Mssql.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //EntityCache.Register(typeof(OmsOrderDetail));
            //var codeFirst = new CodeFirst(new SqlConnection("server=192.168.159.128;user id=sa;password=P@ssw0rd,;persistsecurityinfo=True;database=KogelTest"));
            //codeFirst.SyncStructure();

            string path = $"{Directory.GetCurrentDirectory()}\\abc.txt";
            StreamWriter writer;
            if (!File.Exists(path))
                writer = File.CreateText(path);
            else
            {
                var fileSteam = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write);
                writer = new StreamWriter(fileSteam, Encoding.ASCII);
            }
            using (writer)
            {
               // writer.w("aaa1");
            }

            //SubscribeProgram.Run();

            Console.ReadLine();
        }
    }
}