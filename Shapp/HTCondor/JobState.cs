﻿namespace Shapp {
    /// <summary>
    /// Job state enumerator.
    /// Contains values taken from https://htcondor-python.readthedocs.io/en/latest/htcondor_intro.html
    /// </summary>
    public enum JobState {
        /// <summary>
        /// Job is waiting for matchmaking or for free resource to execute.
        /// </summary>
        IDLE = 1,
        /// <summary>
        /// Job is running.
        /// </summary>
        RUNNING = 2,
        /// <summary>
        /// Job has been removed forcefully (e.g. $ condor_rm -all). It's progress has been lost.
        /// </summary>
        REMOVED = 3,
        /// <summary>
        /// The job has been successfully completed (the program ended and has transfered it's output).
        /// </summary>
        COMPLETED = 4,
        /// <summary>
        /// Job was put in held either directly by HTCondor command or because some environmental problems
        /// (like the lack of write access to working directory to store back output files generated by
        /// remote job).
        /// </summary>
        HELD = 5,
        /// <summary>
        /// It means that the HTCondor is now transfering output files from remote job.
        /// </summary>
        TRANSFERRING_OUTPUT = 6,
        /// <summary>
        /// Job execution was suspended due to temporary resource unavailability (e.g. someone has used
        /// keyboard or mouse on machine that can be also used as personal computer).
        /// </summary>
        SUSPENDED = 7
    }
}