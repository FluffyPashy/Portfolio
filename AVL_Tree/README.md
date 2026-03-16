# AVL Tree

A self-balancing binary search tree (AVL Tree) implemented in C# (.NET 6), featuring an interactive console interface for visualizing and manipulating the tree in real time.

---

## Features

- **Insert** a node and automatically rebalance the tree
- **Delete** a node and automatically rebalance the tree
- **Search** for a value and report whether it exists
- **4 AVL rotation cases**: Left-Left, Left-Right, Right-Right, Right-Left
- **Colored console rendering** — right branches in green (`R`), left branches in magenta (`L`)
- **Three startup modes**: default tree, random tree (10 nodes), or start from scratch
- **Text and numeric input** — commands like `add`, `remove`, `search`, `back` are accepted alongside numeric menu options

---

## Project Structure

```
AVL_Tree/
├── Program.cs          # Entry point — initializes the tree and launches the menu
└── Source/
    ├── Node.cs         # Tree node (value, left/right child, comparison delegate)
    ├── Tree.cs         # AVL Tree logic (insert, delete, search, balance, rotations)
    ├── Display.cs      # Console visualization and interactive menu
    └── Constants.cs    # All string constants, default values, and command aliases
```

---

## Getting Started

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)

### Build & Run

```bash
dotnet build
dotnet run
```

---

## Usage

On startup, choose one of three tree modes:

| Input      | Action                          |
|------------|---------------------------------|
| `1` / `default` | Load a predefined tree (`8 3 1 6 4 7 10 14 13`) |
| `2` / `random`  | Generate a random tree with 10 nodes (values 0–99) |
| `3` / `empty`   | Start with a single value you provide |
| `4` / `exit`    | Exit the program                |

Once a tree is loaded, the following actions are available:

| Input               | Action                        |
|---------------------|-------------------------------|
| `1` / `add`         | Add a new node                |
| `2` / `remove` / `delete` | Remove an existing node |
| `3` / `search` / `find`   | Search for a value      |
| `4` / `back` / `menu`     | Return to tree selection |

The tree is re-rendered after every operation, showing the current structure with colored branch labels.

---

## AVL Balancing

After every insertion or deletion the tree is rebalanced using one of four rotation strategies, selected based on the balance factor (left height − right height) of the affected node:

| Imbalance Case | Condition                                     | Rotation Applied |
|----------------|-----------------------------------------------|------------------|
| Left-Left      | Balance factor > 1, left child not right-heavy | Single right rotation |
| Left-Right     | Balance factor > 1, left child is right-heavy  | Left then right rotation |
| Right-Right    | Balance factor < -1, right child not left-heavy | Single left rotation |
| Right-Left     | Balance factor < -1, right child is left-heavy  | Right then left rotation |

---

## Code Overview

### `Node`
Holds an integer `Value` and optional `LeftChild` / `RightChild` references. Insertion into a subtree is delegated to `Node.Add()` using a comparison delegate, making the comparison logic swappable.

### `Tree`
Owns the `Root` node and exposes:
- `Add(int)` — inserts and rebalances
- `Delete(int)` — removes and rebalances recursively
- `IsValueStored(int)` — iterative BST search, returns `bool`
- `Find(Node)` — returns the matching `Node?`
- `GetHeight(Node?)` — recursive height calculation used by balance-factor logic

### `Display`
Handles all console output. `Display.Tree()` renders the tree recursively with indentation and colored branch markers. `Display.Menu()` drives the full interactive session.

### `Constants`
Central store for all UI strings, default tree values, exception messages, and text command aliases — keeping magic strings out of logic classes.

---

## Author

**Rico Rothe** — Created January 2023
