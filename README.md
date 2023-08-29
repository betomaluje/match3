## Simple Match 3 game based on this Unity project.

### Game mechanics

  * The game consists of m x n tiles (configurable in editor or code).

  * If three or more of the same tile type are in a horizontal row, the game removes them.

  * A tile falls down if there is no tile below it (unless it's on the bottom row).

  * The user can click on a tile. The game then removes the tile.

  * When the game starts, all the positions should be filled with tiles and nothing should fall down.

### Limitations

  * Do not use the physics engine to move the tiles.

  * Do not use Raycasting for anything except if required to detect what tile the player clicked on.

  * Apart from Unity's built-in functionality, do not use any plugin, external library, or code you didn't write yourself.