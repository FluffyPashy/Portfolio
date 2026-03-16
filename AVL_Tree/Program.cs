/*
 * Name: Program.cs
 * Author: Rico Rothe
 * Created: 19. January 2023
 * Last Modified: 16. March 2023
 * License: None
 * Description: Main entry point of the AVL Tree program, where the tree is initialized and the menu is displayed for user interaction.
*/


namespace AVL_Tree
{
    public enum CompareResult { Bigger, Smaller, Equal }

    class Program
    {
        static void Main(string[] args)
        {
            // Create a delegate to compare two integers
            // Can be used to compare any type when converted to generic
            Func<int, int, CompareResult> compDel = new Func<int, int, CompareResult>((int lhs, int rhs) =>
            {
                if (lhs < rhs) return CompareResult.Smaller;
                if (lhs > rhs) return CompareResult.Bigger;
                return CompareResult.Equal;
            });

            // Create a new instance of the tree
            var tree = new Tree(compDel);

            // ! For testing purposes
            // Visualize Console program
            Display.Menu(tree);
            
            Console.WriteLine(Constants.MSG_Exit);
            Console.ReadKey();
        }
    }
}