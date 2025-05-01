# Skill Test GP2 README

## Overview
**Skill Test GP2** is a 3D first-person action game prototype developed in Unity.  
Players explore an environment, interact with objects, fight AI-controlled enemies, and aim to defeat a final boss.  
The project is structured to be modular and extendable as a flexible gameplay framework.

## Table of Contents
- [Key Features](#key-features)
- [Mechanics](#mechanics)
- [Installation Instructions](#installation-instructions)
- [Build](#build)
- [Credits](#credits)
- [License](#license)

## Key Features
- First-person 3D exploration and combat.
- Dynamic enemies with distinct AI behaviors (Turret and Drone).
- Object interaction system (e.g., opening doors, activating switches).
- Bullet-world interaction (e.g., shooting buttons, breaking objects).
- Object Pooling optimization for projectiles.
- UI elements for health, stamina, and score display.
- Modular and scalable architecture designed for future expansion.

## Mechanics

### Player
- Equipped with 3 different weapons.
- Can interact with the environment through shooting or activation.

### Enemies
- **Turret**: Fixed on a moving rail, follows a patrol path and attacks on sight.
- **Drone**: Inspired by *Watch Dogs: Legion* drones, uses a state machine for patrol, chase, and attack behaviors.

### World Interaction
- At least 4 different interactable elements (e.g., doors, mechanisms).
- Game progression requires using shooting and movement strategically.

### Interface
- HUD displaying health, stamina bar for sprinting, and score.
- Start Menu and Pause Menu implemented.

## Installation Instructions

### Clone the Repository
- Download the repository via ZIP or clone it using Git.

### Open in Unity Hub
- Open Unity Hub, click **Add**, and select the project folder.
- Open the project with the appropriate Unity version.

### Build the Project
- In Unity Editor, go to **File → Build Settings**.
- Select the target platform (PC recommended) and build the project.

### Play
- Run the project directly from the Unity Editor or use the built executable.

## Build
> No prebuilt release available yet. Please clone and build manually.

## Credits

### Project Lead & Programming
**Andrea Frigerio**

### Concept and Framework Architecture
**Andrea Frigerio**

## License
> Private project for skill test and educational purposes.

---

**By Psycho Garden - Andrea Frigerio**
