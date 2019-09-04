# shapp
Process swarming on multiple machines as a tiny C# library based on HTCondor.

# WHat is this for?
This library allows you to use fork-like behaviour in distributed environment. You can "clone" your program and execute it on any machine connected to the pool.

## Basic usage - farming
Most basic usage is farming. The advantage here is that the whole logic against coordinating the execution of your program AND your program doing the job - is the same program, even the same file:
```csh
if (SelfSubmitter.AmIRootProcess()) {
    SubmitNewCopyOfMyselfAndWaitForStart();
    WaitForCopiesToComplete();
    if (!outputAsExpected) {
        SubmitNewCopyOfMyselfAndWaitForStart();
        SubmitNewCopyOfMyselfAndWaitForStart();
    }
    WaitForCopiesToComplete();
} else if (SelfSubmitter.AmIChildProcess()) {
    doTheJob();
}
```
Note, that this can be easily achived using regular distributed computing systems (like HTCondor, BOINC). THe only flavour is that you can use plain C# to achive the goal.

## Advanced usage - tree-based computing
If you already took a look at the API, you can noticed that distinction between "child" and "parent" is not the only way to determine the state of a clone. It is fully vialable to submit more clones from the child - and there is no hard limit for that (on the other hand, you can exaust the resources quickly this way).
```csh
if (SelfSubmitter.AmIRootProcess()) {
    SubmitNewCopyOfMyselfAndWaitForStart();
    WaitForCopiesToComplete();
    if (!outputAsExpected) {
        SubmitNewCopyOfMyselfAndWaitForStart();
        SubmitNewCopyOfMyselfAndWaitForStart();
    }
    WaitForCopiesToComplete();
} else if (SelfSubmitter.GetMyNestLevel() == 1) {
    Console.Out.WriteLine("Hello from 1st nest level");
    doTheJob();
    if (isMoreWorkToBeDone) {
        SubmitNewCopyOfMyselfAndWaitForStart();
    }
    doMoreWork();
    WaitForCopiesToComplete();
} else if (SelfSubmitter.GetMyNestLevel() == 2) {
    Console.Out.WriteLine("Hello from 2nd nest level");
    doMoreWork();
}
```
This way you can implement complex computing structures in very few lines of code. Using Shapp, you can move closer together the task and recepie to execute it using way more processing power (gaining from multiple computers connected to computing pool).
