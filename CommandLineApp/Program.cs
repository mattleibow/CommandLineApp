using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CommandLineApp
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var cmd = new RootCommand
            {
                new Command("greeting", "Say hi.")
                {
                    new Argument<string>("name", "Your name."),
                    new Option<string?>(new[] { "--greeting", "-g" }, "The greeting to use."),
                    new Option(new[] { "--verbose", "-v" }, "Show the deets."),
                }.WithHandler(nameof(HandleGreeting)),
                new Command("echo", "Stop copying me!")
                {
                    new Command("times", "Repeat a number of times.")
                    {
                        new Argument<string>("words", "The thing you are saying."),
                        new Option<int>(new[] { "--count", "-c" }, description: "The number of times to copy you.",
                            getDefaultValue: () => 1),
                        new Option<int>(new[] { "--delay", "-d" }, description: "The delay between each echo.",
                            getDefaultValue: () => 100),
                        new Option(new[] { "--verbose", "-v" }, "Show the deets."),
                    }.WithHandler(nameof(HandleEchoTimesAsync)),
                    new Command("forever", "Just keep repeating.")
                    {
                        new Argument<string>("words", "The thing you are saying."),
                        new Option<int>(new[] { "--delay", "-d" }, description: "The delay between each echo.",
                            getDefaultValue: () => 100),
                        new Option(new[] { "--verbose", "-v" }, "Show the deets."),
                    }.WithHandler(nameof(HandleEchoForeverAsync)),
                },
            };

            return await cmd.InvokeAsync(args);
        }

        // The sub-command
        //public static int Main(string[] args)
        //{
        //    var greeting = new Command("greeting", "Say hi.")
        //    {
        //        new Argument<string>("name", "Your name."),
        //        new Option<string?>(new[] { "--greeting", "-g" }, "The greeting to use."),
        //        new Option(new[] { "--verbose", "-v" }, "Show the deets."),
        //    };
        //    greeting.Handler = CommandHandler.Create<string, string?, bool, IConsole>(HandleGreeting);
        //    var cmd = new RootCommand
        //    {
        //       greeting
        //    };
        //    return cmd.Invoke(args);
        //}

        // The basic command
        //public static int Main(string[] args)
        //{
        //    var cmd = new RootCommand
        //    {
        //        new Argument<string>("name", "Your name."),
        //        new Option<string?>(new[] { "--greeting", "-g" }, "The greeting to use."),
        //        new Option(new[] { "--verbose", "-v" }, "Show the deets."),
        //    };
        //    cmd.Handler = CommandHandler.Create<string, string?, bool, IConsole>(HandleGreeting);
        //    return cmd.Invoke(args);
        //}

        private static async Task<int> HandleEchoTimesAsync(string words, int count, int delay, bool verbose, IConsole console, CancellationToken cancellationToken)
        {
            if (count <= 0)
            {
                console.Error.WriteLine($"The count needs to be at least 1.");
                return 1;
            }

            if (delay < 0)
            {
                console.Error.WriteLine($"The delay needs to be 0 or a positive number.");
                return 1;
            }

            if (verbose)
                console.Out.WriteLine($"About to repeat '{words}' {count} time[s]...");

            for (var i = 0; i < count; i++)
            {
                console.Out.WriteLine(words);

                if (verbose)
                    console.Out.WriteLine($"Sleeping for {delay}ms...");

                await Task.Delay(delay, cancellationToken);
            }

            if (verbose)
                console.Out.WriteLine($"All done!");

            return 0;
        }

        private static async Task<int> HandleEchoForeverAsync(string words, int delay, bool verbose, IConsole console, CancellationToken cancellationToken)
        {
            if (delay < 0)
            {
                console.Error.WriteLine($"The delay needs to be 0 or a positive number.");
                return 1;
            }

            if (verbose)
                console.Out.WriteLine($"About to repeat '{words}' forever...");

            while (!cancellationToken.IsCancellationRequested)
            {
                console.Out.WriteLine(words);

                if (verbose)
                    console.Out.WriteLine($"Sleeping for {delay}ms...");

                await Task.Delay(delay, cancellationToken);
            }

            if (verbose)
                console.Out.WriteLine($"All done! Weird that it got here at all...");

            return 0;
        }

        private static void HandleGreeting(string name, string? greeting, bool verbose, IConsole console)
        {
            if (verbose)
                console.Out.WriteLine($"About to say hi to '{name}'...");

            greeting ??= "Hi";
            console.Out.WriteLine($"{greeting} {name}!");

            if (verbose)
                console.Out.WriteLine($"All done!");
        }

        // helpers

        private static Command WithHandler(this Command command, string methodName)
        {
            var method = typeof(Program).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            var handler = CommandHandler.Create(method!);
            command.Handler = handler;
            return command;
        }
    }
}
