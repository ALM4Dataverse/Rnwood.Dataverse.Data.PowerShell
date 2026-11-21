using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Threading;
using System.Diagnostics;


#if NETCOREAPP
using System.Runtime.Loader;
#endif

namespace Rnwood.Dataverse.Data.PowerShell.FrameworkSpecific.Loader
{
    public class ModuleInitProvider : IModuleAssemblyInitializer
    {
        public void OnImport()
        {

#if NET
            // The manifest loads net8.0 loader for all Core editions, but the cmdlets are loaded
            // from the runtime-appropriate TFM folder (net9.0 for .NET 9, net10.0 for .NET 10, etc.).
            // Derive the cmdlets base path from the actual .NET runtime version so that dependency
            // resolution picks up the matching TFM's assemblies, falling back to net8.0 if no
            // specific folder exists for a newer runtime.
            string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../cmdlets";
            var runtimeMajor = Environment.Version.Major;
            string runtimeTfm = runtimeMajor >= 10 ? ("net" + runtimeMajor + ".0")
                               : runtimeMajor == 9  ? "net9.0"
                               :                      "net8.0";
            // Fall back to net8.0 if the runtime-specific folder does not exist (e.g. future .NET versions)
            var runtimeTfmPath = Path.Combine(basePath, runtimeTfm);
            var targetFramework = Directory.Exists(runtimeTfmPath) ? runtimeTfm : "net8.0";
            basePath = Path.Combine(basePath, targetFramework);

            var alc = new CmdletsLoadContext(basePath);

            AssemblyLoadContext.Default.Resolving += (s, args) =>
            {
                // Guard against null or empty assembly names. PowerShell (7.5+) can fire
                // the Resolving event with a null Name when loading certain internal types
                // (e.g. via Set-StrictMode). new AssemblyName(null/empty) throws
                // ArgumentNullException/ArgumentException which propagates as a test failure.
                if (string.IsNullOrEmpty(args.Name))
                {
                    return null;
                }

                AssemblyName assemblyName = new AssemblyName(args.Name);

                // Don't try to resolve satellite assemblies (culture-specific resource assemblies)
                // These are for localization and should be allowed to fail gracefully
                if (assemblyName.CultureName != null && !string.IsNullOrEmpty(assemblyName.CultureName) && assemblyName.CultureName != "neutral")
                {
                    return null;
                }

                string path = Path.Combine(basePath, assemblyName.Name + ".dll");

                if (File.Exists(path))
                {
                    var fileVersion = FileVersionInfo.GetVersionInfo(path);
                    // Strip SemVer build metadata (+sha) and pre-release labels (-beta) from ProductVersion
                    // before parsing, as System.Version doesn't support SemVer suffixes.
                    // Find the first suffix character in the original string and truncate there.
                    var productVersionStr = fileVersion.ProductVersion;
                    if (productVersionStr != null)
                    {
                        int plusIdx = productVersionStr.IndexOf('+');
                        int dashIdx = productVersionStr.IndexOf('-');
                        int suffixIdx = (plusIdx >= 0 && dashIdx >= 0) ? Math.Min(plusIdx, dashIdx)
                                        : (plusIdx >= 0 ? plusIdx : dashIdx);
                        if (suffixIdx >= 0) productVersionStr = productVersionStr.Substring(0, suffixIdx);
                    }
                    if (assemblyName.Version == null ||
                        (Version.TryParse(productVersionStr, out var parsedVersion) && parsedVersion >= assemblyName.Version))
                    {
                        if (assemblyName.Name.StartsWith("Microsoft.Extensions.", StringComparison.OrdinalIgnoreCase))
                        {
                            // Load the bundled fallback directly after the default context failed to resolve it.
                            return alc.LoadFromAssemblyPath(path);
                        }

                        return alc.LoadFromAssemblyName(assemblyName);
                    }
                }

                return null;
            };

            // Eagerly load SDK assemblies that PowerShell scripts commonly reference with New-Object or type literals
            // (e.g. [Microsoft.Crm.Sdk.Messages.WhoAmIRequest] or New-Object Microsoft.Crm.Sdk.Messages.PublishXmlRequest).
            // PS7's type resolver only searches AssemblyLoadContext.Default.Assemblies. Assemblies loaded only inside
            // CmdletsLoadContext (as dependencies of ServiceClient, etc.) are not visible there. We load them directly
            // into DEFAULT ALC here so PS type resolution can find them at any point in the script lifecycle.
            // CmdletsLoadContext.Load() must also exclude these names so that ServiceClient uses the same DEFAULT ALC
            // copy, keeping the type identity consistent (one copy, no duplicate-ALC issues).
            var sdkAssembliesToPreload = new[] { "Microsoft.Crm.Sdk.Proxy" };
            foreach (var asmName in sdkAssembliesToPreload)
            {
                var asmPath = Path.Combine(basePath, asmName + ".dll");
                if (File.Exists(asmPath) &&
                    !AssemblyLoadContext.Default.Assemblies.Any(a => string.Equals(a.GetName().Name, asmName, StringComparison.OrdinalIgnoreCase)))
                {
                    try { AssemblyLoadContext.Default.LoadFromAssemblyPath(asmPath); }
                    catch (FileLoadException) { /* Assembly or a conflicting version already loaded — safe to skip */ }
                    catch (BadImageFormatException) { /* Not a valid .NET assembly for this runtime — skip */ }
                }
            }

#else
			string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../cmdlets/net462";

			AppDomain.CurrentDomain.AssemblyResolve += (s, args) =>
			{
				if (string.IsNullOrEmpty(args.Name))
				{
					return null;
				}

				AssemblyName assemblyName = new AssemblyName(args.Name);
				string path = Path.Combine(basePath, assemblyName.Name + ".dll");

				if (File.Exists(path))
				{
					//Console.WriteLine("Assembly " + assemblyName.Name + " redirected");
					return Assembly.LoadFrom(path);
				}

				//Console.WriteLine("Assembly " + assemblyName.Name + " not resolved");
				return null;
			};
#endif

            // Bump up the min threads reserved for this app to ramp connections faster - minWorkerThreads defaults to 4, minIOCP defaults to 4 
            ThreadPool.SetMinThreads(100, 100);
            // Change max connections from .NET to a remote service default: 2
            System.Net.ServicePointManager.DefaultConnectionLimit = 65000;
            // Turn off the Expect 100 to continue message - 'true' will cause the caller to wait until it round-trip confirms a connection to the server 
            System.Net.ServicePointManager.Expect100Continue = false;
            // Can decrease overall transmission overhead but can cause delay in data packet arrival
            System.Net.ServicePointManager.UseNagleAlgorithm = false;

        }

#if NET

        public class CmdletsLoadContext : AssemblyLoadContext
        {
            public CmdletsLoadContext(string basePath)
            {
                this.basePath = basePath;
            }

            private readonly string basePath;

            protected override Assembly Load(AssemblyName assemblyName)
            {
                // System.ServiceModel.* assemblies may or may not ship with PowerShell itself.
                // By returning null here we let the DEFAULT ALC supply them.
                //
                // When PS7 ships these assemblies (e.g. Windows), Cmdlets.dll (DEFAULT ALC)
                // resolves them directly from PS7's install directory WITHOUT going through
                // Default.Resolving.  If CmdletsLoadContext loaded its own copy here, the
                // CmdletsLoadContext copy and DEFAULT ALC's copy would be different objects,
                // causing FaultException<OrganizationServiceFault> catch clauses in Cmdlets.dll
                // to silently miss exceptions thrown by ServiceClient (type identity mismatch).
                //
                // When PS7 does NOT ship these assemblies (e.g. some Ubuntu PS versions),
                // Default.Resolving already loaded them from cmdlets/net8.0/ INTO CmdletsLoadContext
                // on behalf of Cmdlets.dll.  DEFAULT ALC caches that resolution, so returning null
                // here makes the runtime fall back to DEFAULT ALC, which returns the same
                // CmdletsLoadContext assembly instance.  Both code paths therefore share one copy.
                if (assemblyName.Name != null && assemblyName.Name.StartsWith("System.ServiceModel", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                // Microsoft.Crm.Sdk.Proxy is preloaded directly into DEFAULT ALC by OnImport() so that
                // PowerShell scripts can use New-Object / type literals with CRM message types.
                // Returning null here makes CmdletsLoadContext fall back to that DEFAULT ALC copy, ensuring
                // ServiceClient and PS scripts share the same type identity (one copy, no ALC duplication).
                if (assemblyName.Name != null && assemblyName.Name.Equals("Microsoft.Crm.Sdk.Proxy", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                // Microsoft.Extensions assemblies may already be loaded by the PowerShell host.
                // Reuse those assemblies to keep types such as ILogger consistent across contexts.
                if (assemblyName.Name != null && assemblyName.Name.StartsWith("Microsoft.Extensions.", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                string path = Path.Combine(basePath, assemblyName.Name + ".dll");

                if (File.Exists(path))
                {
                    //Console.WriteLine("Assembly " + assemblyName.Name + " redirected");
                    return LoadFromAssemblyPath(path);
                }
                else
                {
                    //Console.WriteLine("Assembly " + assemblyName.Name + " not resolved");
                }

                return null;
            }
        }
#endif

    }

}
