# Panic Zoo (Prototype)

A modern, high-performance 3D Top-Down Crisis-Management Zoo game prototype built with Unity 6.

You play as the zoo manager — in person. Build habitats while the zoo is closed, then survive the chaos when the gates open: visitors pay per visit, stressed animals escape, and you only have two legs and limited stamina.

## 🏗️ Architecture & Features

This project focuses on **Performance**, **Clean Code**, and **Scalability**. It uses an event-driven architecture to avoid unnecessary `Update()` loops and leverages modern Unity packages.

### 1. Robust Grid System
- **O(1) Data Lookups:** Instead of spawning thousands of invisible GameObjects, the grid uses a mathematical `Dictionary<Vector2, GridSpace>` to track which cells are occupied.
- **Raycast to Plane:** Uses a mathematical infinite `Plane` at `Y=0` instead of relying on expensive `Physics Raycasts` against Box Colliders for mouse-to-world positioning.
- **Top-Down Precision:** Exact visual-to-mathematical grid alignment (`+cellSize/2f` offset) preventing edge-selection errors common in Tycoon games.

### 2. Habitat Builder
- **Click & Drag:** Classic Tycoon behavior. Left-click and drag to create huge rectangular habitats in a single fluid motion.
- **Real-Time Validation:** Displays green visual holograms for valid placements and red when colliding with pre-existing structures.

### 3. Player Avatar (The Manager)
- **Direct Character Control:** The player physically moves through the zoo to build, mitigate stress, and recapture escaped animals — distance is a gameplay resource.
- **Stamina System:** Every action costs energy. Stamina + distance form the core difficulty dial (event frequency vs. response capacity).

### 4. Modern Input & Cameras
- **New Input System:** fully decoupled input mapping controlled by a centralized `InputManager`.
- **Cinemachine 3:** Dynamic and smooth camera interpolation between gameplay (Perspective) and Build Mode (Orthographic 2D) automatically responding to `GameManager` state changes via C# Events.

## 🎮 Core Loop (Stardew-style day cycle)
1. **Night (zoo closed):** sign biome licenses, lay out habitats — your layout IS your level design for tomorrow.
2. **Day (open 8:00–18:00):** visitors pay entry; stressed animals scare them away. No building — only firefighting.
3. **Disaster:** at 100% stress an animal breaks its fence and escapes. Chase it down, recapture it, repair the fence — physically.
4. **Daily settlement at closing time:** `visitor income - (construction + upkeep + disaster losses)`. Close in the red and the Office offers an emergency loan with daily interest — true bankruptcy only if you can't cover it. Demo goal: survive 3 days; final cash is your score.

## 🛠️ Tech Stack
- **Engine:** Unity 6
- **Input:** Unity New Input System
- **Camera:** Unity.Cinemachine (Cinemachine 3)
- **UI:** TextMeshPro & UGUI
- **Data:** ScriptableObjects for all tuning values (licenses, income, stress rates, stamina)

## 🚀 Getting Started
1. Clone the repository.
2. Ensure you have **Unity 6** installed.
3. Open the main scene.
4. Press **`Tab`** to toggle Build Mode (The camera will shift to Orthographic top-down).
5. **Left-Click & Drag** the mouse over the grid to build habitats.

## 📜 Coding Conventions
- **Zero-Comments Policy:** Code must be expressive enough to explain itself through clear `PascalCase` and `camelCase` English naming conventions.
- **Single Responsibility:** Systems are isolated (e.g. `GameManager`, `InputManager`, `UIManager`).