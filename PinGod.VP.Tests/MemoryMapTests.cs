using System.IO.MemoryMappedFiles;
using System.Threading;
using Xunit;

namespace PinGod.VP.Tests
{
    /// <summary>
    /// Integration testing. These tests run the controller without display and check all Changed methods. <para/>
    /// Coils, Lamps, LEDS
    /// </summary>
    public class MemoryMapTests
    {
        private bool created;
        private byte[] _coils;
        private byte[] _lamps;
        private int[] _leds;
        private Mutex mutex;        
        private MemoryMappedFile mmf;
        const string MAP_NAME   = "pingod_vp";
        const int MAP_SIZE = 2048;
        const string MUTEX_NAME = "pingod_vp_mutex";

        public Controller Controller { get; private set; }

        public MemoryMapTests()
        {
            SetupMapping();
        }

        private void SetupMapping()
        {
            Controller = new Controller();
            Controller.CreateMemoryMap(2048);
            created = CreateMutexAndOpenExistingMap();
            
        }

        private void SetupBuffers()
        {
            _coils = new byte[Controller.CoilCount * 2]; //coils to test with
            _lamps = new byte[Controller.LampCount * 2]; //lamps to test with
            _leds = new int[Controller.LedCount * 3]; //leds to test with

            var total = (_coils.Length) + (_lamps.Length) + (_leds.Length * 4);
        }

        [Fact]
        public void ChangedSolenoids_WithMemoryMaps()
        {
            SetupBuffers();
            var va = mmf.CreateViewAccessor(0, _coils.Length, MemoryMappedFileAccess.ReadWrite);

            //call the controller once to init states then set 23=1 and assert that change
            object[,] arrResult = Controller.ChangedSolenoids();
            _coils[0] = 23; _coils[1] = 1;
            va.WriteArray(0, _coils, 0, _coils.Length);
            arrResult = Controller.ChangedSolenoids();
            Assert.True(arrResult.Length == 2);

            //write some coil states 23=0, 24=1
            _coils[0] = 23; _coils[1] = 0;
            _coils[2] = 24; _coils[3] = 1;
            va.WriteArray(0, _coils, 0, _coils.Length);
            arrResult = Controller.ChangedSolenoids();
            Assert.True(arrResult.Length == 4);

            arrResult = Controller.ChangedSolenoids();
            Assert.True(arrResult.Length == 0);

            //Dispose
            Cleanup(va);
        }

        [Fact]
        public void ChangedLamps_WithMemoryMaps()
        {
            SetupBuffers();
            _lamps = new byte[Controller.LampCount * 2]; //lamps to test with
            var va = mmf.CreateViewAccessor(Controller.CoilCount * 2, Controller.LampCount * 2, MemoryMappedFileAccess.ReadWrite);

            //call the controller once to init states then set 23=1 and assert that change
            object[,] arrResult = Controller.ChangedLamps();
            _lamps[0] = 23; _lamps[1] = 1;
            va.WriteArray(0, _lamps, 0, _lamps.Length);
            arrResult = Controller.ChangedLamps();
            Assert.True(arrResult.Length == 2);

            //write some lamp states 23=0, 24=1
            _lamps[0] = 23; _lamps[1] = 0;
            _lamps[2] = 24; _lamps[3] = 1;
            va.WriteArray(0, _lamps, 0, _lamps.Length);
            arrResult = Controller.ChangedLamps();
            Assert.True(arrResult.Length == 4);

            arrResult = Controller.ChangedLamps();
            Assert.True(arrResult.Length == 0);

            //Dispose
            Cleanup(va);
        }

        [Fact]
        public void ChangedLeds_WithMemoryMaps()
        {
            SetupBuffers();
            var va = mmf.CreateViewAccessor(Controller.CoilCount * 2 + Controller.LampCount * 2, sizeof(int) * Controller.LedCount * 3, MemoryMappedFileAccess.ReadWrite);

            //call the controller once to init states then set 23=1 and assert that change
            object[,] arrResult = Controller.ChangedPDLeds();
            _leds[0] = 23; _leds[1] = 1; _leds[2] = 12313;
            va.WriteArray(0, _leds, 0, _leds.Length);
            arrResult = Controller.ChangedPDLeds();
            Assert.True(arrResult.Length == 3);

            //set some led colours
            _leds[0] = 23; _leds[1] = 0; _leds[2] = 12313;
            _leds[3] = 24; _leds[4] = 1; _leds[5] = 12313;
            va.WriteArray(0, _leds, 0, _leds.Length);
            arrResult = Controller.ChangedPDLeds();
            Assert.True(arrResult.Length == 6);
            Assert.True((int)arrResult[1, 2] == 12313);

            arrResult = Controller.ChangedPDLeds();
            Assert.True(arrResult== null);

            //Dispose
            Cleanup(va);
        }

        [Fact]
        public void WriteSwitch_Tests()
        {
            Controller.SwitchCount = 64;

            //create mapping with other machine items
            int[] leds = new int[192]; //leds to test with

            //offset 960
            var offset = Controller.CoilCount * 2 + Controller.LampCount * 2 + sizeof(int) * Controller.LedCount * 3;
            var va = mmf.CreateViewAccessor(offset, Controller.SwitchCount * 2, MemoryMappedFileAccess.ReadWrite);

            //set some switches
            Controller.Switch(0, 1);
            Controller.Switch(35, 1);
            Controller.Switch(45, 1);
            Controller.Switch(48, 1);

            //get the switches from memory
            byte[] buff = new byte[Controller.SwitchCount * 2];
            va.ReadArray(0, buff, 0, Controller.SwitchCount * 2);

            //check the switch is on - index
            Assert.Equal(1, buff[0]);
            Assert.Equal(1, buff[35]);
            Assert.Equal(1, buff[45]);
            Assert.Equal(1, buff[48]);
        }

        [Fact]
        public void WriteAndGetSwitch_Tests()
        {
            Controller.SwitchCount = 64;

            //create mapping with other machine items
            int[] leds = new int[192]; //leds to test with

            //offset 960
            var offset = Controller.CoilCount * 2 + Controller.LampCount * 2 + sizeof(int) * Controller.LedCount * 3;
            var va = mmf.CreateViewAccessor(offset, Controller.SwitchCount * 2, MemoryMappedFileAccess.ReadWrite);

            //set some switches
            Controller.Switch(0, 1);
            Controller.Switch(35, 1);
            Controller.Switch(45, 1);
            Controller.Switch(48, 1);

            //get the switches from memory
            byte[] buff = new byte[Controller.SwitchCount * 2];
            va.ReadArray(0, buff, 0, Controller.SwitchCount * 2);

            //check the switch is on - index
            Assert.Equal(1, buff[0]);
            Assert.Equal(1, buff[35]);
            Assert.Equal(1, buff[45]);
            Assert.Equal(1, buff[48]);

            Assert.Equal(1, Controller.GetSwitch(48));
        }

        #region Support Methods

        /// <summary>
        /// Create mutex -- MUTEX_NAME and opens existing mapping from the controller
        /// </summary>
        /// <returns></returns>
        private bool CreateMutexAndOpenExistingMap()
        {
            var created = Mutex.TryOpenExisting(MUTEX_NAME, out mutex);
            if (!created)
            {
                mutex = new Mutex(true, MUTEX_NAME, out created);
            }

            mmf = MemoryMappedFile.OpenExisting(MAP_NAME);
            return created;
        }

        private void Cleanup(MemoryMappedViewAccessor va)
        {
            if (created)
            {
                try { mutex.ReleaseMutex(); } catch { } //put here because sometimes still open
            }
            va.Dispose();
            mmf.Dispose();
            Controller.Stop();
        }
        #endregion
    }
}
