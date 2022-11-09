# Platformer Controller

A series of C# scripts for Unity that create and define a prototype for a platformer controller. I pulled this from one of my prototype games, but have never really used it.

## How to use

**Note: Unity's new Input System package is required. If this is not desired, please make changes in `PlayerInputHandler.cs`.**

1) Attach the `CharacterController2D`, `PlayerInputHandler`, and `PrototypePlayer` scripts to a game object.
2) Create a new `PlayerData` object in your Assets directory. Adjust settings
3) Attach this object to your `PrototypePlayer`
4) Configure your layers, etc. so that things collide!
5) Voila! You now have a basic platforming character.

I didn't have time to clean this up, so this is just released as is. I don't mind how you use it :)

## Credits

The `CharacterController2D` script is a modified version of prime31's own CharacterController2D, which may be found [here]([https://](https://github.com/prime31/CharacterController2D))! (They also have some more thorough explanations on how to configure the character controller.)
