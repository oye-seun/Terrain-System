# Terrain-System
This terrain system is being created as a modern alternative to Unity Terrain system. It will allow you to generate terrain mesh using noise, and will have functionalities like Triplanar mapping, Non-repeat texture tiling, Bezier path for water, texturing, mesh manipulation, and object spawning.

## Project Roadmap
the [Project Roadmap](https://github.com/users/oye-seun/projects/1) shows the current state of the project, the planned and completed features


## Creating and Removing Terrain
1. Simply create an empty object, and add the ```TerrainParent``` component to it
2. Click on ```Add Base Terrain``` to add the first block of the terrain
3. After this, click on ```Add Terrain Tile``` to toggle on and off a mode that allows you to place adjacent tiles
4. Similarly, click on ```Remove Terrain Tile``` to toggle on and off a mode that allows you to remove tiles.

https://github.com/user-attachments/assets/5c8357dc-203b-4eb3-bbff-dacaa487e6c9

## Painting Textures
1. Navigate to the brush menu in the ```TerrainParent``` inspector
2. You can add texture layers by clicking the ```+``` button
3. Populate the texture layer with the desired texture and values
4. To paint, click on the desired texture layer to select it, then click and drag the cursor over terrain to paint

https://github.com/user-attachments/assets/7ddb6406-47c2-4352-be5e-6bbe75f3906a

## Current Limitations
There are several limitations considering this tool is still in very early production stages. i recommend you to check the [Project Roadmap](https://github.com/users/oye-seun/projects/1) for the currently available features
