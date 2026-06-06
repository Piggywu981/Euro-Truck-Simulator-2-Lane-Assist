using ETS2LA.Shared;
using ETS2LA.Logging;
using ETS2LA.Backend.Events;

using System.Numerics;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using TruckLib;

namespace ETS2LA.Game.SDK;

public class BaseVehicle
{
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 Size { get; set; }

    public List<Vector3> GetCornersOnGround()
    {
        List<Vector3> corners = new List<Vector3>();
        Vector3 halfSize = Size / 2;

        corners.Add(Position + new Vector3(-halfSize.X, -halfSize.Y, -halfSize.Z));
        corners.Add(Position + new Vector3(halfSize.X, -halfSize.Y, -halfSize.Z));
        corners.Add(Position + new Vector3(halfSize.X, -halfSize.Y, halfSize.Z));
        corners.Add(Position + new Vector3(-halfSize.X, -halfSize.Y, halfSize.Z));

        Quaternion invQuat = Quaternion.Conjugate(Rotation);
        Vector3 euler = invQuat.ToEuler();
        Quaternion filteredRot = Quaternion.CreateFromYawPitchRoll(-euler.Y + (float)Math.PI, -euler.Z + (float)Math.PI, -euler.X);
        for (int i = 0; i < corners.Count; i++)
        {
            corners[i] = Vector3.Transform(corners[i] - Position, filteredRot) + Position;
        }

        return corners;
    }
}

public class TrafficTrailer : BaseVehicle
{
    public required TrafficVehicle parent;
}

public class TrafficVehicle : BaseVehicle
{
    public float speed;
    public float acceleration;
    public Int16 trailer_count;
    public Int16 id;

    // These only affect vehicles in TMP
    public bool isTMP;
    public bool isTrailer;

    public TrafficTrailer[] trailers = Array.Empty<TrafficTrailer>();
}

public class TrafficData
{
    public TrafficVehicle[] vehicles = Array.Empty<TrafficVehicle>();
}

public class TrafficProvider
{
    private static readonly Lazy<TrafficProvider> _instance = new(() => new TrafficProvider());
    public static TrafficProvider Current => _instance.Value;

    private float UpdateRate { get; set; } = 1f / 60f;
    public string EventString = "ETS2LA.Game.SDK.Traffic.Data";

    private MemoryReader? _reader;
    private TrafficData? _currentData = new();
    

    string mmapName = "Local\\ETS2LATraffic";
    string mmapNameLinux = "/dev/shm/ETS2LATraffic";
    int mmapSize = 6800;

    public TrafficProvider()
    {
        Thread updateThread = new Thread(UpdateThread)
        {
            IsBackground = true
        };
        updateThread.Start();
    }

    public TrafficData? GetCurrentTrafficData()
    {
        return _currentData;
    }

    private void UpdateThread()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        while (true)
        {
            int timeLeft = (int)((UpdateRate * 1000) - stopwatch.Elapsed.TotalMilliseconds);
            if (timeLeft > 0)
            {
                Thread.Sleep(timeLeft);
                continue;
            }

            stopwatch.Restart();
            try { Update(); }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString(), "Error in traffic update loop.");
            }
        }
    }
    
    private void Update()
    {
        if (_currentData == null)
        {
            _currentData = new TrafficData();
        }

        MemoryMappedFile? mmf = null;
        MemoryMappedViewAccessor? accessor = null;
        byte[] buffer = new byte[mmapSize];

        try
        {
            #if WINDOWS
                mmf = MemoryMappedFile.OpenExisting(mmapName);
            # else
                mmf = MemoryMappedFile.CreateFromFile(mmapNameLinux);
            # endif

            accessor = mmf.CreateViewAccessor(0, mmapSize, MemoryMappedFileAccess.Read);
            accessor.ReadArray(0, buffer, 0, mmapSize);
            _reader = new MemoryReader(buffer);
        }
        catch (FileNotFoundException)
        {
            Thread.Sleep(10000);
            _reader = null;
            return;
        }
        catch (Exception ex)
        {
            Logger.Error($"Error initializing memory mapped file: {ex.Message}");
            Thread.Sleep(10000);
            _reader = null;
            return;
        }
        finally
        {
            accessor?.Dispose();
            mmf?.Dispose();
        }


        List<TrafficVehicle> vehicles = new List<TrafficVehicle>();
        int offset = 0;
        for (int i = 0; i < 40-1; i++)
        {
            TrafficVehicle vehicle = new TrafficVehicle();

            // 0
            vehicle.Position = new Vector3(
                _reader.ReadFloat(offset),
                _reader.ReadFloat(offset + 4),
                _reader.ReadFloat(offset + 8)
            ); offset += 12;

            // 12
            vehicle.Rotation = new System.Numerics.Quaternion(
                _reader.ReadFloat(offset),
                _reader.ReadFloat(offset + 4),
                _reader.ReadFloat(offset + 8),
                _reader.ReadFloat(offset + 12)
            ); offset += 16;

            // 28
            vehicle.Size = new Vector3(
                _reader.ReadFloat(offset),     // Width
                _reader.ReadFloat(offset + 4), // Height
                _reader.ReadFloat(offset + 8)  // Length
            ); offset += 12;

            // 40
            vehicle.speed = _reader.ReadFloat(offset); offset += 4;
            vehicle.acceleration = _reader.ReadFloat(offset); offset += 4;
            vehicle.trailer_count = _reader.ReadInt16(offset); offset += 2;
            vehicle.id = _reader.ReadInt16(offset); offset += 2;

            // 52
            vehicle.isTMP = _reader.ReadBool(offset); offset += 1;
            vehicle.isTrailer = _reader.ReadBool(offset); offset += 1;

            // 54
            if(vehicle.trailer_count > 2) { vehicle.trailer_count = 2; }
            if(vehicle.trailer_count < 0) { vehicle.trailer_count = 0; }

            TrafficTrailer[] trailers = new TrafficTrailer[3];
            for (int j = 0; j < 3; j++)
            {
                // 0
                TrafficTrailer trailer = new TrafficTrailer{ parent = vehicle };
                trailer.Position = new Vector3(
                    _reader.ReadFloat(offset),
                    _reader.ReadFloat(offset + 4),
                    _reader.ReadFloat(offset + 8)
                ); offset += 12;

                // 12
                trailer.Rotation = new System.Numerics.Quaternion(
                    _reader.ReadFloat(offset),
                    _reader.ReadFloat(offset + 4),
                    _reader.ReadFloat(offset + 8),
                    _reader.ReadFloat(offset + 12)
                ); offset += 16;

                // 28
                trailer.Size = new Vector3(
                    _reader.ReadFloat(offset),     // Width
                    _reader.ReadFloat(offset + 4), // Height
                    _reader.ReadFloat(offset + 8)  // Length
                ); offset += 12;

                // 40
                trailers[j] = trailer;
            }

            vehicle.trailers = trailers;
            vehicles.Add(vehicle);
        }

        _currentData.vehicles = vehicles.ToArray();
        Events.Current.Publish<TrafficData>(EventString, _currentData);
    }
}