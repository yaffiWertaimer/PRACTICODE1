using System.CommandLine;
using System.CommandLine.Invocation;


var bundleCommand = new Command("bundle", "Bundle code files into a single file");
var bundleOption = new Option<FileInfo>("--output", "file path and name")
{
    IsRequired = true,
};
var languageOption = new Option<string[]>("--language", "programming languages to include in the bundle")
;
var noteOption = new Option<bool>("--note", "whether to list the source code as a comment in the bundle file");
var sortOption = new Option<string>("--sort", "the order of copying the code files, according to the alphabet of the file name or according to the type of code")  ;
var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines", "do delete empty lines");
var authorOption = new Option<string>("--author", "registering the name of the creator of the file");
var createRspCommand = new Command("create-rsp", "create a response file with a ready command");
bundleCommand.AddOption(bundleOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);

bundleCommand.SetHandler((output, language, note, sort, remove, author) =>
{
    try
    {

        var currentDirectory = Environment.CurrentDirectory;
        var filesToBundle = Directory.GetFiles(currentDirectory)
            .Where(file => language.Contains(Path.GetExtension(file).TrimStart('.')));
        if (sort == "name")
        {
            filesToBundle = filesToBundle.OrderBy(file => Path.GetFileName(file));
        }
        else if (sort == "type")
        {
            filesToBundle = filesToBundle.OrderBy(file => Path.GetExtension(file));
        }

        using (var outputFile = File.CreateText(output.FullName))
        {
            if (note)
            {
                foreach (var file in filesToBundle)
                {
                    outputFile.WriteLine($"// Source code: {Path.GetFileName(file)} - Relative path: {Path.GetRelativePath(currentDirectory, file)}");
                    if (remove)
                    {
                        var lines = File.ReadAllLines(file).Where(line => !string.IsNullOrWhiteSpace(line));
                        outputFile.WriteLine(string.Join(Environment.NewLine, lines));
                    }
                    else
                    {
                        outputFile.WriteLine(File.ReadAllText(file));
                    }
                }
            }
            else
            {
                foreach (var file in filesToBundle)
                {
                    if (remove)
                    {
                        var lines = File.ReadAllLines(file).Where(line => !string.IsNullOrWhiteSpace(line));
                        outputFile.WriteLine(string.Join(Environment.NewLine, lines));
                    }
                    else
                    {
                        outputFile.WriteLine(File.ReadAllText(file));
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(author))
        {
            var bundleContent = File.ReadAllText(output.FullName);
            bundleContent = $"// Author: {author}{Environment.NewLine}{bundleContent}";
            File.WriteAllText(output.FullName, bundleContent);
        }

        Console.WriteLine($"Bundle created at {output.FullName}");
    
  
      
        Console.WriteLine("file was created ");
    }
    catch (DirectoryNotFoundException e)
    {
        Console.WriteLine(" Error :path is invalid");
    }


},bundleOption,languageOption,   noteOption, sortOption ,removeEmptyLinesOption, authorOption);

createRspCommand.SetHandler(() =>
{
    try
    {
        Console.Write("Enter output file path (including file name and extension): ");
        var outputPath = Console.ReadLine();

        Console.Write("Enter programming languages (separated by space) or 'all' for all languages: ");
        var languagesInput = Console.ReadLine();
        var languages = languagesInput.ToLower() == "all" ? new string[] { } : languagesInput.Split(' ');

        Console.Write("Include note in the bundle file? (true/false): ");
        var includeNoteInput = Console.ReadLine();
        bool includeNote = Convert.ToBoolean(includeNoteInput.ToLower());

        Console.Write("Sort by name or type? (name/type): ");
        var sortOption = Console.ReadLine().ToLower();

        Console.Write("Remove empty lines? (true/false): ");
        var removeEmptyLinesInput = Console.ReadLine();
        bool removeEmptyLines = Convert.ToBoolean(removeEmptyLinesInput.ToLower());

        Console.Write("Enter author's name (leave empty if not applicable): ");
        var author = Console.ReadLine();

        var currentDirectory = Environment.CurrentDirectory;
        var rspFilePath = Path.Combine(currentDirectory, "bundle.rsp");

        using (var rspFile = File.CreateText(rspFilePath))
        {
          
            rspFile.WriteLine("bundle");
            rspFile.WriteLine($"--output \"{outputPath}\"");
            rspFile.WriteLine($"--language {string.Join(" ", languages)}");
            rspFile.WriteLine($"--note {includeNote.ToString().ToLower()}");
            rspFile.WriteLine($"--sort {sortOption}");
            rspFile.WriteLine($"--remove-empty-lines {removeEmptyLines.ToString().ToLower()}");
            rspFile.WriteLine($"--author \"{author}\"");
        }

        Console.WriteLine($"Response file created at {rspFilePath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
});
var rootCommand = new RootCommand("root  command for file bundler cli");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);

rootCommand.InvokeAsync(args);
