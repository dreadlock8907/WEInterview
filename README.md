# Railroad Network Editor

A Unity editor tool for creating and managing railroad networks using ECS (Entity Component System) architecture with LeoECS Lite.

## Features

- Visual node-based railroad network editor
- Scene view editing and visualization
- Support for different node types:
  - Regular nodes
  - Mine nodes
  - Base nodes
- Node connections with distance display
- Real-time network visualization

## Technical Details

The project is built using:
- Unity (2022.3+)
- [LeoECS Lite](https://github.com/Leopotam/ecslite) - Light ECS framework
- Custom editor tools and debug visualization

### Core Systems

- `RailroadUtilsSystem` - Manages railroad network topology
- `MineUtilsSystem` - Handles mine node functionality
- `BaseUtilsSystem` - Handles base node functionality
- `TransformUtilsSystem` - Manages node positions and transformations

### Debug Tools

The editor includes a comprehensive debug interface:
- Node creation and placement
- Node type management
- Connection creation and removal
- Visual properties editing
- Distance measurements
- Real-time network visualization

## Usage

1. Lauch Game scene
2. Select Debugger_Railroad GameObject
3. Use its DebuggerComponent
4. Create nodes by specifying positions
5. Select nodes to edit their properties
6. Connect nodes to create railroad segments
7. Set node types (Regular/Mine/Base) and adjust their parameters
8. Use scene view for visual editing and inspection

## Project Structure 