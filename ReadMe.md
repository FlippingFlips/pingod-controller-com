# pingod-controller-com
![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white) 4.72

A COM controller written in C# to send / receive pinball machine events over a windows memory mapping. This is a quick way to interop the `pingod` display with a simulator.

See the [pingod-memorymap-win](http://github.com/FlippingFlips/pingod-addons/addons/pingod-memorymap-win) plug-in in the [pingod-addons](http://github.com/FlippingFlips/pingod-addons/pingod-addons) to enable it in projects.

---
## How it works?
The `pingod-addons` have a copy of the memory mapping class. The memory map will create a `mutex` and a new memory mapping. Either will create or open the mapping depending on who was first to do so.

States are written to memory from the simulator and the game so both do read / write. See the `tests` to run through it.

---
## Controller Registry
For `Visual Pinball` and `Future Pinball` simulators that use Visual Basic Script for scripting their games installed COM objects can be accessed from the simulator scripts as long as they are registered on the system.

## Registry Installer
Use the setup registry application to simplify `regasm` usage. This can be installed anywhere but if you move the `PinGod.VP.dll` then you can re-register in another location.

![image](screen.jpg)

---
## VP Table Setup
- Tables must have a `PinMameTimer` and `PulseTimer` Timer objects to get updates from controller. This is the same as any PinMame game.

---
## Visual Pinball Setup
Visual pinball keeps machine settings and global scripts in its `Scripts` directory. Copy the following to `VisualPinball/Scripts`
|Script|Description|
|-|-|
[core_c_sharp.vbs](PinGod.VP.Domain/Scripts/core_c_sharp.vbs)| A copy of `core.vbs` compatible with C# methods on the interface.
[PinGod.vbs](PinGod.VP.Domain/Scripts/PinGod.vbs)| Base machine config for cabinet switches, flippers

---
## VP Constants

|Constant|Description|
|-|-|
|Const UseSolenoids = 1|Enable / Disable checking the solenoid states|
|Const UseLamps = 1|Enable / Disable checking the lamp states|
|Const UsePdbLeds = 1|Enable / Disable checking the led states|

*Must be excplitly set to 0 if not using them and want to save some process* See `core_c_sharp.vbs`

---
## Run Game / Display
Options here are to run `IsDebug` or from a release executable. The `IsDebug` requires the system to have `godot` in the system paths. From the controller interface you can use:

|Method|Description|
|-|-|
RunDebug|Runs `godot` with the given project directory
Run|Runs an exported game executable without debug.

_* The Game directory can also be run from a relative path, it doesn't have to be the full path._

```
RunDebug GetPlayerHWnd, GameDirectory
Run GetPlayerHWnd, GameExe
```

### Display Properties
---

```
	public bool DisplayFullScreen { get; set; }
	public int DisplayWidth { get; set; }
	public int DisplayHeight { get; set; }
	public int DisplayX { get; set; }
	public int DisplayY { get; set; }
	public bool DisplayAlwaysOnTop { get; set; }
	public bool DisplayLowDpi { get; set; }
	public bool DisplayNoWindow { get; set; }
	public bool DisplayNoBorder { get; set; }
	public bool DisplayMaximized { get; set; }
```	

Visual Pinball Example:

```
	With Controller
	.DisplayX			= 10
	.DisplayY			= 10
	.DisplayWidth 		= 512 ' 1024 Original W
	.DisplayHeight 		= 300 ' 600  Original H
	.DisplayAlwaysOnTop = True
	.DisplayFullScreen 	= False 'Providing the position is on another display it should fullscreen to window
	.DisplayLowDpi 		= False
	.DisplayNoWindow 	= False
```

### Extra Machine Items
---

By default the memory is allocated a set number of items.

- Coils = 34, Lamps = 64, Leds  = 64, Switches = 128

If you want to increase you can add properties like the above display properties.

```
With Controller
.LampCount			= 81
.LedCount			= 72
.CoilCount			= 63
```

### Pause
---

VP `Controller.Pause 1`

Runs `SetAction` to send `pause` to Godot

### SetAction
---

VP: `Controller.SetAction "my_custom_action", 1`

Create an action in the `InputMap` settings inside godot and invoke this.

### Stop
---

VP `Controller.Stop`

### Switch
---

`Controller.Switch 69, 1`

`Controller.Switch 69, 0`

`vpmPulseSw 69`