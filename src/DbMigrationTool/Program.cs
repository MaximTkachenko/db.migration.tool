using System;

namespace DbMigrationTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var initializer = new MigrationProcessor();
            initializer.Process(GetMode(args));

            Console.WriteLine("DONE");
            Console.Read();
        }

        static Mode GetMode(string[] args)
        {
            if (args == null || args.Length == 0 || string.IsNullOrEmpty(args[0]))
            {
                return Mode.Run;
            }

            if (!Enum.TryParse(args[0], true, out Mode parsedMode))
            {
                throw new ArgumentException("invalid mode parameter");
            }

            return parsedMode;
        }
    }
}
