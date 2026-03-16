/*
 * Name: Constants.cs
 * Author: Rico Rothe
 * Created: 19. January 2023
 * Last Modified: 16. March 2023
 * License: None
 * Description: Static class to store constant data for the program, such as messages and display strings.
*/

using System.Diagnostics.Contracts;

namespace AVL_Tree
{
    public static class Constants
    {
        // Default Tree values
        public static readonly int[] DefaultTreeValues = new int[] { 8, 3, 1, 6, 4, 7, 10, 14, 13 };
        public const int RandomTreeSize = 10;

        // Message Strings
        public const string MSG_Exit = "Press any key to exit...";
        public const string MSG_InvalidInput = "Invalid input!";
        public const string MSG_InvalidValue = "Invalid value";

        // Tree strings
        public const string MSG_Tree_Empty = "The tree is empty.";
        

        // Exeption messages
        public const string EXC_Rotation_LeftLeft = "Left child is required for left-left rotation.";
        public const string EXC_Rotation_RightRight = "Right child is required for right-right rotation.";
        public const string EXC_Rotation_LeftRight = "Left child is required for left-right rotation.";
        public const string EXC_Rotation_RightLeft = "Right child is required for right-left rotation.";
        
        // Display related data
        public const string Link_Left = "L";
        public const string Link_Right = "R";
        public const string Link_Horizontal = "----";
        public const string Link_Indent = "     ";
        public const string Link_Indent_Child = "|    ";

        // Menu strings
        public const string Menu_SelectTree = "Choose a tree to begin with:";
        public const string Menu_SelectTreeOptions = "1. Default tree\n2. Random tree\n3. Empty tree\n4. Exit";
        public const string Menu_InvalidChoice = "Invalid choice";
        public const string Menu_SelectAction = "Choose an action:";
        public const string Menu_SelectActionOptions = "1. Add a node\n2. Remove a node\n3. Search for a node\n4. Return to menu\n\n";
        
        // Prompt strings
        public const string Prompt_AddNode = "Enter a node value to add: ";
        public const string Prompt_RemoveNode = "Enter a node value to remove: ";
        public const string Prompt_SearchNode = "Enter a node value to search for: ";
        public const string Prompt_InitialValue = "Enter a value(int) to begin with: ";
        
        // Result strings
        public const string Result_DoesExist = "does";
        public const string Result_DoesNotExist = "does not";
        public const string Result_SearchNodeValue = " {0} ";
        public const string Result_Search = "{0} {1} exist in the tree.";

        // Tree selection commands
        public static readonly string[] CMD_Tree_Default = new[] { "default" };
        public static readonly string[] CMD_Tree_Random = new[] { "random" };
        public static readonly string[] CMD_Tree_Empty = new[] { "empty" };
        public static readonly string[] CMD_Tree_Exit = new[] { "exit", "quit" };

        // Action commands
        public static readonly string[] CMD_Action_Add = new[] { "add" };
        public static readonly string[] CMD_Action_Remove = new[] { "remove", "delete" };
        public static readonly string[] CMD_Action_Search = new[] { "search", "find" };
        public static readonly string[] CMD_Action_Back = new[] { "back", "menu", "exit" };
    }
}