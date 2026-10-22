

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Converts between Dataverse entities and PowerShell objects.
    /// </summary>
    public class DataverseEntityConverter
    {
        /// <summary>
        /// Initializes a new instance of the DataverseEntityConverter class.
        /// </summary>
        /// <param name="service">The organization service.</param>
        /// <param name="entityMetadataFactory">The entity metadata factory.</param>
        public DataverseEntityConverter(IOrganizationService service, EntityMetadataFactory entityMetadataFactory)
        {
            this.service = service;
            this.entityMetadataFactory = entityMetadataFactory;
        }

        private IOrganizationService service;
        private EntityMetadataFactory entityMetadataFactory;
        private Action<string> writeVerbose;

        /// <summary>
        /// Converts an Entity to a PSObject.
        /// </summary>
        public PSObject ConvertToPSObject(Entity entity, ColumnSet includedColumns, Func<AttributeMetadata, ValueType> getValueType)
        {
            return ConvertToPSObject(entity, includedColumns, getValueType, null);
        }

        /// <summary>
        /// Converts an Entity to a PSObject with verbose logging.
        /// </summary>
        public PSObject ConvertToPSObject(Entity entity, ColumnSet includedColumns, Func<AttributeMetadata, ValueType> getValueType, Action<string> writeVerbose)
        {
            // Store the verbose delegate for use in GetPSValue
            var previousVerbose = this.writeVerbose;
            this.writeVerbose = writeVerbose;

            try
            {
                PSObject result = new PSObject();

                result.Properties.Add(new PSNoteProperty("Id", entity.Id));
                result.Properties.Add(new PSNoteProperty("TableName", entity.LogicalName));

                EntityMetadata entityMetadata = entityMetadataFactory.GetLimitedMetadata(entity.LogicalName, writeVerbose);

                foreach (AttributeMetadata attributeMetadata in entityMetadata.Attributes.OrderBy(a => a.LogicalName, StringComparer.OrdinalIgnoreCase))
                {
                    if (includedColumns.AllColumns || includedColumns.Columns.Contains(attributeMetadata.LogicalName, StringComparer.OrdinalIgnoreCase))
                    {
                        result.Properties.Add(new PSNoteProperty(attributeMetadata.LogicalName, GetPSValue(entity, entityMetadata, attributeMetadata, getValueType)));
                    }
                }

                foreach (KeyValuePair<string, object> attr in entity.Attributes.Where(aa => aa.Value is AliasedValue && !entityMetadata.Attributes.Any(a => a.LogicalName.Equals(aa.Key, StringComparison.OrdinalIgnoreCase))))
                {
                    AliasedValue aliasedValue = (AliasedValue)attr.Value;

                    result.Properties.Add(new PSNoteProperty(attr.Key, aliasedValue.Value));
                }

                foreach (KeyValuePair<string, object> attr in entity.Attributes.Where(aa => aa.Value is EntityCollection && !entityMetadata.Attributes.Any(a => a.LogicalName.Equals(aa.Key, StringComparison.OrdinalIgnoreCase))))
                {
                    if (includedColumns.AllColumns || includedColumns.Columns.Contains(attr.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        EntityCollection entities = (EntityCollection)attr.Value;
                        List<PSObject> psObjects = new List<PSObject>(entities.Entities.Count);

                        if (entities.Entities.Any())
                        {
                            EntityMetadata referencedEntityMetadata = entityMetadataFactory.GetLimitedMetadata(entities.Entities.First().LogicalName, writeVerbose);

                            foreach (Entity referenceEntity in entities.Entities)
                            {
                                psObjects.Add(ConvertToPSObject(referenceEntity, new ColumnSet(GetAllColumnNames(referencedEntityMetadata, false, null, false)), getValueType, writeVerbose));
                            }
                        }

                        result.Properties.Add(new PSNoteProperty(attr.Key, psObjects.ToArray()));
                    }
                }

                return result;
            }
            finally
            {
                // Restore previous verbose delegate
                this.writeVerbose = previousVerbose;
            }
        }

        /// <summary>
        /// Converts a PSObject to an Entity.
        /// </summary>
        public Entity ConvertToDataverseEntity(PSObject psObject, string entityName, ConvertToDataverseEntityOptions options)
        {
            if (psObject.ImmediateBaseObject is Entity e)
            {
                return e;
            }

            if (psObject.ImmediateBaseObject is Hashtable ht)
            {
                psObject = new PSObject();
                foreach (var kvp in ht.Cast<DictionaryEntry>())
                {
                    psObject.Properties.Add(new PSNoteProperty((string)kvp.Key, kvp.Value));
                }
            }

            EntityMetadata entityMetadata = entityMetadataFactory.GetLimitedMetadata(entityName);

            Entity result = new Entity(entityName);

            foreach (PSPropertyInfo property in psObject.Properties)
            {
                if (property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    Guid id;
                    if (Guid.TryParse(Convert.ToString(property.Value), out id))
                    {
                        result.Id = id;
                    }

                    continue;
                }

                if (options.IgnoredPropertyName.Contains(property.Name) || property.Name.Equals(
                 "EntityName", StringComparison.OrdinalIgnoreCase) || property.Name.Equals(
                 "TableName", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ConvertToDataverseEntityColumnOptions columnOptions;
                if (!options.ColumnOptions.TryGetValue(property.Name, out columnOptions))
                {
                    columnOptions = new ConvertToDataverseEntityColumnOptions();
                }

                AttributeMetadata attributeMetadata = entityMetadataFactory.GetAttribute(entityName, property.Name);

                if (attributeMetadata == null)
                {
                    if (property.Name == "calendarrules") //Magic attribute not in metadata
                    {
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("Table {0} does not contain a column with the name {1}", entityName, property.Name));
                    }
                }

                try
                {
                    object convertedValue = ConvertToDataverseValue(entityMetadata, property.Name, attributeMetadata, property.Value, columnOptions, options.DisallowLookupQueries);

                    if (attributeMetadata != null)
                    {
                        result[attributeMetadata.LogicalName] = convertedValue;

                        if (attributeMetadata.LogicalName == entityMetadata.PrimaryIdAttribute && convertedValue != null)
                        {
                            result.Id = (Guid)convertedValue;
                        }
                    }
                    else
                    {
                        result[property.Name.ToLower()] = convertedValue;
                    }
                } catch (FormatException conversionEx)
                {
                    throw new FormatException($"Error converting column value '{attributeMetadata.LogicalName}' - {conversionEx.Message}", conversionEx);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all column names for an entity.
        /// </summary>
        public static string[] GetAllColumnNames(EntityMetadata entityMetadata, bool includeSystemColumns, string[] excludeColumns, bool includeFileAndImageMetadataColumns)
        {
            string[] magicColumns = new string[0];

            if (entityMetadata.LogicalName.Equals("calendar", StringComparison.OrdinalIgnoreCase))
            {
                magicColumns = new string[] { "calendarrules" };
            }

            return entityMetadata.Attributes
                                 .Where(a => a.IsValidForRead.GetValueOrDefault()
                                 && (a.AttributeOf == null
                                    || a.AttributeTypeName == AttributeTypeDisplayName.ImageType //Include the image columns
                                    || (includeFileAndImageMetadataColumns
                                    && entityMetadata.Attributes.Any(ao => (ao.LogicalName == a.AttributeOf || $"{ao.LogicalName}id" == a.AttributeOf)
                                        && (ao.AttributeTypeName == AttributeTypeDisplayName.FileType || ao.AttributeTypeName == AttributeTypeDisplayName.ImageType)))
                                 )
                                 && (a.AttributeType != AttributeTypeCode.Virtual
                                    || a.AttributeTypeName == AttributeTypeDisplayName.FileType //Include the file id columns
                                    || a.AttributeTypeName == AttributeTypeDisplayName.ImageType
                                    )
                                 && (a.AttributeType != AttributeTypeCode.Uniqueidentifier || !a.LogicalName.EndsWith("id") || includeFileAndImageMetadataColumns || !entityMetadata.Attributes.Any(ao => ao.AttributeOf == a.LogicalName && ao.AttributeTypeName == AttributeTypeDisplayName.ImageType))
                                 )

                                 .Select(a => a.LogicalName)
                                 .Concat(magicColumns)
                                 .Except(!includeSystemColumns
                                             ? new[]
                                                 { "organizationid",
                                                     "createdby", "createdon", "createdonbehalfby", "modifiedby",
                                                     "modifiedon", "modifiedonbehalfby", "ownerid",
                                                     "transactioncurrencyid",
                                                     "importsequencenumber", "overriddencreatedon",
                                                     "owningbusinessunit", "owningteam", "owninguser", "timezoneruleversionnumber",
                                                     "utcconversiontimezonecode", "versionnumber"
                                                 }
                                             : new string[0], StringComparer.OrdinalIgnoreCase)
                                  .Except(excludeColumns ?? new string[0], StringComparer.OrdinalIgnoreCase)
                                 .ToArray();
        }

        /// <summary>
        /// Gets the PowerShell value for an entity attribute.
        /// </summary>
        public object GetPSValue(Entity entity, EntityMetadata entityMetadata, AttributeMetadata attributeMetadata, Func<AttributeMetadata, ValueType> getValueType)
        {
            ValueType valueType = getValueType(attributeMetadata);
            bool useRawValues = valueType == ValueType.Raw;

            switch (attributeMetadata.AttributeType.Value)
            {
                case AttributeTypeCode.Owner:
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Customer:
                    EntityReference entityReferenceValue =
                        entity.GetAttributeValue<EntityReference>(attributeMetadata.LogicalName);

                    if (entityReferenceValue != null)
                    {
                        //The LogicalName returned in entityreferences on activitymimeattachment entity are invalid for some reason, so don't try to resolve.

                        DataverseEntityReference nameAndId = new DataverseEntityReference(entityReferenceValue);

                        if (useRawValues || attributeMetadata.LogicalName == "objectid" || attributeMetadata.LogicalName == "attachmentid")
                        {
                            return nameAndId;
                        }

                        if (entityReferenceValue.Name != null)
                        {
                            return entityReferenceValue.Name;
                        }

                        //For some odd reason the EntityReference Name that is returned is sometimes null even though the record it's pointing at has a name
                        string nameAttribute = entityMetadataFactory.GetLimitedMetadata(entityReferenceValue.LogicalName, writeVerbose).PrimaryNameAttribute;

                        //Some internal entities do not have a primary name attribute
                        if (nameAttribute == null)
                        {
                            return nameAndId;
                        }

                        if (entityReferenceValue.Id == Guid.Empty)
                        {
                            return null;
                        }

                        writeVerbose?.Invoke($"Retrieving name for lookup {attributeMetadata.LogicalName}: {entityReferenceValue.LogicalName}({entityReferenceValue.Id})");
                        string name = service.Retrieve(entityReferenceValue.LogicalName, entityReferenceValue.Id,
                                         new ColumnSet(nameAttribute)).GetAttributeValue<string>(nameAttribute);

                        if (string.IsNullOrEmpty(name))
                        {
                            return nameAndId;
                        }

                        return name;
                    }

                    return null;

                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.Status:
                case AttributeTypeCode.State:

                    OptionSetValue optionSetValue =
                        entity.GetAttributeValue<OptionSetValue>(attributeMetadata.LogicalName);

                    if (optionSetValue != null)
                    {
                        if (useRawValues)
                        {
                            return optionSetValue.Value;
                        }

                        EnumAttributeMetadata pickListMetadata = (EnumAttributeMetadata)attributeMetadata;

                        OptionMetadata option = pickListMetadata.OptionSet.Options.FirstOrDefault(o => o.Value == optionSetValue.Value);
                        return option != null ? option.Label.UserLocalizedLabel.Label : (object)optionSetValue.Value;
                    }

                    return null;

                case AttributeTypeCode.DateTime:
                    DateTime? dateValue = entity.GetAttributeValue<DateTime?>(attributeMetadata.LogicalName);

                    //Deal with an issue in Dataverse web service or sdk (not sure which)
                    //Some scheduling entities store their date/times in the DB as local times (as opposed to UTC which is used in all other cases)
                    //These dates are incorrectly being returned with a DateTimeKind of UTC when they are actually local times rel
                    if (entityMetadata.LogicalName != "usersettings" && entityMetadata.Attributes.Any(a => a.LogicalName.Equals("timezonecode", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (!entity.Contains("timezonecode"))
                        {
                            throw new InvalidOperationException("This is an table with special non-UTC datetimes. Must include timezonecode in entity to allow conversion");
                        }

                        int? tzCode = entity.GetAttributeValue<int?>("timezonecode");
                        if (tzCode.HasValue && dateValue.HasValue)
                        {
                            if (tzCode.Value != 85 && tzCode.Value != -1)
                            {
                                throw new NotImplementedException("Unhandled timezonecode " + tzCode);
                            }

                            return DateTime.SpecifyKind(dateValue.Value, DateTimeKind.Local);
                        }
                    }

                    return dateValue.HasValue ? dateValue.Value.ToLocalTime() : (DateTime?)null;

                case AttributeTypeCode.Money:
                    Money moneyValue = entity.GetAttributeValue<Money>(attributeMetadata.LogicalName);
                    return moneyValue != null ? moneyValue.Value : (decimal?)null;

                case AttributeTypeCode.Uniqueidentifier:
                    Guid? guidValue = entity.GetAttributeValue<Guid?>(attributeMetadata.LogicalName);

                    if (guidValue.HasValue)
                    {
                        if (entityMetadata.IsIntersect.GetValueOrDefault() && !useRawValues)
                        {
                            ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = entityMetadata.ManyToManyRelationships[0];

                            if (manyToManyRelationshipMetadata.Entity1IntersectAttribute.Equals(attributeMetadata.LogicalName, StringComparison.OrdinalIgnoreCase))
                            {
                                string nameAttribute = entityMetadataFactory.GetLimitedMetadata(manyToManyRelationshipMetadata.Entity1LogicalName, writeVerbose).PrimaryNameAttribute;
                                writeVerbose?.Invoke($"Retrieving name for many-to-many relationship {attributeMetadata.LogicalName}: {manyToManyRelationshipMetadata.Entity1LogicalName}({guidValue.Value})");
                                return service.Retrieve(manyToManyRelationshipMetadata.Entity1LogicalName, guidValue.Value, new ColumnSet(nameAttribute)).GetAttributeValue<string>(nameAttribute);
                            }
                            else if (manyToManyRelationshipMetadata.Entity2IntersectAttribute.Equals(attributeMetadata.LogicalName, StringComparison.OrdinalIgnoreCase))
                            {
                                string nameAttribute = entityMetadataFactory.GetLimitedMetadata(manyToManyRelationshipMetadata.Entity2LogicalName, writeVerbose).PrimaryNameAttribute;
                                writeVerbose?.Invoke($"Retrieving name for many-to-many relationship {attributeMetadata.LogicalName}: {manyToManyRelationshipMetadata.Entity2LogicalName}({guidValue.Value})");
                                return service.Retrieve(manyToManyRelationshipMetadata.Entity2LogicalName, guidValue.Value, new ColumnSet(nameAttribute)).GetAttributeValue<string>(nameAttribute);
                            }
                        }
                        else if (entityMetadata.Attributes.Any(a => a.AttributeTypeName == AttributeTypeDisplayName.ImageType && a.AttributeOf == attributeMetadata.LogicalName))
                        {
                            return new DataverseFileReference(guidValue.Value);
                        }

                    }
                    return guidValue;

                case AttributeTypeCode.PartyList:

                    EntityCollection entities = entity.GetAttributeValue<EntityCollection>(attributeMetadata.LogicalName);
                    List<PSObject> psObjects = new List<PSObject>(entities.Entities.Count);

                    if (entities.Entities.Any())
                    {
                        EntityMetadata referencedEntityMetadata = entityMetadataFactory.GetLimitedMetadata(entities.Entities.First().LogicalName);

                        foreach (Entity referenceEntity in entities.Entities)
                        {
                            psObjects.Add(ConvertToPSObject(referenceEntity, new ColumnSet(GetAllColumnNames(referencedEntityMetadata, false, new[] { "exchangeentryid" }, false)), getValueType));
                        }
                    }

                    return psObjects.ToArray();
                default:
                    if (attributeMetadata.AttributeTypeName == AttributeTypeDisplayName.FileType)
                    {
                        Guid? fileId = entity.GetAttributeValue<Guid?>(attributeMetadata.LogicalName);
                        if (fileId.HasValue)
                        {
                            return new DataverseFileReference(fileId.Value);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (attributeMetadata is MultiSelectPicklistAttributeMetadata
                        multiPicklistAttributeMetadata)
                    {
                        OptionSetValueCollection optionSetValues = entity.GetAttributeValue<OptionSetValueCollection>(attributeMetadata.LogicalName);

                        if (optionSetValues != null)
                        {
                            if (useRawValues)
                            {
                                return optionSetValues.Select(v => v.Value).ToArray();
                            }

                            return optionSetValues.Select(v =>
                            {
                                OptionMetadata option = multiPicklistAttributeMetadata.OptionSet.Options.FirstOrDefault(o => o.Value == v.Value);
                                return option != null ? option.Label.UserLocalizedLabel.Label : (object)v.Value;
                            }).ToArray();
                        }
                    }

                    // Check for special managed property types
                    object rawValue = entity.GetAttributeValue<object>(attributeMetadata.LogicalName);
                    if (rawValue is Microsoft.Xrm.Sdk.BooleanManagedProperty booleanManagedProperty)
                    {
                        return booleanManagedProperty.Value;
                    }

                    return rawValue;
            }
        }

        /// <summary>
        /// Converts a PowerShell value to a Dataverse value.
        /// </summary>
        public object ConvertToDataverseValue(EntityMetadata entityMetadata, string propertyName, AttributeMetadata attributeMetadata, object psValue, ConvertToDataverseEntityColumnOptions columnOptions, bool disallowLookupQueries = false)
        {
            if (propertyName.Equals("calendarrules", StringComparison.OrdinalIgnoreCase)) //Magic attribute no metadata
            {
                if (psValue is IEnumerable)
                {
                    ConvertToDataverseEntityOptions options = new ConvertToDataverseEntityOptions
                    {
                        DisallowLookupQueries = disallowLookupQueries
                    };

                    Entity[] entities = ((IEnumerable)psValue).Cast<object>().Select(PSObject.AsPSObject)
                        .Select(o => ConvertToDataverseEntity(o, "calendarrule", options)).ToArray();

                    return new EntityCollection(entities);
                }
                else
                {
                    throw new Exception("calendarrule property must contain collection of objects");
                }
            }

            if (psValue is PSObject && !(((PSObject)psValue).BaseObject is PSCustomObject))
            {
                psValue = ((PSObject)psValue).BaseObject;
            }

            object convertedValue;
            string stringValue = Convert.ToString(psValue);

            switch (attributeMetadata.AttributeType.Value)
            {
                case AttributeTypeCode.PartyList:
                    if (psValue is IEnumerable)
                    {
                        ConvertToDataverseEntityOptions options = new ConvertToDataverseEntityOptions
                        {
                            DisallowLookupQueries = disallowLookupQueries
                        };

                        Entity[] entities = ((IEnumerable)psValue).Cast<object>().Select(PSObject.AsPSObject)
                            .Select(o => ConvertToDataverseEntity(o, "activityparty", options)).ToArray();

                        return new EntityCollection(entities);
                    }
                    else
                    {
                        throw new Exception("partylist property must contain collection of objects");
                    }

                case AttributeTypeCode.Uniqueidentifier:
                    {
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            if (entityMetadata.IsIntersect.GetValueOrDefault())
                            {
                                Guid guidValue;
                                if (Guid.TryParse(stringValue, out guidValue))
                                {
                                    return guidValue;
                                }

                                ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = entityMetadata.ManyToManyRelationships[0];

                                string lookupEntity = null;
                                if (manyToManyRelationshipMetadata.Entity1IntersectAttribute.Equals(attributeMetadata.LogicalName, StringComparison.OrdinalIgnoreCase))
                                {
                                    lookupEntity = manyToManyRelationshipMetadata.Entity1LogicalName;
                                }
                                else if (manyToManyRelationshipMetadata.Entity2IntersectAttribute.Equals(attributeMetadata.LogicalName, StringComparison.OrdinalIgnoreCase))
                                {
                                    lookupEntity = manyToManyRelationshipMetadata.Entity2LogicalName;
                                }

                                if (lookupEntity != null)
                                {
                                    if (disallowLookupQueries)
                                    {
                                        throw CreateLookupResolutionDisabledException(entityMetadata, attributeMetadata);
                                    }

                                    var targetEntityMetadata = entityMetadataFactory.GetLimitedMetadata(lookupEntity);
                                    string lookupNameAttribute = targetEntityMetadata.PrimaryNameAttribute;


                                    QueryByAttribute lookupQuery = new QueryByAttribute(lookupEntity);
                                    lookupQuery.TopCount = 2;
                                    lookupQuery.ColumnSet = new ColumnSet();

                                    if (columnOptions.LookupColumn != null)
                                    {
                                        lookupQuery.AddAttributeValue(columnOptions.LookupColumn, stringValue);
                                    }
                                    else
                                    {
                                        lookupQuery.AddAttributeValue(lookupNameAttribute, stringValue);
                                    }

                                    var lookupRecords = QueryHelpers.ExecuteQueryWithPaging(lookupQuery, service, (msg) => { }).ToList();

                                    if (lookupRecords.Count == 1)
                                    {
                                        return lookupRecords[0].Id;
                                    }
                                    else if (lookupRecords.Count > 1)
                                    {
                                        throw new Exception(string.Format("Could not find a single {0} record matching {2} '{1}'", lookupEntity, stringValue, columnOptions.LookupColumn ?? "name"));
                                    }
                                }

                                throw new Exception(string.Format("Could not find a single {0} record uniquely matching {2} '{1}'", lookupEntity, stringValue, columnOptions.LookupColumn ?? "name"));

                            }

                            convertedValue = Guid.Parse(stringValue);
                        }

                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }


                case AttributeTypeCode.BigInt:
                    {
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            convertedValue = Convert.ToInt64(psValue);
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }

                case AttributeTypeCode.Boolean:
                    {
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            convertedValue = Convert.ToBoolean(psValue);
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }

                case AttributeTypeCode.DateTime:
                    {
                        if (psValue is DateTime)
                        {
                            DateTime dateValue = (DateTime)psValue;
                            if (dateValue.Kind == DateTimeKind.Unspecified)
                            {
                                dateValue = DateTime.SpecifyKind(dateValue, DateTimeKind.Local);
                            }

                            convertedValue = dateValue.ToUniversalTime();
                        }
                        else if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            convertedValue = DateTime.Parse(stringValue, null, System.Globalization.DateTimeStyles.AssumeLocal).ToUniversalTime();
                        }
                        else
                        {
                            convertedValue = null;
                        }

                        //Deal with an issue in Dataverse web service or sdk (not sure which)
                        //Some scheduling entities store their date/times in the DB as local times (as opposed to UTC which is used in all other cases)
                        //These dates are incorrectly being returned with a DateTimeKind of UTC when they are actually local times
                        if (convertedValue is DateTime && entityMetadata.Attributes.Any(a => a.LogicalName.Equals("timezonecode", StringComparison.OrdinalIgnoreCase)))
                        {
                            convertedValue = DateTime.SpecifyKind(((DateTime)convertedValue).ToLocalTime(), DateTimeKind.Utc);
                        }

                        break;
                    }

                case AttributeTypeCode.Money:
                    {
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            convertedValue = new Money(Convert.ToDecimal(psValue));
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }

                case AttributeTypeCode.Decimal:
                    {
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            convertedValue = Convert.ToDecimal(psValue);
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }

                case AttributeTypeCode.Double:
                    {
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            convertedValue = Convert.ToDouble(psValue);
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }

                case AttributeTypeCode.Integer:
                    {
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            convertedValue = Convert.ToInt32(psValue);
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }

                case AttributeTypeCode.Memo:
                    {
                        convertedValue = stringValue;
                        break;
                    }

                case AttributeTypeCode.EntityName:
                case AttributeTypeCode.String:
                    {
                        if (psValue != null)
                        {
                            convertedValue = Convert.ToString(psValue);
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }

                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.Status:
                case AttributeTypeCode.State:
                    {
                        convertedValue = ConvertPicklistToDataverseValue(propertyName, attributeMetadata, stringValue);
                        break;
                    }
                case AttributeTypeCode.Owner:
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Customer:
                    {
                        if (psValue != null && psValue is DataverseEntityReference)
                        {
                            convertedValue = ((DataverseEntityReference)psValue).ToEntityReference();
                        }
                        else if (psValue is EntityReference entityReference)
                        {
                            convertedValue = entityReference;
                        }
                        else if (psValue != null && psValue is PSObject)
                        {
                            PSObject customObj = (PSObject)psValue;

                            var nameProp = customObj.Properties.FirstOrDefault(p => p.Name.Equals("TableName", StringComparison.OrdinalIgnoreCase));
                            if (nameProp == null)
                            {
                                nameProp = customObj.Properties.FirstOrDefault(p => p.Name.Equals("EntityName", StringComparison.OrdinalIgnoreCase));
                            }

                            if (nameProp == null)
                            {
                                nameProp = customObj.Properties.FirstOrDefault(p => p.Name.Equals("LogicalName", StringComparison.OrdinalIgnoreCase));
                            }

                            if (nameProp == null)
                            {
                                throw new FormatException("Could not convert value to entity reference. TableName(/EntityName/LogicalName) property is missing");
                            }

                            var keyAttributesProp = customObj.Properties.FirstOrDefault(p => p.Name.Equals("KeyAttributes", StringComparison.OrdinalIgnoreCase));
                            if (keyAttributesProp?.Value != null)
                            {
                                convertedValue = new EntityReference((string)nameProp.Value, ConvertToKeyAttributeCollection(keyAttributesProp.Value));
                            }
                            else
                            {
                                var idProp = customObj.Properties.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
                                if (idProp?.Value == null || !Guid.TryParse(idProp.Value.ToString(), out Guid referenceId))
                                {
                                    throw new FormatException("Could not convert value to entity reference. Id property is missing or is not a valid GUID");
                                }

                                convertedValue = new EntityReference((string)nameProp.Value, referenceId);
                            }
                        }
                        else if (psValue is IDictionary lookupObject)
                        {
                            string tableName = GetDictionaryValue(lookupObject, "TableName")?.ToString()
                                ?? GetDictionaryValue(lookupObject, "EntityName")?.ToString()
                                ?? GetDictionaryValue(lookupObject, "LogicalName")?.ToString();

                            if (string.IsNullOrWhiteSpace(tableName))
                            {
                                throw new FormatException("Could not convert value to entity reference. TableName(/EntityName/LogicalName) property is missing");
                            }

                            object keyAttributes = GetDictionaryValue(lookupObject, "KeyAttributes");
                            if (keyAttributes != null)
                            {
                                convertedValue = new EntityReference(tableName, ConvertToKeyAttributeCollection(keyAttributes));
                            }
                            else if (Guid.TryParse(Convert.ToString(GetDictionaryValue(lookupObject, "Id")), out Guid referenceId))
                            {
                                convertedValue = new EntityReference(tableName, referenceId);
                            }
                            else
                            {
                                throw new FormatException("Could not convert value to entity reference. Either a valid Id or KeyAttributes property is required");
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(stringValue) &&
                            (
                                stringValue.Contains("TableName=")
                                || stringValue.Contains("EntityName=")
                            )
                            && stringValue.Contains("Id="))
                        {
                            string[] keyValuePairs = stringValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            string entityName = null;
                            Guid? id = null;

                            foreach (string keyValuePair in keyValuePairs)
                            {
                                string[] keyValuePairBits = keyValuePair.Split(new[] { '=' }, 2).Select(v => v.Trim()).ToArray();
                                if (keyValuePairBits.Length == 2)
                                {
                                    string key = keyValuePairBits[0];
                                    string value = keyValuePairBits[1];

                                    if (key.Equals("Id", StringComparison.OrdinalIgnoreCase))
                                    {
                                        Guid idValue;
                                        if (Guid.TryParse(value, out idValue))
                                        {
                                            id = idValue;
                                        }
                                    }
                                    else if (key.Equals("TableName", StringComparison.OrdinalIgnoreCase)
                                        || key.Equals("EntityName", StringComparison.OrdinalIgnoreCase)
                                        || key.Equals("LogicalName", StringComparison.OrdinalIgnoreCase))
                                    {
                                        entityName = value;
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(entityName) || !id.HasValue)
                            {
                                throw new FormatException(string.Format("Could not convert value '{0}' to entity reference. Either TableName(/EntityName/LogicalName) or Id value is missing or invalid", stringValue));
                            }

                            convertedValue = new EntityReference(entityName, id.Value);
                        }
                        else if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            LookupAttributeMetadata lookupAttributeMetadata = (LookupAttributeMetadata)attributeMetadata;
                            convertedValue = null;

                            if (lookupAttributeMetadata.Targets == null || lookupAttributeMetadata.Targets.Length == 0)
                            {
                                //Dataverse bug!
                                //Targets missing so go find them from relationship data, which is not affected by this issue.
                                lookupAttributeMetadata.Targets = entityMetadata.ManyToOneRelationships.Where(r => r.ReferencingAttribute.Equals(attributeMetadata.LogicalName, StringComparison.OrdinalIgnoreCase)).Select(r => r.ReferencedEntity).ToArray();
                            }

                            if (lookupAttributeMetadata.Targets.Length == 0)
                            {
                                throw new FormatException("Could not determine the target tables for lookup column " + attributeMetadata.LogicalName + " - Dataverse metadata returned 0 targets!");
                            }

                            //Optimise the most common case. Single target lookup column and incoming guid as the value to convert
                            //Don't query to check record exists - Dataverse will do that.
                            if (lookupAttributeMetadata.Targets.Length == 1)
                            {
                                Guid guidValue;
                                if (Guid.TryParse(stringValue, out guidValue))
                                {
                                    convertedValue = new EntityReference(lookupAttributeMetadata.Targets[0], guidValue);
                                    break;
                                }
                            }

                            if (disallowLookupQueries)
                            {
                                throw CreateLookupResolutionDisabledException(entityMetadata, attributeMetadata);
                            }

                            bool applyStateFilter = true;

                            for (int i = 1; i <= 2; i++)
                            {
                                foreach (string targetEntityName in lookupAttributeMetadata.Targets)
                                {
                                    EntityMetadata targetEntityMetadata;

                                    try
                                    {
                                        targetEntityMetadata = entityMetadataFactory.GetLimitedMetadata(targetEntityName);
                                    }
                                    catch (Exception e)
                                    {
                                        //Dataverse returns entities which don't exists in Targets!
                                        if (e.Message == "Could not find table")
                                        {
                                            continue;
                                        }

                                        throw;
                                    }

                                    QueryByAttribute targetEntityQuery = new QueryByAttribute(targetEntityName);
                                    targetEntityQuery.ColumnSet = new ColumnSet();
                                    targetEntityQuery.TopCount = 2;

                                    if (targetEntityMetadata.Attributes.Any(a => string.Equals(a.LogicalName, "createdon", StringComparison.OrdinalIgnoreCase)))
                                    {
                                        targetEntityQuery.AddOrder("createdon", OrderType.Descending);
                                    }

                                    Guid guidValue;
                                    if (Guid.TryParse(stringValue, out guidValue))
                                    {
                                        targetEntityQuery.AddAttributeValue(targetEntityMetadata.PrimaryIdAttribute, guidValue);
                                    }
                                    else
                                    {
                                        if (columnOptions.LookupColumn != null)
                                        {
                                            targetEntityQuery.AddAttributeValue(columnOptions.LookupColumn, stringValue);
                                        }
                                        else
                                        {
                                            targetEntityQuery.AddAttributeValue(targetEntityMetadata.PrimaryNameAttribute, stringValue);
                                        }

                                        if (applyStateFilter)
                                        {
                                            if (targetEntityMetadata.Attributes.Any(a => string.Equals(a.LogicalName, "statecode", StringComparison.OrdinalIgnoreCase)))
                                            {
                                                targetEntityQuery.AddAttributeValue("statecode", 0);
                                            }

                                            if (targetEntityMetadata.Attributes.Any(a => string.Equals(a.LogicalName, "isdisabled", StringComparison.OrdinalIgnoreCase)))
                                            {
                                                targetEntityQuery.AddAttributeValue("isdisabled", false);
                                            }
                                        }
                                    }

                                    var targetRecords = QueryHelpers.ExecuteQueryWithPaging(targetEntityQuery, service, (msg) => { }).ToList();
                                    if (targetRecords.Count > 0)
                                    {
                                        convertedValue = targetRecords[0].ToEntityReference();
                                        break;
                                    }
                                }

                                if (convertedValue != null)
                                {
                                    break;
                                }

                                applyStateFilter = false;
                            }

                            if (convertedValue == null)
                            {
                                throw new FormatException(string.Format("Could not find any {0} record with ID matching or any single active record with {2} matching '{1}'", string.Join("/", lookupAttributeMetadata.Targets), stringValue, columnOptions.LookupColumn ?? "name"));
                            }
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }

                default:
                    if (attributeMetadata.AttributeTypeName == AttributeTypeDisplayName.FileType) {
                        if (string.IsNullOrEmpty(stringValue))
                        {
                            return null;
                        } 

                        if (psValue is DataverseFileReference fileReference)
                        {
                            return fileReference.Id;
                        }else if (psValue is PSObject psObject)
                        {
                            var idProp = psObject.Properties.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));

                            if (idProp == null)
                            {
                                throw new FormatException("Could not convert value to file reference. Id property is missing");
                            }

                            return Guid.Parse(idProp.Value.ToString());
                        }

                        return Guid.Parse(stringValue);
                    }
                    else if (attributeMetadata.AttributeTypeName == AttributeTypeDisplayName.ImageType)
                    {
                        if (string.IsNullOrEmpty(stringValue))
                        {
                            return null;
                        }

                        byte[] bytes = Convert.FromBase64String(stringValue);
                        return bytes;
                    }
                    else if (attributeMetadata is MultiSelectPicklistAttributeMetadata
         multiPicklistAttributeMetadata)
                    {
                        if (psValue == null)
                        {
                            convertedValue = null;
                            break;
                        }

                        if (psValue is string || psValue is int || !(psValue is IEnumerable enumerable))
                        {
                            enumerable = new[] { psValue };
                        }

                        OptionSetValueCollection optionSetValues = new OptionSetValueCollection();
                        foreach (var itemPsValue in enumerable)
                        {
                            string itemStringValue = Convert.ToString(itemPsValue);

                            optionSetValues.Add(ConvertPicklistToDataverseValue(propertyName, attributeMetadata, itemStringValue));
                        }

                        convertedValue = optionSetValues;

                    }
                    else
                    {
                        throw new NotImplementedException($"Column {attributeMetadata.LogicalName} - conversion to column type {attributeMetadata.AttributeType.Value} is not implemented");
                    }
                    break;
            }

            return convertedValue;
        }

        private static OptionSetValue ConvertPicklistToDataverseValue(string propertyName, AttributeMetadata attributeMetadata, string stringValue)
        {
            OptionSetValue convertedValue;
            if (!string.IsNullOrWhiteSpace(stringValue))
            {
                EnumAttributeMetadata pickListAttributeMetadata = (EnumAttributeMetadata)attributeMetadata;
                convertedValue = pickListAttributeMetadata.OptionSet.Options
                    .Where(o => string.Equals(o.Label.UserLocalizedLabel.Label, stringValue, StringComparison.OrdinalIgnoreCase))
                    .Select(o => new OptionSetValue(o.Value.Value))
                    .FirstOrDefault();

                if (convertedValue == null)
                {
                    int intValue;
                    if (int.TryParse(stringValue, out intValue))
                    {
                        convertedValue = new OptionSetValue(intValue);
                    }
                }

                if (convertedValue == null)
                {
                    throw new FormatException(string.Format("Could not find options set value for matching label or value for string '{0}' for attribute {1}", stringValue, propertyName));
                }
            }
            else
            {
                convertedValue = null;
            }

            return convertedValue;
        }

        private static FormatException CreateLookupResolutionDisabledException(EntityMetadata entityMetadata, AttributeMetadata attributeMetadata)
        {
            return new FormatException(
                $"Resolving the value for column '{attributeMetadata.LogicalName}' on table '{entityMetadata.LogicalName}' requires a lookup query, but lookup queries are disabled. Supply a reference object with TableName and Id, or TableName and KeyAttributes, instead of a name.");
        }

        private static object GetDictionaryValue(IDictionary dictionary, string key)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                if (string.Equals(Convert.ToString(entry.Key), key, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Value;
                }
            }

            return null;
        }

        private static KeyAttributeCollection ConvertToKeyAttributeCollection(object keyAttributesValue)
        {
            if (keyAttributesValue is KeyAttributeCollection existingKeyAttributes)
            {
                return existingKeyAttributes;
            }

            var keyAttributes = new KeyAttributeCollection();
            if (keyAttributesValue is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    keyAttributes.Add(Convert.ToString(entry.Key), entry.Value);
                }
            }
            else if (keyAttributesValue is PSObject psObject)
            {
                foreach (PSPropertyInfo property in psObject.Properties)
                {
                    keyAttributes.Add(property.Name, property.Value);
                }
            }
            else
            {
                throw new FormatException("KeyAttributes must be a hashtable or object whose properties are the alternate key columns and values");
            }

            if (keyAttributes.Count == 0)
            {
                throw new FormatException("KeyAttributes must contain at least one alternate key column and value");
            }

            return keyAttributes;
        }

        /// <summary>
        /// Converts a PSObject to an EntityReference.
        /// </summary>
        /// <param name="psObject">The PSObject to convert. Must have Id property and one of: TableName, EntityName, or LogicalName properties.</param>
        /// <returns>The converted EntityReference.</returns>
        /// <exception cref="ArgumentNullException">Thrown when psObject is null.</exception>
        /// <exception cref="FormatException">Thrown when required properties are missing or invalid.</exception>
        public static EntityReference ConvertPSObjectToEntityReference(PSObject psObject)
        {
            if (psObject == null)
                throw new ArgumentNullException(nameof(psObject));

            // Find the table/entity name property (case insensitive)
            var nameProperty = psObject.Properties.FirstOrDefault(p => p.Name.Equals("TableName", StringComparison.OrdinalIgnoreCase));
            if (nameProperty == null)
            {
                nameProperty = psObject.Properties.FirstOrDefault(p => p.Name.Equals("EntityName", StringComparison.OrdinalIgnoreCase));
            }
            if (nameProperty == null)
            {
                nameProperty = psObject.Properties.FirstOrDefault(p => p.Name.Equals("LogicalName", StringComparison.OrdinalIgnoreCase));
            }

            if (nameProperty == null)
            {
                throw new FormatException("Could not convert value to entity reference. TableName(/EntityName/LogicalName) property is missing");
            }

            // Find the Id property
            var idProperty = psObject.Properties.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
            if (idProperty == null)
            {
                throw new FormatException("Could not convert value to entity reference. Id property is missing");
            }

            // Convert the values
            string tableName = nameProperty.Value?.ToString();
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new FormatException("Could not convert value to entity reference. TableName(/EntityName/LogicalName) property is null or empty");
            }

            if (!Guid.TryParse(idProperty.Value?.ToString(), out Guid id))
            {
                throw new FormatException("Could not convert value to entity reference. Id property is not a valid GUID");
            }

            // Check for Name property and include it if present
            var entityRef = new EntityReference(tableName, id);
            var namePropertyInfo = psObject.Properties.FirstOrDefault(p => p.Name.Equals("Name", StringComparison.OrdinalIgnoreCase));
            if (namePropertyInfo != null && namePropertyInfo.Value != null)
            {
                entityRef.Name = namePropertyInfo.Value.ToString();
            }

            return entityRef;
        }

        /// <summary>
        /// Converts an OrganizationResponse to a PSObject with converted property values.
        /// </summary>
        /// <param name="response">The OrganizationResponse to convert.</param>
        /// <returns>A PSObject with properties converted using entity converter conventions.</returns>
        public PSObject ConvertResponseToPSObject(OrganizationResponse response)
        {
            if (response == null)
                return null;

            PSObject result = new PSObject();

            // Convert each property in the Results collection
            foreach (var kvp in response.Results)
            {
                string propertyName = kvp.Key;
                object propertyValue = kvp.Value;

                // Convert the value using the appropriate converter
                object convertedValue = ConvertResponsePropertyValue(propertyValue);
                result.Properties.Add(new PSNoteProperty(propertyName, convertedValue));
            }

            return result;
        }

        /// <summary>
        /// Converts a response property value to a PowerShell-friendly format.
        /// </summary>
        /// <remarks>
        /// Note: This method retrieves all columns for Entity conversions and may perform
        /// additional service calls to resolve EntityReference names. This is intentional
        /// to provide user-friendly display values in responses, but may impact performance
        /// for responses with many entities or entity references.
        /// </remarks>
        private object ConvertResponsePropertyValue(object value)
        {
            if (value == null)
                return null;

            // Handle Entity
            if (value is Entity entity)
            {
                // Get all columns for the entity
                EntityMetadata entityMetadata = entityMetadataFactory.GetLimitedMetadata(entity.LogicalName);
                ColumnSet columns = new ColumnSet(GetAllColumnNames(entityMetadata, false, null, false));
                return ConvertToPSObject(entity, columns, _ => ValueType.Display);
            }

            // Handle EntityCollection
            if (value is EntityCollection entityCollection)
            {
                List<PSObject> psObjects = new List<PSObject>();
                if (entityCollection.Entities.Any())
                {
                    EntityMetadata entityMetadata = entityMetadataFactory.GetLimitedMetadata(entityCollection.Entities.First().LogicalName);
                    ColumnSet columns = new ColumnSet(GetAllColumnNames(entityMetadata, false, null, false));

                    foreach (Entity e in entityCollection.Entities)
                    {
                        psObjects.Add(ConvertToPSObject(e, columns, _ => ValueType.Display));
                    }
                }
                return psObjects.ToArray();
            }

            // Handle EntityReference
            if (value is EntityReference entityRef)
            {
                // Try to get the name if not present
                if (string.IsNullOrEmpty(entityRef.Name) && entityRef.Id != Guid.Empty)
                {
                    try
                    {
                        EntityMetadata refEntityMetadata = entityMetadataFactory.GetLimitedMetadata(entityRef.LogicalName);
                        string nameAttribute = refEntityMetadata.PrimaryNameAttribute;

                        if (!string.IsNullOrEmpty(nameAttribute))
                        {
                            string name = service.Retrieve(entityRef.LogicalName, entityRef.Id, new ColumnSet(nameAttribute))
                                .GetAttributeValue<string>(nameAttribute);

                            if (!string.IsNullOrEmpty(name))
                            {
                                return name;
                            }
                        }
                    }
                    catch
                    {
                        // If we can't resolve the name, return the entity reference as an object
                    }
                }
                else if (!string.IsNullOrEmpty(entityRef.Name))
                {
                    return entityRef.Name;
                }

                // Return as PSObject with Id, TableName, and Name (if available)
                return new DataverseEntityReference(entityRef);
            }

            // Handle OptionSetValue
            if (value is OptionSetValue optionSetValue)
            {
                // We can't convert to label without metadata, so return the numeric value
                return optionSetValue.Value;
            }

            // Handle Money
            if (value is Money money)
            {
                return money.Value;
            }

            // Handle arrays/collections of convertible types
            if (value is IEnumerable enumerable && !(value is string))
            {
                List<object> convertedList = new List<object>();
                foreach (object item in enumerable)
                {
                    convertedList.Add(ConvertResponsePropertyValue(item));
                }
                return convertedList.ToArray();
            }

            // Return primitive types and other values as-is
            return value;
        }
    }
}