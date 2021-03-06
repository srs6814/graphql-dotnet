using System;
using System.Linq;
using GraphQL.Types;

namespace GraphQL.Utilities
{
    /// <summary>
    /// This visitor verifies the correct application of directives to the schema elements.
    /// </summary>
    internal sealed class AppliedDirectivesValidationVisitor : ISchemaNodeVisitor
    {
        internal static readonly AppliedDirectivesValidationVisitor Instance = new AppliedDirectivesValidationVisitor();

        public void VisitFieldArgumentDefinition(QueryArgument argument, ISchema schema) => ValidateAppliedDirectives(argument, schema, DirectiveLocation.ArgumentDefinition);

        public void VisitDirectiveArgumentDefinition(QueryArgument argument, ISchema schema) => ValidateAppliedDirectives(argument, schema, DirectiveLocation.ArgumentDefinition);

        public void VisitEnum(EnumerationGraphType type, ISchema schema) => ValidateAppliedDirectives(type, schema, DirectiveLocation.Enum);

        public void VisitDirective(DirectiveGraphType directive, ISchema schema) => ValidateAppliedDirectives(directive, schema, null); // no location for directives (yet), see https://github.com/graphql/graphql-spec/issues/818

        public void VisitEnumValue(EnumValueDefinition value, ISchema schema) => ValidateAppliedDirectives(value, schema, DirectiveLocation.EnumValue);

        public void VisitFieldDefinition(FieldType field, ISchema schema) => ValidateAppliedDirectives(field, schema, DirectiveLocation.FieldDefinition);

        public void VisitInputFieldDefinition(FieldType field, ISchema schema) => ValidateAppliedDirectives(field, schema, DirectiveLocation.InputFieldDefinition);

        public void VisitInputObject(IInputObjectGraphType type, ISchema schema) => ValidateAppliedDirectives(type, schema, DirectiveLocation.InputObject);

        public void VisitInterface(IInterfaceGraphType iface, ISchema schema) => ValidateAppliedDirectives(iface, schema, DirectiveLocation.Interface);

        public void VisitObject(IObjectGraphType type, ISchema schema) => ValidateAppliedDirectives(type, schema, DirectiveLocation.Object);

        public void VisitScalar(ScalarGraphType scalar, ISchema schema) => ValidateAppliedDirectives(scalar, schema, DirectiveLocation.Scalar);

        public void VisitSchema(ISchema schema) => ValidateAppliedDirectives(schema, schema, DirectiveLocation.Schema);

        public void VisitUnion(UnionGraphType union, ISchema schema) => ValidateAppliedDirectives(union, schema, DirectiveLocation.Union);

        private void ValidateAppliedDirectives(IProvideMetadata provider, ISchema schema, DirectiveLocation? location) //TODO: add check for argument value type
        {
            if (provider.HasAppliedDirectives())
            {
                var applied = provider.GetAppliedDirectives().List;

                foreach (var appliedDirective in applied)
                {
                    var schemaDirective = schema.Directives.Find(appliedDirective.Name);
                    if (schemaDirective == null)
                        throw new InvalidOperationException($"Unknown directive '{appliedDirective.Name}'.");

                    if (location != null && !schemaDirective.Locations.Contains(location.Value))
                        throw new InvalidOperationException($"Directive '{schemaDirective.Name}' is applied in the wrong location '{location.Value}'. Allowed locations: {string.Join(", ", schemaDirective.Locations)}");

                    if (schemaDirective.Arguments?.Count > 0)
                    {
                        foreach (var arg in schemaDirective.Arguments.List)
                        {
                            if (arg.DefaultValue == null && appliedDirective.FindArgument(arg.Name) == null)
                                throw new InvalidOperationException($"Directive '{appliedDirective.Name}' must specify required argument '{arg.Name}'.");
                        }
                    }

                    if (appliedDirective.Arguments?.Count > 0)
                    {
                        foreach (var arg in appliedDirective.Arguments)
                        {
                            if (schemaDirective.Arguments.Find(arg.Name) == null)
                                throw new InvalidOperationException($"Unknown directive argument '{arg.Name}' for directive '{appliedDirective.Name}'.");
                        }
                    }
                }

                foreach (var directive in schema.Directives.List)
                {
                    if (!directive.Repeatable && applied.Count(applied => applied.Name == directive.Name) > 1)
                        throw new InvalidOperationException($"Non-repeatable directive '{directive.Name}' is applied to element '{provider.GetType().Name}' more than one time.");
                }
            }
        }
    }
}
