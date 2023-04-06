<h2 align="center">ball game</h2>
<p align="center"><b>a small mobile game made in C# with the Unity engine</b><br><em>made by <a href="https://cloud-worm.github.io/" target="_blank">Cloud Worm</a> in 2023</em></p>

<br>

### The Game

Ball Game is a physics-based game. You control a ball that you can shoot in any direction, with variable force; however, you may only shoot when the ball is touching something else, like a wall or the floor. You also have a limited amount of shots for each level, meaning that you must time each shot perfectly right.

Different materials will have different effects on the ball — while the regular, black walls does not bounce, pink walls will send your ball flying away. The game is all about aiming and timing your shots correctly to attain the final objective.

### Development

The game was created using the [Unity game engine](https://unity.com/) (version 22.2.6f1), and therefore the language C#. The source code is available at the [official GitHub repository](https://github.com/cloud-worm/ball-game), and is licensed under the GNU General Public License.

The game is fully free and has no ads or microtransactions or whatnot. The source code is also written in a way that's easy to understand for beginners, and is elegantly commented for easier reading.

### Download & Install

There are two ways to get the game:
+ You can get the latest Android `.apk` build from the [GitHub releases page](https://github.com/cloud-worm/ball-game/releases).
+ Alternatively, you can build the source by downloading it from said release page, or get the current development commit by running:

```bash
$ git clone "https://github.com/cloud-worm/ball-game"
```

+ Building from source allows building towards iOS, desktop, etc. Note that you will need to have Unity installed.
+ Hopefully I'll get it published on Google Play soon. iOS will probably have to wait a little longer though 😊

### Unity nerd stuff

Several tricks are used in the code to make it more performant and easier to read — in fact, creating clear, efficient code was of my main goals here, since my scripts sometimes end up unreadable. For example, instead of using a bunch of booleans and therefore having many unnecessary binary operations, I used a simple `enum` to represent each state of the player control:

```csharp
public enum State
{
    Playing,
    Ended,
    CanRestart,
}
```

Each state corresponds to what the player can do at a given time — `Ended` is when they lose, for example, and `CanRestart` is when they can touch the screen to try a failed level again.

To access specific data for each level (such as the amount of shots the player has), I have created a class called `LevelDetails`, which contains public variables that I can access in the `Player.cs` script and methods that allow me to fetch level data. This is probably not optimal, but it works well and is quite practical. Here's an example:

```csharp
public class LevelDetails
{
    public int number; // Level number
	
    [HideInInspector]
    public Dictionary<int, int> levelAttempts = new Dictionary<int, int>()
    {
        { 1, 1 },
        { 2, 2 },
        { 3, 3 },
    };
    // [...]
}
```

This class also boasts the following function, facilitating data retrieval:

```csharp
public int NumAttempts() { return levelAttempts[number]; }
```
