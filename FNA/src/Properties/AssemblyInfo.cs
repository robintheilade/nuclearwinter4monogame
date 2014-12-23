using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Resources;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("FNA")]
[assembly: AssemblyDescription("XNA4 Reimplementation for Desktop")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("FNA")]
[assembly: AssemblyCopyright("Copyright (c) 2011-2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Mark the assembly as CLS compliant so it can be safely used in other .NET languages
[assembly:CLSCompliant(false)]

// Allow the content pipeline assembly to access 
// some of our internal helper methods that it needs.
[assembly: InternalsVisibleTo("MonoGame.Framework.Content.Pipeline")]
[assembly: InternalsVisibleTo("MonoGame.Framework.Net")]

//Tests projects need access too
[assembly: InternalsVisibleTo("MonoGameTests")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("81119db2-82a6-45fb-a366-63a08437b485")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.0.0.1")]
[assembly: AssemblyFileVersion("0.0.0.1")]
[assembly: NeutralResourcesLanguageAttribute("en-US")]
