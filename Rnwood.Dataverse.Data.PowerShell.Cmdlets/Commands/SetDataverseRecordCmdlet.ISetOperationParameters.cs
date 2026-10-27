using System;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Interface defining parameters for set operations that can be shared between cmdlet and operation context.
    /// </summary>
    internal interface ISetOperationParameters
    {
        /// <summary>
        /// Gets the business logic types to bypass.
        /// </summary>
        CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] BypassBusinessLogicExecution { get; }

        /// <summary>
        /// Gets the business logic execution step IDs to bypass.
        /// </summary>
        Guid[] BypassBusinessLogicExecutionStepIds { get; }

        /// <summary>
        /// Gets the number of retries for failed operations.
        /// </summary>
        int Retries { get; }

        /// <summary>
        /// Gets the initial retry delay in seconds.
        /// </summary>
        int InitialRetryDelay { get; }

        /// <summary>
        /// Gets a value indicating whether to skip updating existing records.
        /// </summary>
        bool NoUpdate { get; }

        /// <summary>
        /// Gets a value indicating whether to skip creating new records.
        /// </summary>
        bool NoCreate { get; }

        /// <summary>
        /// Gets a value indicating whether to use create-only mode.
        /// </summary>
        bool CreateOnly { get; }

        /// <summary>
        /// Gets a value indicating whether to use upsert mode.
        /// </summary>
        bool Upsert { get; }

        /// <summary>
        /// Gets a value indicating whether to use ExecuteMultiple instead of the bulk messages.
        /// </summary>
        bool UseExecuteMultiple { get; }

        /// <summary>
        /// Gets a value indicating whether to pass through the input object with Id set.
        /// </summary>
        bool PassThru { get; }

        /// <summary>
        /// Gets a value indicating whether to update all columns without comparison.
        /// </summary>
        bool UpdateAllColumns { get; }

        /// <summary>
        /// Gets the list of columns that should not be updated.
        /// </summary>
        string[] NoUpdateColumns { get; }

        /// <summary>
        /// Gets the match-on column lists for finding existing records.
        /// </summary>
        string[][] MatchOn { get; }

        /// <summary>
        /// Gets the ID of the record.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets a value indicating whether to allow processing multiple matching records.
        /// </summary>
        bool AllowMultipleMatches { get; }

        /// <summary>
        /// Gets a value indicating whether to enable duplicate detection (by setting SuppressDuplicateDetection to false in requests).
        /// </summary>
        bool EnableDuplicateDetection { get; }

        /// <summary>
        /// Gets the path to a directory containing file attachments to upload.
        /// When a file column value (GUID) changes, the cmdlet looks for a subfolder with the matching GUID 
        /// inside this directory and uploads the single file found there.
        /// </summary>
        string FileDirectory { get; }
    }
}
