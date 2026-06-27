namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model
{
    /// <summary>
    /// Specifies the source control format for solution packing and unpacking.
    /// </summary>
    public enum SolutionSourceFormat
    {
        /// <summary>
        /// YAML source control format (default for PAC CLI 2.4.1+).
        /// Produces solution.yml and YAML-based component files.
        /// </summary>
        Yaml,

        /// <summary>
        /// Legacy XML source control format.
        /// Produces Other/Solution.xml and XML-based component files.
        /// </summary>
        Xml
    }

    /// <summary>
    /// Specifies the package type for solution packing and unpacking.
    /// </summary>
    public enum SolutionPackageType
    {
        /// <summary>
        /// Unmanaged solution package.
        /// </summary>
        Unmanaged,

        /// <summary>
        /// Managed solution package.
        /// </summary>
        Managed,

        /// <summary>
        /// Both managed and unmanaged solution packages.
        /// </summary>
        Both
    }

    /// <summary>
    /// Specifies the package type for solution import (packing).
    /// </summary>
    public enum ImportSolutionPackageType
    {
        /// <summary>
        /// Unmanaged solution package.
        /// </summary>
        Unmanaged,

        /// <summary>
        /// Managed solution package.
        /// </summary>
        Managed
    }
}
