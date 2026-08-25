namespace MockConverter;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Error: No arguments provided");
            return 1;
        }
        
        Dictionary<string, string> arguments = new Dictionary<string, string>();
        string? currentKey = null;

        foreach (var arg in args)
        {
            if (arg.StartsWith("--"))
            {
                currentKey = arg.ToLower();
                arguments[currentKey] = "";
            }
            else if (currentKey != null)
            {
                if (arguments[currentKey].Length > 0)
                    arguments[currentKey] += " ";

                arguments[currentKey] += arg;
            }
        }

        if (!arguments.ContainsKey("--input") || !arguments.ContainsKey("--output"))
        {
            Console.WriteLine("Error: Missing required arguments (--input and --output are required)");
            return 1; 
        }
        
        int progress = 0;
        while (progress <= 100)
        {
            Console.WriteLine($"PROGRESS {progress}");

            if (progress == 100)
            {
                bool result = Random.Shared.Next(100) < 75;
                if (result)
                {
                    Console.WriteLine("DONE OK");
                    return 0;
                }
                else
                {
                    Console.WriteLine("DONE FAIL");
                    return 1;
                }
            }

            Thread.Sleep(4000);
            progress += 10;
        }

        return 0;
    }
}