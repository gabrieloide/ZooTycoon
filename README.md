# Zoo Tycoon (Prototype)

A modern, high-performance 3D Top-Down Zoo Management game prototype built with Unity 6. 

## 🏗️ Architecture & Features

This project focuses on **Performance**, **Clean Code**, and **Scalability**. It uses an event-driven architecture to avoid unnecessary `Update()` loops and leverages modern Unity packages.

### 1. Robust Grid System
- **O(1) Data Lookups:** Instead of spawning thousands of invisible GameObjects, the grid uses a mathematical `Dictionary<Vector2, GridSpace>` to track which cells are occupied.
- **Raycast to Plane:** Uses a mathematical infinite `Plane` at `Y=0` instead of relying on expensive `Physics Raycasts` against Box Colliders for mouse-to-world positioning.
- **Top-Down Precision:** Exact visual-to-mathematical grid alignment (`+cellSize/2f` offset) preventing edge-selection errors common in Tycoon games.

### 2. Habitat Builder
- **Click & Drag:** Classic Tycoon behavior. Left-click and drag to create huge rectangular habitats in a single fluid motion.
- **Real-Time Validation:** Displays green visual holograms for valid placements and red when colliding with pre-existing structures. 

### 3. Modern Input & Cameras
- **New Input System:** fully decoupled input mapping controlled by a centralized `InputManager`.
- **Cinemachine 3:** Dynamic and smooth camera interpolation between gameplay (Perspective) and Build Mode (Orthographic 2D) automatically responding to `GameManager` state changes via C# Events.

## 🛠️ Tech Stack
- **Engine:** Unity 6
- **Input:** Unity New Input System
- **Camera:** Unity.Cinemachine (Cinemachine 3)
- **UI:** TextMeshPro & UGUI

## 🚀 Getting Started
1. Clone the repository.
2. Ensure you have **Unity 6** installed.
3. Open the main scene.
4. Press **`Tab`** to toggle Build Mode (The camera will shift to Orthographic top-down).
5. **Left-Click & Drag** the mouse over the grid to build habitats.

## 📜 Coding Conventions
- **Zero-Comments Policy:** Code must be expressive enough to explain itself through clear `PascalCase` and `camelCase` English naming conventions.
- **Single Responsibility:** Systems are isolated (e.g. `GameManager`, `InputManager`, `UIManager`).
