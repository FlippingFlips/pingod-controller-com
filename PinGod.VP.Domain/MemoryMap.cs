﻿using System;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace PinGod.VP.Domain
{
    public class MemoryMap : IDisposable
    {
        const int MAP_SIZE = 2048;
        const string MAP_NAME = "pingod_vp";        
        const string MUTEX_NAME = "pingod_vp_mutex";

        private Mutex mutex;
        private MemoryMappedFile mmf;
        private MemoryMappedViewAccessor gameStateMap;
        private MemoryMappedViewAccessor coilsMap;
        private MemoryMappedViewAccessor lampsMap;
        private MemoryMappedViewAccessor ledsMap;
        private MemoryMappedViewAccessor switchesMap;

        byte[] _coilStates;
        byte[] _lampStates;
        int[] _ledStates;
        byte[] _switchStates;

        private bool mutexCreated = false;        

        public void CreateMemoryMap(long size = MAP_SIZE, int coils = 32, int lamps = 64, int leds = 64, int switches = 64)
        {
            if (mutex == null)
            {
                var mutexCreated = Mutex.TryOpenExisting(MUTEX_NAME, out mutex);
                if (!mutexCreated)
                {
                    mutex = new Mutex(true, MUTEX_NAME, out mutexCreated);
                }

                _coilStates = new byte[coils * 2];
                _lampStates = new byte[lamps * 2];
                _ledStates = new int[leds * 3];
                _switchStates = new byte[switches * 2];

                //Create a memory mapped file - windows. Create a mapping for each game item type
#pragma warning disable CA1416 // Validate platform compatibility
                mmf = MemoryMappedFile.CreateOrOpen(MAP_NAME, size, MemoryMappedFileAccess.ReadWrite);
#pragma warning restore CA1416 // Validate platform compatibility
                gameStateMap = mmf.CreateViewAccessor(0, 1, MemoryMappedFileAccess.ReadWrite);
                int offset = 1;
                coilsMap = mmf.CreateViewAccessor(offset, _coilStates.Length, MemoryMappedFileAccess.ReadWrite);
                offset += _coilStates.Length;
                lampsMap = mmf.CreateViewAccessor(offset, _lampStates.Length, MemoryMappedFileAccess.ReadWrite);
                offset += _lampStates.Length;
                ledsMap = mmf.CreateViewAccessor(offset, sizeof(int) * _ledStates.Length, MemoryMappedFileAccess.ReadWrite);
                offset += sizeof(int) * _ledStates.Length;
                //outgoing switches
                switchesMap = mmf.CreateViewAccessor(offset, _switchStates.Length, MemoryMappedFileAccess.ReadWrite);
            }
        }

        public byte[] GetCoilStates() 
        {
            coilsMap.ReadArray(0, _coilStates, 0, _coilStates.Length);
            return _coilStates;
        }
        public byte[] GetLampStates() 
        {
            lampsMap.ReadArray(0, _lampStates, 0, _lampStates.Length);
            return _lampStates;
        }
        public int[] GetLedStates() 
        {
            ledsMap.ReadArray(0, _ledStates, 0, _ledStates.Length);
            return _ledStates;
        }

        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                mmf?.Dispose();
                coilsMap?.Dispose();
                lampsMap?.Dispose();
                ledsMap?.Dispose();
                if (mutexCreated)
                    mutex?.ReleaseMutex();
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets switch state from the <see cref="switchesMap"/>
        /// </summary>
        /// <param name="swNum"></param>
        /// <returns></returns>
        public int GetSwitch(int swNum)
        {
            switchesMap.Read<int>(swNum, out var currState);
            return currState;
        }

        /// <summary>
        /// Sets the switch state to <see cref="switchesMap"/>
        /// </summary>
        /// <param name="swNum"></param>
        /// <param name="state"></param>
        public void SetSwitch(int swNum, byte state)
        {
            switchesMap.Write(swNum, state);
        }

        public int GetLamp(int lampNum)
        {
            lampsMap.Read<int>(lampNum, out var currState);
            return currState;
        }

        public int GetLed(int ledNum)
        {
            ledsMap.Read<int>(ledNum, out var currState);
            return currState;
        }

        /// <summary>
        /// Gets the memory mapped position 0 for game state
        /// </summary>
        /// <returns></returns>
        public GameSyncState GetGameState()
        {
            gameStateMap.Read(0, out byte state);            
            return (GameSyncState)state;
        }

        public void WriteGameState(GameSyncState state) => gameStateMap.Write(0, (byte)state);
    }
}
