class SelectionMenu
{
    List<string> options;
    string title;

    public SelectionMenu(List<string> options, string title)
    {
        this.options = options;
        this.title = title;
    }

    public void SetTitle(string title)
    {
        this.title = title;
    }

    protected void PrintMenu()
    {
        Console.Out.WriteLine(title + "\n");
        for (int i = 0; i < options.Count; i++)
        {
            Console.Out.WriteLine(" {0}. {1}", i + 1, options[i]);
        }
        Console.Out.WriteLine();
    }

    public int Prompt()
    {
        if (this.options.Count == 0)
        {
            Console.WriteLine("Menu contains no options");
            return -1;
        }

        PrintMenu();

        int i;
        bool couldParse;
        do
        {
            Console.Out.Write("> ");
            couldParse = int.TryParse(Console.ReadLine(), out i);
        } 
        while (!couldParse || i <= 0 || i > options.Count);

        return i - 1;
    }
}