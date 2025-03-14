using CommandLine;
using RedEx;
using ExportOptions = RedEx.ExportOptions;

if (args.Any(x => x.Equals("export", StringComparison.InvariantCultureIgnoreCase)))
{
    await Parser.Default.ParseArguments<ExportOptions>(args).WithParsedAsync(Export.Run);
}
else if (args.Any(x => x.Equals("import", StringComparison.InvariantCultureIgnoreCase)))
{
    await Parser.Default.ParseArguments<ImportOptions>(args).WithParsedAsync(Import.Run);
}
else
{
    Console.WriteLine("Error: import/export commands are required use the 'import --help' or 'export --help'");
}