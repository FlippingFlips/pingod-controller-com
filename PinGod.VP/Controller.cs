﻿using PinGod.VP.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PinGod.VP
{
    [ComVisible(true)]
    [GuidAttribute(ContractGuids.ControllerClass)]
    public class Controller : IController, IDisposable
    {
        byte[] _lastCoilStates;
        byte[] _lastLampStates;
        int[] _lastLedStates;
        private MemoryMap _memoryMap;
        private Process displayProcess;
        int vpHwnd;
        public Controller() { }

        public string Arguments { get; set; }
        public byte CoilCount { get; set; } = 32;
        public bool ControllerRunning { get; set; }
        public bool GameRunning { get; set; }

        #region Display Properties
        public bool DisplayAlwaysOnTop { get; set; }
        public bool DisplayFullScreen { get; set; }
        public int DisplayHeight { get; set; }
        public bool DisplayMaximized { get; set; }
        public bool DisplayOverrideOn { get; set; }
        public bool DisplayScreen { get; set; }
        public int DisplayWidth { get; set; }
        public bool DisplayWindowed { get; set; }
        public int DisplayX { get; set; }
        public int DisplayY { get; set; }
        #endregion
        public byte LampCount { get; set; } = 64;
        public byte LedCount { get; set; } = 64;
        public byte SwitchCount { get; set; } = 64;

        [DllImportAttribute("User32.dll")]
        public static extern System.IntPtr BringWindowToTop(int hWnd);

        [DllImportAttribute("User32.dll")]
        public static extern System.IntPtr SetForegroundWindow(int hWnd);

        /// <summary>
        /// Gets changed from memory map
        /// </summary>
        /// <returns>object[,2]</returns>
        public dynamic ChangedLamps()
        {
            var _lampsStates = _memoryMap.GetLampStates();
            var arr = new object[0, 2];
            if (_lastLampStates == null)
            {
                _lastLampStates = new byte[_lampsStates.Length];
                _lampsStates.CopyTo(_lastLampStates, 0);
                return arr;
            }
            if (_lampsStates != _lastLampStates)
            {
                //collect the changed coils
                List<byte> chgd = new List<byte>();
                for (int i = 0; i < _lampsStates.Length; i += 2)
                {
                    if (_lampsStates[i + 1] != _lastLampStates[i + 1])
                    {
                        chgd.Add(_lampsStates[i]);
                        chgd.Add(_lampsStates[i + 1]);
                    }
                }

                //have to convert the object array for VP, PITA
                int c = 0;
                arr = new object[chgd.Count / 2, 2];
                for (int ii = 0; ii < chgd.Count; ii += 2)
                {
                    arr[c, 0] = chgd[ii];
                    arr[c, 1] = chgd[ii + 1];
                    c++;
                }
                //Array.Copy(chgd.ToArray(), arr, chgd.Count); //Cant array copy multi dimension?
                //Buffer.BlockCopy(chgd.ToArray(), 0, arr, 0, chgd.Count);
                _lampsStates.CopyTo(_lastLampStates, 0);
                return arr;
            }
            else
            {
                return arr;
            }
        }

        /// <summary>
        /// Gets changed from memory map
        /// </summary>
        /// <returns>object[,3]</returns>
        public dynamic ChangedPDLeds()
        {
            var _ledStates = _memoryMap.GetLedStates();
            var arr = new object[0, 3];
            if (_lastLedStates == null)
            {
                _lastLedStates = new int[_ledStates.Length];
                _ledStates.CopyTo(_lastLedStates, 0);
                return null;
            }
            if (_ledStates != _lastLedStates)
            {
                //collect the changed coils
                List<int> chgd = new List<int>();
                for (int i = 0; i < _ledStates.Length; i += 3)
                {
                    //check for state and colour
                    if (_ledStates[i + 1] != _lastLedStates[i + 1] || _ledStates[i + 2] != _lastLedStates[i + 2])
                    {
                        chgd.Add(_ledStates[i]);
                        chgd.Add(_ledStates[i + 1]);
                        chgd.Add(_ledStates[i + 2]);
                    }
                }

                if (chgd.Count <= 0)
                    return null;

                //create int[,] to send back (VP)
                //Array.Copy(chgd.ToArray(), arr, chgd.Count); //Cant array copy multi dimension?

                //arr = new int[chgd.Count/3, 3];                
                //Buffer.BlockCopy(chgd.ToArray(), 0, arr, 0, chgd.Count);

                //have to convert the object array for VP, PITA
                int c = 0;
                arr = new object[chgd.Count / 3, 3];
                for (int ii = 0; ii < chgd.Count; ii += 3)
                {
                    arr[c, 0] = chgd[ii];
                    arr[c, 1] = chgd[ii + 1];
                    arr[c, 2] = chgd[ii + 2];
                    c++;
                }

                _ledStates.CopyTo(_lastLedStates, 0);
                return arr;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets changed from memory map
        /// </summary>
        /// <returns>object[,2]</returns>
        public dynamic ChangedSolenoids()
        {
            var _coilStates = _memoryMap.GetCoilStates();
            var arr = new object[0, 2];
            if (_lastCoilStates == null)
            {
                _lastCoilStates = new byte[_coilStates.Length];
                _coilStates.CopyTo(_lastCoilStates, 0);
                return arr;
            }
            if (_coilStates != _lastCoilStates)
            {
                //collect the changed coils
                List<byte> chgd = new List<byte>();
                for (int i = 0; i < _coilStates.Length; i += 2)
                {
                    if (_coilStates[i + 1] != _lastCoilStates[i + 1])
                    {
                        chgd.Add(_coilStates[i]);
                        chgd.Add(_coilStates[i + 1]);
                    }
                }

                //have to convert the object array for VP, PITA
                int c = 0;
                arr = new object[chgd.Count / 2, 2];
                for (int ii = 0; ii < chgd.Count; ii += 2)
                {
                    arr[c, 0] = chgd[ii];
                    arr[c, 1] = chgd[ii + 1];
                    c++;
                }
                //Array.Copy(chgd.ToArray(), arr, chgd.Count); //Cant array copy multi dimension?
                //Buffer.BlockCopy(chgd.ToArray(), 0, arr, 0, chgd.Count);
                _coilStates.CopyTo(_lastCoilStates, 0);
                return arr;
            }
            else
            {
                return arr;
            }
        }

        public void CreateMemoryMap(long size = 2048)
        {
            _memoryMap.CreateMemoryMap(size, coils: CoilCount, lamps: LampCount, leds: LedCount);
        }

        /// <summary> brings window handle to top and sets foreground </summary>
        public void FocusSimulator() 
        {
            BringWindowToTop(this.vpHwnd); SetForegroundWindow(this.vpHwnd); 
        }

        public int GetLamp(int lampNum) => _memoryMap?.GetLamp(lampNum) ?? 0;

        public int GetLed(int ledNum) => _memoryMap?.GetLed(ledNum) ?? 0;

        public int GetSwitch(int swNum) => _memoryMap?.GetSwitch(swNum) ?? 0;

        /// <summary>
        /// Pause the display with Switch 0 and value
        /// </summary>
        /// <param name="paused"></param>
        public void Pause(int paused)
        {
            if (paused > 0) _memoryMap.WriteGameState(GameSyncState.pause);
            else _memoryMap.WriteGameState(GameSyncState.resume);
        }

        public void Reset() => _memoryMap.WriteGameState(GameSyncState.reset);

        public void Run(int vpHwnd, string game)
        {
            string godotArgs = GetGodotArgs();
            var sinfo = new ProcessStartInfo(game, godotArgs);

            Run(vpHwnd, sinfo);
        }

        /// <summary>
        /// Providing Godot is in environment paths
        /// </summary>
        /// <param name="vpHwnd"></param>
        /// <param name="game"></param>
        public void RunDebug(int vpHwnd, string game)
        {
            string godotArgs = GetGodotArgs();
            var sinfo = new ProcessStartInfo("godot", godotArgs);

            sinfo.WorkingDirectory = game;
            Run(vpHwnd, sinfo);
        }

        /// <summary>
        /// Stop and clean up
        /// </summary>
        public void Stop()
        {
            _memoryMap.WriteGameState(GameSyncState.quit);

            Task.Delay(1000); //give a little time for the game to pick up the quit

            ControllerRunning = false;
            GameRunning = false;

            //displayProcess?.Kill();
            displayProcess?.Close();
            displayProcess = null;

            this.Dispose();
        }

        /// <summary>
        /// Writes a switch state
        /// </summary>
        /// <param name="swNum"></param>
        /// <param name="state"></param>
        public void Switch(int swNum, int state)
        {
            if (swNum >= 0 && swNum < 256)
            {
                _memoryMap.SetSwitch(swNum, (byte)state);
            }
        }

        /// <summary> build window args from custom args or display properties </summary>
        /// <returns></returns>
        private string GetGodotArgs()
        {
            string godotArgs = string.Empty;
            if (!string.IsNullOrWhiteSpace(Arguments))
                godotArgs = Arguments;
            else if (DisplayOverrideOn)
                godotArgs = BuildDisplayArguments();
            return godotArgs;
        }

        #region Private methods

        bool disposed = false;

        bool isDisposing = false;

        public void Dispose()
        {
            if (isDisposing || disposed) return;
            isDisposing = true;
            try { _memoryMap.Dispose(); disposed = true; } catch { }

        }

        /// <summary>
        /// Activates the Visual pinball player
        /// </summary>
        /// <param name="vpHwnd"></param>
        private static void ActivateVpWindow(int vpHwnd)
        {
            if (vpHwnd > 0)
                SetForegroundWindow(vpHwnd);
        }

        /// <summary>
        /// args for starting godot. <para/>
        /// no point using this anymore when the display creates an override.cfg and the user can also do this in the window now <para/>
        /// the player should run the game first then set up the window
        /// </summary>
        /// <returns></returns>
        private string BuildDisplayArguments()
        {
            var displayArgs = $"--position {DisplayX}, {DisplayY} ";
            if (DisplayWidth > 0 && DisplayHeight > 0)
                displayArgs += $"--resolution {DisplayWidth}x{DisplayHeight}";
            displayArgs = DisplayFullScreen ? displayArgs += " -f" : displayArgs;
            displayArgs = DisplayAlwaysOnTop ? displayArgs += " -t" : displayArgs;
            displayArgs = DisplayScreen ? displayArgs += $" --screen {DisplayScreen}" : displayArgs;
            displayArgs = DisplayMaximized ? displayArgs += " -m" : displayArgs;
            displayArgs = DisplayWindowed ? displayArgs += " -w" : displayArgs;
            return displayArgs;
        }

        private void Run(int vpHwnd, ProcessStartInfo startInfo)
        {
            //VP game window
            this.vpHwnd = vpHwnd;

            _memoryMap = new MemoryMap();

            //create mapping for machine states
            CreateMemoryMap();

            //send switches
            ControllerRunning = true;

            //run the game display
            displayProcess = new Process();
            displayProcess.StartInfo = startInfo;
            displayProcess.Start();

            Task.Run(() =>
            {
                while (!GameRunning)
                {
                    var gameState = _memoryMap.GetGameState();
                    if (gameState > GameSyncState.None)
                    {
                        GameRunning = true;
                        FocusSimulator();
                        break;
                    }                    
                    else
                    {
                        Task.Delay(100);
                    }                    
                }                
            });

            Task.Delay(500);
        }
        #endregion
    }
}