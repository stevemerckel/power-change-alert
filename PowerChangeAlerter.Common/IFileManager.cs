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
        /// Resets the URLZONE enum on a file to UNTRUSTED.  This is to unblock files from being used, which typically happens on on remotely downloaded EXEs (even those extracted from a downloaded ZIP file).
        /// </summary>
        /// <remarks>
        /// This is essentially the same as right-clicking a downloaded EXE, choosing "Properties", ticking the "Unblock" checkbox from the bottom of the modal, and clicking "Ok" or "Apply".
        /// More info is here: https://blogs.msmvps.com/bsonnino/2016/11/24/alternate-data-streams-in-c/
        /// </remarks>
        /// <param name="fileLocation">Location of the file to unblock</param>
        void UnblockFile(string fileLocation);

        /// <summary>
        /// Combines the two paths together
        /// </summary>
        /// <param name="first">The first path</param>
        /// <param name="second">The second path</param>
        /// <returns>The combined path</returns>
        string CombinePath(string first, string second);

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
        Version GetCurrentAssemblyVersion();

        /// <summary>
        /// Returns the version of the target file
        /// </summary>
        /// <remarks>
        /// If file cannot be found or has an error getting the version, then NULL will be returned.
        /// </remarks>
        Version GetAssemblyVersion(string fileLocation);
    }
}