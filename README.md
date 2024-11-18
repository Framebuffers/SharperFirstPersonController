# SharperFirstPersonController
A cleaner implementation of [quality-first-person-controller-2](https://github.com/ColormaticStudios/quality-godot-first-person-2)
[Click here for a link to the original asset.](https://godotengine.org/asset-library/asset/2418)

## Versions:
- **1.0:** Initial release!
	- Cleanup
	- Object detection 
	- Debug features moved to a single C# class (`TextPanel`)
	- **BIG TODO: RETICLES**

## Description
An even better First Person Controller written in C# for Godot 4.3+
- **Much** cleaner and understandable file structure.
	- Project is split into 4 files, plus the debug TextPanel:
	- CameraFirstPerson.cs: contains all variables shared by the class, a separate definition for your InputMap keys, and all code related to Godot's loops.
		- CameraFirstPerson.Animation.cs: contains everything related to animation.
		- CameraFirstPerson.Camera.cs: contains everything related to camera movement, motion smoothing, and other features.
		- CameraFirstPerson.ItemDetection.cs: a new adition to this asset! Detects items with an Area3D in front of the camera's centre (where the Reticle would go).
    		- TextPanel.cs: script for the debug TextPanel.
		- Functionality is split into smaller methods. Not a lot of them, but just enough to understand what's going on, and separate functionality away from the main file. Everything is made to ease readability.
		- Code now complies (almost 100%) to C#'s code styleguides.
	- Object detection!
		- This project comes from a personal project, where I cleaned up and refactored the code. This is an extra feature that comes for free!
			- Means that some of the terminology fits my needs, which sound weird out of context. Needs a bit of work in this aspect.
		- Works by casting a ray from the camera centre to the nearest Area3D, and emits a ShowItem or HideItem signal (these are new to this asset).
		- A `HideItem()` and `ShowItem()` signal will appear on the editor. Connect anything you need: `ShowItem()` works when an `Area3D` is detected, `HideItem()` when its not.
		- In this case, I use a special Collision layer/mask pair assigned for this use. If you want to use this on your project, this is a good idea.
	- Differences:
		- Hooking up to controls.
			- The original has a `Controls` exported category of strings, on which you wrote what you had on your own Project Settings.
			- This is not ideal, and a next goal is to autodetect them, and disable functionality (like crouch, sprint, jump or pause) when it's not there.
			- In the meantime, they're neatly in a single place at the top of `CameraFirstPerson.cs`, where you can wire them up on code.

## why tho?
The original had some abstractions that tripped the `SourceGenerator` of GodotSharp. This triggered a [rather obscure bug](https://github.com/godotengine/godot/issues/71102#issuecomment-2369199135) that almost made me lose a game jam. Although I made a lot of mistakes, this came out of nowhere. I removed a lot of abstractions and `[Export]` attributes, and it worked.

Over the months I kept on working on the same camera for a remake of [Premonition](https://framebuffers.itch.io/premonition). This is the work I made there, extracyed and prepared to be used by someone else.

Maybe this can be of use to someone else, so here it is!

## to do:
 - **RETICLES!!!**
	- This one I didn't port originally, because I didn't need it at that time. But now is a good time to bring them back from the original.
- **Controls**
	- Like it was said above: the method to get `InputMap` string names has a lot of room for error. I want to try a smarter way to approach it.

## Thanks!
This codebase is possible by the work of a lot of people before me.
- [Original by Colormatic Studios.](https://github.com/ColormaticStudios/quality-godot-first-person-2) Released under MIT License.
- [Based on this asset by Zakarya on the Asset Library](https://godotengine.org/asset-library/asset/2418). Released under MIT License.
- Original translation by LokoNeko. Released under MIT License. Lost the link so, if you find it, please send a PR my way and I'll add it!

Released under MIT license.

Here's the original README below!
---
Actually good first person controller for the Godot Engine.
MIT License (credit Colormatic Studios)

This first person controller was made because there aren't many first person controllers for Godot, and the ones that do exist are pretty bad.  
It is highly customizable and comes with many features, QOL, and clean code.

Some parts came from StayAtHomeDev's FPS tutorial. You can find that [here](https://www.youtube.com/playlist?list=PLEHvj4yeNfeF6s-UVs5Zx5TfNYmeCiYwf).

# Directions
Move with WASD, space to jump, shift to sprint, C to crouch.

**FEATURES:**
- Extremely configurable
- In-air momentum
- Motion smoothing
- FOV smoothing
- Movement animations
- Crouching
- Sprinting
- 2 crosshairs/reticles, one is animated (more to come?)
- Controller/GamePad support (enabled through code, see wiki)
- In-editor tools (enable editable children to use)

If you make a cool game with this addon, I would love to hear about it!

# Wiki
**To start out**, you should probably remap all of the movement keys to your own control set.

You can make this a super basic controller by just disabling everything.

**How to add controller/GamePad support**  
- In the controls export group, there is a commented section at the end that says "Uncomment this if you want full controller support". Uncomment that block.
- Make a key map for each direction (left, right, up, down) and map them to your joystick.
- Write in these keymaps in the controls section of the player settings.
- In the `handle_head_rotation` function, there is another block of commented code that says the same thing. Uncomment that too.
- You should now be able to look around with the joystick. Make sure you add the other controls to the input map. (movement, jumping, crouching, sprinting, etc.)

**Slope/staircase:**   
Credit to @roberto-urbani23  
In the character inspector, you can uncheck Stop on Slope and set the max angle to 89 (for some reason, 90 will make the player stuck). Also Snap Length to 1 otherwise your character will not remain attached to stairs if you sprint while going downstairs.

**How to change settings:**  
Click on the character node and there should be settings in the "Feature Settings" group.

**How to add animations for a mesh:**  
- Create a function for your animation and attach it to `_physics_process` to call it every frame.
- Use `input_dir` as a boolean (it is actually a `Vector2`) to know if the player is walking.
- Use the `state` member variable to tell if the player is sprinting or crouching.
- Use the `is_on_floor` function to tell if the player is standing or falling.

**How to change reticles (crosshairs):**  
Change the "Default Reticle" setting to your reticle file.  
During runtime:  
Use the `change_reticle` function on the character.

**How to create a new reticle:**  
- Choose a reticle to base it off of.
- Open that reticle and save it as a new reticle.
- Remove the script from the reticle and create a new one. (for some reason you have to do this)
- Edit the reticle to your needs.
- Follow the "how to change reticles" directions to use it.

**How to use the editor tools:**  
- Enable editable children on the `CharacterBody` node
- Use the options in the Properties tab to change things
- These changes apply in runtime as well
