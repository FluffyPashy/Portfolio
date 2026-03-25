# Unity Engine Tools

A collection of custom editor tools and procedural generation systems for **Unity 6** (6000.4.0f1).

## Features

### 🏔️ Procedural Terrain Generator
Generates realistic terrains procedurally with customizable parameters:
- **HeightMapGenerator**: Creates heightmaps using noise and algorithms
- **MeshGenerator**: Converts heightmaps into optimized mesh geometry
- **ProceduralTerrain**: Main controller for terrain generation and customization

### 🎨 Prefab Painter
An editor tool for efficiently painting and placing prefabs across your scene:
- Paint prefabs directly in the scene
- Customizable brush settings
- Quick iteration and placement

### 🔄 ReplaceMe
Quick replacement tool for swapping objects with prefabs:
- Select multiple objects in the hierarchy
- Replace them with a prefab in one click
- Preserves position, rotation, and scale
- Full undo support

### 📁 FolderStrucGen
Automatic project folder structure generator:
- Creates a complete folder hierarchy with one click
- Generates standard folders: Scripts, Materials, PhysicsMaterials, etc.
- Project-specific organizational folders
- Perfect for kickstarting new projects

## Getting Started

1. Open the project in **Unity 6.0.4** or compatible version
2. Navigate to `Assets/Scripts/` to explore the tools
3. Use the editor tools through the Unity Editor UI
4. Adjust terrain and prefab parameters in the Inspector

## Project Structure

```
Assets/
├── Scripts/
│   ├── ProceduralTerrain/    # Terrain generation system
│   └── EditorTool/           # Prefab Painter editor tool
├── Scenes/                   # Example scenes
├── Prefabs/                  # Pre-built prefabs for use
└── Materials/                # Terrain and object materials
```

## Requirements

- **Unity Version**: Unity 6 (6000.4.0f1)
- **Render Pipeline**: Universal Render Pipeline (URP 17.4.0+)
- **Input System**: InputSystem 1.19.0+

## Notes

Both tools are fully integrated into the Unity Editor workflow and ready to use in your projects.
