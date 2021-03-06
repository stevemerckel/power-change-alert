﻿using System;
using System.Collections.Generic;

namespace PowerChangeAlerter.Common
{
    /// <summary>
    /// Holds methods and functions for interacting with a file store
    /// </summary>
    public interface IFileManager
    {
        /// <summary>
        /// Combines elements of a path together using rules based on the operating system
        /// </summary>
        /// <param name="paths">List of path elements to cobmine, in the order needed to be combined</param>
        /// <returns>The combined path</returns>
        string PathCombine(params string[] paths);

        /// <summary>
        /// Reads contents of text file
        /// </summary>
        /// <param name="fileLocation">File location to open + read</param>
        /// <returns>Contents of file</returns>
        string ReadAllText(string fileLocation);

        /// <summary>
        /// Whether the specified file exists or not
        /// </summary>
        /// <param name="fileLocation">File location</param>
        /// <returns>Boolean of <c>true</c> if found and accessible, or <c>false</c> if cannot be found or accessed</returns>
        bool FileExists(string fileLocation);

        /// <summary>
        /// Resets the URLZONE enum on a file to UNTRUSTED.  This is to unblock files from being used, which typically happens on on remotely downloaded EXEs (even those extracted from a downloaded ZIP file).  To see if a file is blocked, use the <seealso cref="IsFileBlocked(string)"/> function.
        /// </summary>
        /// <remarks>
        /// This is essentially the same as right-clicking a downloaded EXE, choosing "Properties", ticking the "Unblock" checkbox from the bottom of the modal, and clicking "Ok" or "Apply".
        /// More info is here: https://blogs.msmvps.com/bsonnino/2016/11/24/alternate-data-streams-in-c/
        /// </remarks>
        /// <param name="fileLocation">Location of the file to unblock</param>
        void UnblockFile(string fileLocation);

        /// <summary>
        /// Checks whether the URLZONE enum on a file would show this file as blocked in the File Properties.  To unblock a file, use <seealso cref="UnblockFile(string)"/> method.
        /// </summary>
        /// <remarks>
        /// <para>This is essentially the same as right-clicking a downloaded EXE, choosing "Properties", and seeing if the "Unblock" checkbox exists at the bottom of the modal.</para>
        /// <para>More info here: https://blogs.msmvps.com/bsonnino/2016/11/24/alternate-data-streams-in-c/</para>
        /// </remarks>
        /// <param name="fileLocation">Location of the file to inspect</param>
        /// <returns>Boolean indicating whether the file is blocked or not</returns>
        bool IsFileBlocked(string fileLocation);

        /// <summary>
        /// <para>Returns the parent directory path.</para>
        /// <para>If no info is available, returns <c>string.Empty</c></para>
        /// <para>If path is a root directory, returns <c>null</c></para>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetDirectoryName(string path);

        /// <summary>
        /// Returns the version of the current assembly
        /// </summary>
        Version GetExecutingAssemblyVersion();

        /// <summary>
        /// Returns the version of the target file
        /// </summary>
        /// <remarks>
        /// If file cannot be found or has an error getting the version, then NULL will be returned.
        /// </remarks>
        Version GetAssemblyVersion(string fileLocation);

        /// <summary>
        /// Returns a list of file paths (if any) based on the <paramref name="fileNamePattern"/> that are found in the <paramref name="rootDirectory"/> and any sub-directories.
        /// </summary>
        /// <param name="rootDirectory">Root directory to search</param>
        /// <param name="fileNamePattern">Pattern for file name to search (think of CMD search options)</param>
        /// <returns>Matches found (if any)</returns>
        List<string> GetFilesByPattern(string rootDirectory, string fileNamePattern);

        /// <summary>
        /// Returns the full directory path leading up to the file
        /// </summary>
        /// <param name="fileLocation">File location to pull the directory path from</param>
        /// <returns>directory path</returns>
        string PathGetDirectoryName(string fileLocation);

        /// <summary>
        /// Returns the mapped special folder to the environment
        /// </summary>
        string EnvironmentGetFolderPath(Environment.SpecialFolder specialFolder);
    }
}