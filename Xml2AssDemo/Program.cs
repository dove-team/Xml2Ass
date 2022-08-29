using Xml2Ass;

namespace Xml2AssDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var xml = File.ReadAllText("200887808.xml");
            var data = DanmakuConverter.ConvertToAss(xml, 1920, 1080);
            Console.WriteLine(data);
            Console.Read();
        }
    }
}