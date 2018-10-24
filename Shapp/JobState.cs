namespace Shapp
{
    public enum JobState
    {
        // values taken from https://htcondor-python.readthedocs.io/en/latest/htcondor_intro.html
        IDLE = 1,
        RUNNING = 2,
        REMOVED = 3,
        COMPLETED = 4,
        HELD = 5,
        TRANSFERRING_OUTPUT = 6,
        SUSPENDED = 7
    }
}