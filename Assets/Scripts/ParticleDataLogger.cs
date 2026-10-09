using System; // for DataTime and Exception 
using System.Globalization; // controls how the numerical values are formatted
using System.IO; // create and write CSV files
using UnityEngine; 


// Attach this logger to any GameObject and assign the existing CyclotronParticle.

public class ParticleDataLogger : MonoBehaviour
{
    [Header("Particle to Monitor")] public CyclotronParticle targetparticle;
    [Header("Logging")] [Min(0.02f)] public float loggingInterval = 1f; // restricts the logging interval to at least 0.02 seconds and logs information once every second
    public bool printToconsole = true; // console output 
    public bool saveToCSV = true; // enable or disable CSV recording 

    [Header("Physical Scale")] [Tooltip("Real-world meters represented by the reference distance in Unity units.")]
    [Min(0.000001f)] public float realDistanceMeters = 1f; // how many real-world meters a reference distance represents
    [Tooltip("Reference distance measured in Unity units.")]
    [Min(0.000001f)] public float unityDistanceUnits = 1f; // the length of that same reference distance in Unity units
    [Tooltip("Real-world seconds represented by one Unity simulation second.")]
    [Min(0.000001f)] public float realSecondsPerUnitySecond = 1f; // how much physical time one Unity simulation second represents

    private float elapsed; // tracks how much time has passed since the last log entry
    private string csvPath; // stores the location where the CSV file will be saved
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private void Start()
    {
        // checks whether or not we've assigned a particle to monitor; if not, it attempts to find one in the scene
        if (targetparticle == null)
        {
            targetparticle = FindFirstObjectByType<CyclotronParticle>(); // searches the loaded scene objects for an active object with that component
        }
        // triggers if the logger still cannot find a particle
        if (targetparticle == null)
        {
            Debug.LogError("Particle Data Logger: No Cyclotron Particle found. Assign Target Particle in the Inspector.", this); // associates the message with the logger component
            enabled = false; // disables the logger component
            return; // exits the method early to prevent further execution
        }
        if (saveToCSV)
        {
            // combines the directory with the filename
            csvPath = Path.Combine(Application.persistentDataPath, // gives Unity persistent data directory for the application
                "cyclotron_data_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
            try
            {
                File.WriteAllText(csvPath,
                    "unity_time_s,velocity_x,velocity_y,velocity_z,unity_speed_units_per_s,physical_speed_m_per_s,magnetic_force_x,magnetic_force_y,magnetic_force_z,magnetic_acceleration_x,magnetic_acceleration_y,magnetic_acceleration_z,kinetic_energy_simulation_units\n");
                Debug.Log("Particle Data Logger: CSV file: " + csvPath, this);
            }
            catch (Exception ex)
            {
                Debug.LogError("Particle Data Logger: Could not create CSV: " + ex.Message, this);
                saveToCSV = false;
            }
        }
    }

    private void LateUpdate()
    {
        if (targetparticle == null) return; // if the particle does not exist, exit the method early to prevent further execution   
        elapsed += Time.deltaTime; // measures the time that has passed since the last frame
        if (elapsed < loggingInterval) return; // if the logging interval hasn't passed, exit the method without recording anything
        elapsed = 0f; // resets the elapsed time to zero to start counting for the next logging interval

        Vector3 velocity = targetparticle.velocity; // Vector3 means it has three components and represents how quickly the particle moves along each coordinate axis
        float UnitySpeed = velocity.magnitude; // calculates the length of the velocity vector, which represents the speed of the particle in Unity units per second

        // distance AND time conversion; values MUST be calibrated to the scene
        // using Mathf.Max to avoid division by zero, which would cause a runtime error and crash the program
        float distanceScale = realDistanceMeters / Mathf.Max(unityDistanceUnits, 0.000001f); // determines how many real-world meters correspond to one Unity distance unit
        float timeScale = Mathf.Max(realSecondsPerUnitySecond, 0.000001f); // determines how many real-world seconds correspond to one Unity simulation second
        float physicalSpeed = UnitySpeed * distanceScale / timeScale; // converts the Unity speed to physical speed in meters per second

        // matches the magnetic-force calculation in the particle movement script
        Vector3 magneticForce = Vector3.Cross(velocity, targetparticle.magneticField) * targetparticle.charge; // calculates the cross product of the velocity and magnetic field vectors, then multiplies it by the particle's charge to get the magnetic force vector

        if (printToconsole)
            // displays information in Unity's console
            Debug.Log(
                $"Cyclotron t={Time.time:F2}s | Unity speed={UnitySpeed:F4} units/s | Scaled speed={physicalSpeed:F4} m/s | velocity={velocity}", this
            );
        // if the saving to CSV is disabled, exit LateUpdate
        if (!saveToCSV) return;

        // creating the measurements for CSV file
        try
        {
            string[] values = {
                F(Time.time),
                F(velocity.x), F(velocity.y), F(velocity.z),
                F(UnitySpeed), F(physicalSpeed),
                F(magneticForce.x), F(magneticForce.y), F(magneticForce.z),
            };
            File.AppendAllText(csvPath, string.Join(",", values) + "\n");
        }
        catch (Exception ex)
        {
            Debug.LogError("Particle Data Logger: CSV write failed: " + ex.Message, this);
            saveToCSV = false; // disables CSV writing to prevent further errors
        }
    }

    private static string F(float number) => number.ToString("R", Inv);
}
