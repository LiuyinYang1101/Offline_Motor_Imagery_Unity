using System;
using System.Linq;
using System.Text;
using Gtec.Unicorn;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class UnicornListener : MonoBehaviour
{
    private Thread unicornThread;
    const bool TestsignaleEnabled = false;
    const uint FrameLength = 1;
    const string DataFile = "data.bin";
    // Start is called before the first frame update
    void Start()
    {
        unicornThread = new Thread(startRecording) { Name = "Unicorn Listener" };
        unicornThread.IsBackground = true;
        unicornThread.Start();
    }

    void OnDestroy()
    {
        Application.Quit();
    }

    private void startRecording()
    {
        try
        {
            // Get available devices.
            //-------------------------------------------------------------------------------------

            //Get available device serials.
            IList<string> devices = Unicorn.GetAvailableDevices(true);

            if (devices.Count < 1 || devices == null)
                throw new ArgumentNullException("No device available.Please pair with a Unicorn first.");
            int deviceId = 0;

            if (deviceId >= devices.Count || deviceId < 0)
                throw new ArgumentOutOfRangeException("The selected device ID is not valid.");

            // Open selected device.
            //-------------------------------------------------------------------------------------
            print(String.Format("Trying to connect to '{0}'.", devices.ElementAt(deviceId)));
            Unicorn device = new Unicorn(devices.ElementAt(deviceId));
            print(String.Format("Connected to '{0}'.", devices.ElementAt(deviceId)));

            // Initialize acquisition members.
            //-------------------------------------------------------------------------------------
            uint numberOfAcquiredChannels = device.GetNumberOfAcquiredChannels();
            Unicorn.AmplifierConfiguration configuration = device.GetConfiguration();
            ushort samplingRate = Unicorn.SamplingRate;

            // Print acquisition configuration
            print("Acquisition Configuration: ");
            print("Sampling Rate: " + samplingRate + "Hz");
            print("Frame Length: " + FrameLength);
            print("Number Of Acquired Channels: " + numberOfAcquiredChannels);

            // Allocate memory for the acquisition buffer.
            byte[] receiveBuffer = new byte[FrameLength * sizeof(float) * numberOfAcquiredChannels];
            GCHandle receiveBufferHandle = GCHandle.Alloc(receiveBuffer, GCHandleType.Pinned);

            try
            {
                // Start data acquisition.
                //-------------------------------------------------------------------------------------
                device.StartAcquisition(TestsignaleEnabled);
                print("Data acquisition started.");

                using (BinaryWriter fileWriter = new BinaryWriter(File.Open(DataFile, FileMode.Create)))
                {
                    // Acquisition loop.
                    //-------------------------------------------------------------------------------------
                    while (ExperimentController.experimentRun)
                    {
                        // Receives the configured number of samples from the Unicorn device and writes it to the acquisition buffer.
                        device.GetData(FrameLength, receiveBufferHandle.AddrOfPinnedObject(), (uint)(receiveBuffer.Length / sizeof(float)));
                        DateTime now = DateTime.UtcNow;
                        Int64 unixTimeMilliseconds = new DateTimeOffset(now).ToUnixTimeMilliseconds();

                        // Write data to file.
                        fileWriter.Write(receiveBuffer, 0, (int)(FrameLength * 8 * sizeof(float)));
                        fileWriter.Write(unixTimeMilliseconds);
                        
                    }
                    fileWriter.Flush();
                    fileWriter.Close();
                }
                
                // Stop data acquisition.
                //-------------------------------------------------------------------------------------
                device.StopAcquisition();
                print("Data acquisition stopped.");
            }
            catch (DeviceException ex)
            {
                // Write error message to console if something goes wrong.
                PrintExceptionMessage(ex);
            }
            catch
            {
                // Write error message to console if something goes wrong.
                print("An unknown error occured.");
            }
            finally
            {
                //release allocated unmanaged resources
                receiveBufferHandle.Free();

                // Close device.
                //-------------------------------------------------------------------------------------
                device.Dispose();
                print("Disconnected from Unicorn.");
            }
        }
        catch (DeviceException ex)
        {
            // Write error message to console if something goes wrong.
            PrintExceptionMessage(ex);
        }
        catch
        {
            // Write error message to console if something goes wrong.
            print("An unknown error occured.");
        }
       
    }

    static void PrintExceptionMessage(DeviceException ex)
    {
       Debug.Log(String.Format("An error occured. Error Code: {0} - {1}", ex.ErrorCode, ex.Message));
    }
}
