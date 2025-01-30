# Railroad Network Simulation Core

ECS-based railroad network simulation core system with editor tools for modeling resource transportation and logistics.

## Core Systems Architecture

### Network & Transportation
- **RailroadSystem**: Network topology and connections management
- **NavigationSystem**: Pathfinding and route calculation
- **TrainSystem**: Train movement and state management
- **CargoSystem**: Resource loading/unloading logic

### Resource Processing
- **MineSystem**: Resource extraction simulation
- **BaseSystem**: Resource processing and scoring
- **ScoreSystem**: Resource delivery tracking

### Technical Foundation
- **TransformSystem**: Entity positioning and movement
- **TimeSystem**: Simulation timing and updates
- **EntityRepository**: Entity lifecycle management

## Debug & Development Tools

### Railroad Network Editor
- Visual node creation and connection
- Node type management (Regular/Mine/Base)
- Real-time network visualization
- Save/Load functionality
- Distance measurements

### Train Management
- Train creation and configuration
- Route visualization
- Cargo status monitoring
- Real-time parameters adjustment

### Performance Features
- ECS-based architecture (LeoECS Lite)
- Native collections for memory efficiency
- Cached UI elements
- Proper resource disposal

## Technical Requirements
- Unity 2022.3+
- LeoECS Lite
- Unity Mathematics package

## Getting Started

1. Open Game scene
2. Launch Play Mode 
2. Switch to Scene View window - all debug visualization happens there

### Setting Up Railroad Network
- Select Debugger_Railroad GameObject in Hierarchy
- Use RailroadDebugger window to:
  - Load existing network from railroad-network.json
  - Create new nodes by specifying positions
  - Connect nodes to create railroad segments
  - Set node types (Regular/Mine/Base)
  - Adjust node parameters
  - Save network to file on demand

### Managing Trains
- Select Debugger_Train GameObject in Hierarchy
- Use TrainDebugger window to:
  - Create new trains
  - Select starting node (or use random)
  - Configure train parameters:
    - Move speed
    - Loading time
    - Max resource capacity
  - Monitor train states and cargo

### Monitoring Score
- Score panel is displayed in Scene View
- Shows total resources delivered to bases

## TODO & Potential Improvements

### Navigation & Pathfinding
- Consider node congestion in pathfinding
- Add traffic analysis for route optimization
- Implement dynamic path recalculation based on network load

### Resource Management
- Add node capacity limits

### Performance Optimization
- Job System integration for pathfinding