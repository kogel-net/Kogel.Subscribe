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



            ApplicationProgram.Run();

            Console.ReadLine();


            ApplicationProgram.Close();
        }




    }
}