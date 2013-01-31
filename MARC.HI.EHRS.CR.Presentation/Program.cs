﻿/**
 * Copyright 2012-2012 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 16-7-2012
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using console = System.Console;
using System.Reflection;
using System.Diagnostics;
using MARC.HI.EHRS.SVC.Core;
using MARC.HI.EHRS.SVC.Core.Services;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using MARC.HI.EHRS.SVC.Presentation.Console;
using MARC.HI.EHRS.SVC.Presentation;
using System.Collections.Specialized;
using System.ComponentModel;

namespace MARC.HI.EHRS.CR.Presentation.Console
{
    [GuidAttribute("1D5016E2-F3E7-4a33-B263-B9CDBAC20F9C")]
    class Program
    {
        /// <summary>
        /// Entry point for the console presentation layer
        /// </summary>
        static void Main(string[] args)
        {

            // Do this because loading stuff is tricky ;)
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            // Keep track of console access so we don't throw any wonky service exceptions
            bool hasConsole = true;

            // Parser for the parameters
            MohawkCollege.Util.Console.Parameters.ParameterParser<ConsoleParameters> parser = new MohawkCollege.Util.Console.Parameters.ParameterParser<ConsoleParameters>();
            try
            {
                var parameters = parser.Parse(args);

                // Help?
                if (parameters.Help)
                    parser.WriteHelp(System.Console.Out);
                else if (parameters.Interactive)
                    Console();
                else
                {
                    hasConsole = false;
                    System.ServiceProcess.ServiceBase[] ServicesToRun;
                    ServicesToRun = new System.ServiceProcess.ServiceBase[] { new SharedHealthRecord() };
                    System.ServiceProcess.ServiceBase.Run(ServicesToRun);
                }
            }
            catch (Exception e)
            {
                if (hasConsole)
                    console.Write(e.ToString());
                else
                    Trace.TraceError(e.ToString());
            }
        }

        static void Console() 
        {

            ShowCopyright();

            console.Write("Starting MARC-HI Service Framework...");

            // Initialize 
            HostContext context = new HostContext();

            Trace.CorrelationManager.ActivityId = typeof(Program).GUID;
            Trace.TraceInformation("Starting host context on Console Presentation System at {0}", DateTime.Now);

            // Detect platform
            if (System.Environment.OSVersion.Platform != PlatformID.Win32NT)
                Trace.TraceWarning("Not running on WindowsNT, some features may not function correctly");

            
            // Start the message handler service
            IMessageHandlerService messageHandlerService = null;
            try
            {

                
                Trace.TraceInformation("Getting default message handler service.");
                messageHandlerService = context.GetService(typeof(IMessageHandlerService)) as IMessageHandlerService;

                console.WriteLine("ok");
                console.Write("Starting default MessageHandler...");

                if (messageHandlerService == null)
                    Trace.TraceError("PANIC! Can't find a default message handler service: {0}", "No IMessageHandlerService classes are registered with this host context");
                else
                {
                    Trace.TraceInformation("Starting message handler service {0}", messageHandlerService);
                    if (messageHandlerService.Start())
                    {
                        console.WriteLine("ok\r\nService host console started succesfully, press any key to terminate...");
                        console.ReadKey();
                    }
                    else
                    {
                        console.WriteLine("fail");
                        Trace.TraceError("No message handler service started. Terminating program");
                    }
                }
            }
            catch(Exception e)
            {
                Trace.TraceError("Fatal exception occurred: {0}", e.ToString());
            }
            finally
            {
                context.Dispose();
                if (messageHandlerService != null)
                {
                    console.Write("Stopping listeners...");
                    Trace.TraceInformation("Stopping message handler service {0}", messageHandlerService);
                    messageHandlerService.Stop();
                    console.WriteLine("ok");
                }
            }

            
            console.WriteLine("Service Terminated, press any key to close...");
            console.ReadKey();
        }

        internal static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                if (args.Name == asm.FullName)
                    return asm;

            /// Try for an non-same number Version
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                string fAsmName = args.Name;
                if (fAsmName.Contains(","))
                    fAsmName = fAsmName.Substring(0, fAsmName.IndexOf(","));
                if (fAsmName == asm.GetName().Name)
                    return asm;
            }

            return null;
        }

        /// <summary>
        /// Show copyright information on screen
        /// </summary>
        private static void ShowCopyright()
        {
            console.WriteLine("MARC-HI Service Host Console v{0}", Assembly.GetEntryAssembly().GetName().Version);
            console.WriteLine("Copyright (C) 2010, Mohawk College of Applied Arts and Technology");
        }
    }
}
