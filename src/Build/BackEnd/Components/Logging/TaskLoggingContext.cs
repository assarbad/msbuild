﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Shared;

#nullable disable

namespace Microsoft.Build.BackEnd.Logging
{
    /// <summary>
    /// A logging context representing a task being built.
    /// </summary>
    internal class TaskLoggingContext : BuildLoggingContext
    {
        /// <summary>
        /// The target context in which this task is being built.
        /// </summary>
        private TargetLoggingContext _targetLoggingContext;

        /// <summary>
        /// The task instance
        /// </summary>
        private ProjectTargetInstanceChild _task;

        /// <summary>
        /// The name of the task
        /// </summary>
        private string _taskName;

        /// <summary>
        /// Constructs a task logging context from a parent target context and a task node.
        /// </summary>
        internal TaskLoggingContext(TargetLoggingContext targetLoggingContext, string projectFullPath, ProjectTargetInstanceChild task, string taskAssemblyLocation)
            : base(targetLoggingContext, CreateInitialContext(targetLoggingContext, projectFullPath, task, taskAssemblyLocation))
        {
            _targetLoggingContext = targetLoggingContext;
            _task = task;
            _taskName = GetTaskName(task);
            this.IsValid = true;
        }

        private static BuildEventContext CreateInitialContext(TargetLoggingContext targetLoggingContext,
            string projectFullPath, ProjectTargetInstanceChild task, string taskAssemblyLocation)
        {
            BuildEventContext buildEventContext = targetLoggingContext.LoggingService.LogTaskStarted2(
                targetLoggingContext.BuildEventContext,
                GetTaskName(task),
                projectFullPath,
                task.Location.File,
                task.Location.Line,
                task.Location.Column,
                taskAssemblyLocation);

            return buildEventContext;
        }

        private static string GetTaskName(ProjectTargetInstanceChild task)
        {
            ProjectTaskInstance taskInstance = task as ProjectTaskInstance;
            if (taskInstance != null)
            {
                return taskInstance.Name;
            }

            ProjectPropertyGroupTaskInstance propertyGroupInstance = task as ProjectPropertyGroupTaskInstance;
            if (propertyGroupInstance != null)
            {
                return "PropertyGroup";
            }

            ProjectItemGroupTaskInstance itemGroupInstance = task as ProjectItemGroupTaskInstance;
            if (itemGroupInstance != null)
            {
                return "ItemGroup";
            }

            return "Unknown";
        }

        /// <summary>
        /// Constructor used to support out-of-proc task host (proxy for in-proc logging service.)
        /// </summary>
        internal TaskLoggingContext(ILoggingService loggingService, BuildEventContext outOfProcContext)
            : base(loggingService, outOfProcContext, true)
        {
            this.IsValid = true;
        }

        /// <summary>
        /// Retrieves the target logging context.
        /// </summary>
        internal TargetLoggingContext TargetLoggingContext
        {
            get
            {
                return _targetLoggingContext;
            }
        }

        /// <summary>
        /// Retrieves the task node.
        /// </summary>
        internal ProjectTargetInstanceChild Task
        {
            get
            {
                return _task;
            }
        }

        /// <summary>
        /// Retrieves the task node.
        /// </summary>
        internal string TaskName
        {
            get
            {
                return _taskName;
            }
        }

        /// <summary>
        /// Log that a task has just completed
        /// </summary>
        internal void LogTaskBatchFinished(string projectFullPath, bool success)
        {
            ErrorUtilities.VerifyThrow(this.IsValid, "invalid");

            LoggingService.LogTaskFinished(
                BuildEventContext,
                _taskName,
                projectFullPath,
                _task.Location.File,
                success);
            this.IsValid = false;
        }

        /// <summary>
        /// Log a warning based on an exception
        /// </summary>
        /// <param name="exception">The exception to be logged as a warning</param>
        /// <param name="file">The file in which the warning occurred</param>
        /// <param name="taskName">The task in which the warning occurred</param>
        internal void LogTaskWarningFromException(Exception exception, BuildEventFileInfo file, string taskName)
        {
            CheckValidity();
            LoggingService.LogTaskWarningFromException(BuildEventContext, exception, file, taskName);
        }

        internal ICollection<string> GetWarningsAsErrors()
        {
            return LoggingService.GetWarningsAsErrors(BuildEventContext);
        }

        internal ICollection<string> GetWarningsNotAsErrors()
        {
            return LoggingService.GetWarningsNotAsErrors(BuildEventContext);
        }

        internal ICollection<string> GetWarningsAsMessages()
        {
            return LoggingService.GetWarningsAsMessages(BuildEventContext);
        }
    }
}
