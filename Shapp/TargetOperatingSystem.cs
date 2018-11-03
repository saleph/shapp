namespace Shapp
{
    /// <summary>
    /// Target operating system enum. Used in NewJobSubmitter for acquiring information
    /// from the user about target system.
    /// </summary>
    public enum TargetOperatingSystem
    {
        /// <summary>
        /// Target operating system will be the same as current one.
        /// </summary>
        SAME_AS_CURRENT,
        /// <summary>
        /// The job will be executed only at windows machines.
        /// </summary>
        ONLY_WINDOWS,
        /// <summary>
        /// The job will be executed only at unix/linux machines.
        /// </summary>
        ONLY_LINUX,
        /// <summary>
        /// The job will be executed either at unix/linux of windows.
        /// </summary>
        ANY
    }
}