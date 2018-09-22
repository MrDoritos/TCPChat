using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPChatConsoleClient
{
    class ConsoleActions
    {
        static public int MultipleChoice(string title, params object[] options)
        {
            Console.WriteLine($"{title}");
            int cursortop = Console.CursorTop;
            List(options, 0, cursortop);
            int max = options.Length - 1;
            int selected = 0;

            while (true)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (selected > 0) selected--; else selected = max;
                        List(options, selected, cursortop);
                        break;
                    case ConsoleKey.DownArrow:
                        if (selected < max) selected++; else selected = 0;
                        List(options, selected, cursortop);
                        break;
                    case ConsoleKey.Enter:
                        Console.SetCursorPosition(0, cursortop);
                        Console.Clear();
                        Console.WriteLine($"Selected: {options[selected]}");
                        return selected;
                }
            }            
        }

        

        static private void List(object[] list, int selected, int cursortop, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor selectedColor = ConsoleColor.Cyan, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            var lastcursor = Console.CursorTop;
            var lastcolor = Console.ForegroundColor;
            var lastbcolor = Console.BackgroundColor;
            Console.CursorTop = cursortop;
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;

            for (int i = 0; i < list.Length; i++)
            {
                if (i == selected)
                {
                    Console.ForegroundColor = selectedColor;

                    Console.WriteLine(list[i]);

                    Console.ForegroundColor = foregroundColor;
                }
                else
                {
                    Console.WriteLine(list[i]);
                }
            }

            Console.ForegroundColor = lastcolor;
            Console.BackgroundColor = lastbcolor;
            Console.CursorTop = lastcursor;
        }
    }
}
