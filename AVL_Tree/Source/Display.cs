/*
 * Name: Display.cs
 * Author: Rico Rothe
 * Created: 19. January 2023
 * Last Modified: 16. March 2023
 * License: None
 * Description: Class for displaying the AVL Tree in a structured format with links and highlights.
*/

namespace AVL_Tree
{
    public class Display
    {
        /// <summary>
        /// Displays the tree in a structured format with links and highlights
        /// </summary>
        /// <param name="node"></param>
        /// <param name="indent"></param>
        /// <param name="last"></param>
        public static void Tree(Node? node, string indent = Constants.Link_Indent, bool last = true)
        {
            //display child nodes recursively
            if(node != null)
            {    
                Console.Write(indent);
                if (last)
                {
                    PrintRightLink();
                    indent += Constants.Link_Indent;
                }
                else
                {
                    PrintLeftLink();
                    indent += Constants.Link_Indent_Child;
                }


                // highlight the node values
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine(string.Format(Constants.Result_SearchNodeValue, node.Value));
                Console.ResetColor();

                //display left child node and children
                Tree(node.LeftChild, indent, false);
                //display right child node and children
                Tree(node.RightChild, indent, true);
            }
        }

        /// <summary>
        /// Prints a right link
        /// </summary>
        private static void PrintRightLink()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(Constants.Link_Right);
            Console.ResetColor();
            Console.Write(Constants.Link_Horizontal);
        }

        /// <summary>
        /// Prints a left link
        /// </summary>
        private static void PrintLeftLink()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(Constants.Link_Left);
            Console.ResetColor();
            Console.Write(Constants.Link_Horizontal);
        }

        /// <summary>
        /// For debugging purposes and visualizing the tree
        /// </summary>
        /// <param name="tree"></param>
        public static void Menu(Tree tree)
        {
            int menu = 0;
            int choice = 0;

            while (menu != 2) //Choosing a tree to begin with
            {
                if (menu == 0)
                {
                    Console.Clear();

                    Console.WriteLine(Constants.Menu_SelectTree);
                    Console.WriteLine(Constants.Menu_SelectTreeOptions);
                    
                    // get user input and parse it (both numeric and text)
                    string userInput = Console.ReadLine() ?? "";
                    choice = ParseTreeInput(userInput);
                    
                    if (choice > 0)
                    {
                        switch (choice)
                        {
                            case 1:
                                DefaultTree(tree);
                                menu = 1;
                                break;
                            case 2:
                                RandomTree(tree, Constants.RandomTreeSize);
                                menu = 1;
                                break;
                            case 3:
                                EmptyTree(tree);
                                menu = 1;
                                break;
                            case 4:
                                menu = 2;
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine(Constants.Menu_InvalidChoice);
                        Console.ReadKey();
                    }
                }

                //if a tree has been chosen
                while (menu == 1)
                {
                    Console.Clear();
                    
                    //display all aviailable commands
                    Console.WriteLine(Constants.Menu_SelectAction);
                    Console.WriteLine(Constants.Menu_SelectActionOptions);
                    
                    Tree(tree.Root);
                    
                    // get user input and parse it (both numeric and text)
                    string userInput = Console.ReadLine() ?? "";
                    choice = ParseActionInput(userInput);
                    
                    if (choice > 0)
                    {
                        switch (choice)
                        {
                            case 1:
                                Console.WriteLine(Constants.Prompt_AddNode);
                                int value;
                                if (int.TryParse(Console.ReadLine(), out value)) tree.Add(value);
                                else Console.WriteLine(Constants.MSG_InvalidInput);
                                break;
                            case 2:
                                Console.WriteLine(Constants.Prompt_RemoveNode);
                                int target;
                                if (int.TryParse(Console.ReadLine(), out target)) tree.Delete(target);
                                else Console.WriteLine(Constants.MSG_InvalidInput);
                                break;
                            case 3:
                                Console.WriteLine(Constants.Prompt_SearchNode);
                                int search;
                                
                                if (int.TryParse(Console.ReadLine(), out search)) tree.IsValueStored(search);
                                else Console.WriteLine(Constants.MSG_InvalidInput);

                                string result = tree.IsValueStored(search) ? Constants.Result_DoesExist : Constants.Result_DoesNotExist;
                                Console.WriteLine(string.Format(Constants.Result_Search, search, result));
                                Console.ReadKey();
                                break;
                            case 4:
                                menu = 0;
                                tree = new Tree(CreateIntComparer());
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine(Constants.Menu_InvalidChoice);
                        Console.ReadKey();
                    }
                }
            }

        }

        /// <summary>
        /// Parses tree selection input from text (e.g., "default", "random", "empty") to numeric choice
        /// Supports both text commands and numeric input
        /// </summary>
        /// <param name="input">User input as string</param>
        /// <returns>Returns the numeric choice (1-4) for trees, or 0 if invalid</returns>
        private static int ParseTreeInput(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;

            string command = input.Trim().ToLower();

            if (Constants.CMD_Tree_Default.Contains(command)) return 1;
            if (Constants.CMD_Tree_Random.Contains(command)) return 2;
            if (Constants.CMD_Tree_Empty.Contains(command)) return 3;
            if (Constants.CMD_Tree_Exit.Contains(command)) return 4;

            // Also accept numeric input
            return int.TryParse(command, out int result) && result >= 1 && result <= 4 ? result : 0;
        }

        /// <summary>
        /// Parses menu action input from text (e.g., "add", "remove", "search") to numeric choice
        /// Supports both text commands and numeric input
        /// </summary>
        /// <param name="input">User input as string</param>
        /// <returns>Returns the numeric choice (1-4) for actions, or 0 if invalid</returns>
        private static int ParseActionInput(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;

            string command = input.Trim().ToLower();

            if (Constants.CMD_Action_Add.Contains(command)) return 1;
            if (Constants.CMD_Action_Remove.Contains(command)) return 2;
            if (Constants.CMD_Action_Search.Contains(command)) return 3;
            if (Constants.CMD_Action_Back.Contains(command)) return 4;

            // Also accept numeric input
            return int.TryParse(command, out int result) && result >= 1 && result <= 4 ? result : 0;
        }

        #region Tree methods

        private static Func<int, int, CompareResult> CreateIntComparer()
        {
            return (lhs, rhs) =>
            {
                if (lhs < rhs) return CompareResult.Smaller;
                if (lhs > rhs) return CompareResult.Bigger;
                return CompareResult.Equal;
            };
        }

        /// <summary>
        /// create a random tree with a given amount of elements
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="elements"></param>
        private static void RandomTree(Tree tree, int elements)
        {
            var random = new Random();
            for (int i = 0; i < elements; i++)
            {
                tree.Add(random.Next(0, 100));
            }
        }

        /// <summary>
        /// Creates a tree with a set of values
        /// </summary>
        /// <param name="tree"></param>
        private static void DefaultTree(Tree tree)
        {
            foreach (var value in Constants.DefaultTreeValues)
            {
                tree.Add(value);
            }
        }

        /// <summary>
        /// Creates an empty tree to start
        /// </summary>
        /// <param name="tree"></param>
        private static void EmptyTree(Tree tree)
        {
            Console.Write(Constants.Prompt_InitialValue);
            
            int value;
            if (int.TryParse(Console.ReadLine(), out value))
            {
                tree.Add(value);
            }
            else
            {
                Console.WriteLine(Constants.MSG_InvalidValue);
            }
        }

        #endregion


    }
}