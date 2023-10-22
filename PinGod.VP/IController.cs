using System;
using System.Runtime.InteropServices;

namespace PinGod.VP.Domain
{
    [ComVisible(true)]
    [Guid(ContractGuids.ControllerInterface)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IController
    {
        #region Display Properties

        /// <summary> Set this to true if want to override the project display settings</summary>
        bool DisplayOverrideOn { get; set; }

        /// <summary> set windows on top </summary>
        bool DisplayAlwaysOnTop { get; set; }

        /// <summary> </summary>
        bool DisplayFullScreen { get; set; }

        /// <summary> Display Height </summary>
        int DisplayHeight { get; set; }

        /// <summary> Display Width </summary>
        int DisplayWidth { get; set; }

        /// <summary> position X </summary>
        int DisplayX { get; set; }

        /// <summary> position Y </summary>
        int DisplayY { get; set; }

        /// <summary> Run window maximized </summary>
        bool DisplayMaximized { get; set; }

        /// <summary> Run windowed </summary>
        bool DisplayWindowed { get; set; }

        /// <summary> Screen number to display, position ignored if set? </summary>
        bool DisplayScreen { get; set; }
        #endregion

        #region Machine Items
        byte CoilCount { get; set; }
        byte LampCount { get; set; }
        byte LedCount { get; set; }
        byte SwitchCount { get; set; }
        #endregion

        /// <summary> custom args to send to the window, overrides display settings </summary>
        string Arguments { get; set; }

        bool GameRunning { get; set; }

        /// <summary>
        /// Get changed lamps
        /// </summary>
        /// <returns>object[i,2] Id, State</returns>
        [ComVisible(true)]
        dynamic ChangedLamps();

        /// <summary>
        /// Get changed leds
        /// </summary>
        /// <returns>object[i,3] Id, State, colour (ole)</returns>
        [ComVisible(true)]
        dynamic ChangedPDLeds();

        /// <summary>
        /// Get changed coils / solenoids receive
        /// </summary>
        /// <returns>object[i,2] Id, State</returns>
        [ComVisible(true)]
        dynamic ChangedSolenoids();
        /// <summary>
        /// Used by the implementing class. Here for testing creating maps without display
        /// </summary>
        /// <param name="size"></param>
        [ComVisible(true)]
        void CreateMemoryMap(long size = 2048);

        /// <summary>Try activate the simulator window</summary>
        [ComVisible(true)]
        void FocusSimulator();

        /// <summary>
        /// Gets lamp state
        /// </summary>
        /// <param name="lampNum"></param>
        /// <returns></returns>
        [ComVisible(true)]
        int GetLamp(int lampNum);

        /// <summary>
        /// Get led state
        /// </summary>
        /// <param name="ledNum"></param>
        /// <returns></returns>
        [ComVisible(true)]
        int GetLed(int ledNum);

        /// <summary>
        /// Gets a switches state
        /// </summary>
        /// <param name="swNum"></param>
        /// <returns></returns>
        [ComVisible(true)]
        int GetSwitch(int swNum);

        /// <summary>
        /// Pause the display
        /// </summary>
        /// <param name="paused"></param>
        [ComVisible(true)]
        void Pause(int paused);

        /// <summary>
        /// Resets the game
        /// </summary>
        [ComVisible(true)]
        void Reset();

        /// <summary>
        /// Runs a packaged game, no debug (if set by developer on export)
        /// </summary>
        /// <param name="vpHwnd"></param>
        /// <param name="game"></param>
        [ComVisible(true)]
        void Run(int vpHwnd, string game);

        /// <summary>
        /// Runs the game from it's source directory. Godot needs to be in environment
        /// </summary>
        /// <param name="vpHwnd"></param>
        /// <param name="game"></param>
        [ComVisible(true)]
        void RunDebug(int vpHwnd, string game);

        /// <summary>
        /// Stops and kills the display
        /// </summary>
        [ComVisible(true)]
        void Stop();

        /// <summary>
        /// Writes a switch event /sw to be picked up by InputMap in Godot
        /// </summary>
        /// <param name="swNum"></param>
        /// <param name="state"></param>
        [ComVisible(true)]
        void Switch(int swNum, int state);
    }
}
