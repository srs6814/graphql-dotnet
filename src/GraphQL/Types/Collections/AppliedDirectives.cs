using System;
using System.Collections;
using System.Collections.Generic;

namespace GraphQL.Types
{
    /// <summary>
    /// A class that represents a list of directives applied to a schema element (type, field, argument, etc.).
    /// </summary>
    public class AppliedDirectives : IEnumerable<AppliedDirective>
    {
        internal List<AppliedDirective> List { get; private set; }

        /// <summary>
        /// Gets the count of applied directives.
        /// </summary>
        public int Count => List?.Count ?? 0;

        /// <summary>
        /// Adds directive to list.
        /// </summary>
        public void Add(AppliedDirective directive)
        {
            if (directive == null)
                throw new ArgumentNullException(nameof(directive));

            //TODO: rewrite for repeatable directives
            if (List != null && List.Contains(directive))
                throw new InvalidOperationException("Already exists");

            (List ??= new List<AppliedDirective>()).Add(directive);
        }

        /// <summary>
        /// Finds a directive by its name from the list.
        /// </summary>
        public AppliedDirective Find(string name)
        {
            // DO NOT USE LINQ ON HOT PATH
            if (List != null)
            {
                foreach (var arg in List)
                {
                    if (arg.Name == name)
                        return arg;
                }
            }

            return null;

        }
        /// <inheritdoc cref="IEnumerable.GetEnumerator"/>
        public IEnumerator<AppliedDirective> GetEnumerator() => (List ?? System.Linq.Enumerable.Empty<AppliedDirective>()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
