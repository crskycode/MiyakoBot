namespace MiyakoBot.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new Application();

            System.Console.CancelKeyPress += (s, e) => {
                e.Cancel = true;
                app.Stop();
            };

            app.Run();
        }
    }
}
