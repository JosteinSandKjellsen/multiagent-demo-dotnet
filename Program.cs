// Copyright (c) Microsoft Corporation. All rights reserved.
// Program.cs

using AutoGen.BasicSample;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var task = new Dynamic_GroupChat_Coding_Task();
        await task.RunAsync();
    }
}