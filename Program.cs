
namespace LumpiBot
{
    class Program
    {
        public static void Main(string[] args) =>
            new LumpiBot().RunAndBlockAsync(args).GetAwaiter().GetResult();
    }
}
