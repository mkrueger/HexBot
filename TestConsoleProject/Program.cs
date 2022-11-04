// See https://aka.ms/new-console-template for more information
int i = 42;
float f = 32;

while (true)
{
    Console.WriteLine($"new: {i} float:{f}");
    var line = Console.ReadLine();
    if (line.StartsWith("f"))
    {
        line = line.Substring(1);
        f = float.Parse(line);
    } else
    {
        if (int.TryParse(line, out var result))
        {
            i = result;
        }
    }
}

