#nullable enable

using System;

namespace TyDs
{
    public static class IdParser
    {
        public static bool TryParse(string? id, out Id? result)
        {
            if (id == null)
            {
                result = null;
                return false;
            }

            var segments = id.Split('-');

            if (segments.Length != 2)
            {
                result = null;
                return false;
            }

            var prefix = segments[0].ToLowerInvariant();
            var identifier = segments[1].ToLowerInvariant();

            var type = prefix switch
            {
                %SwitchBranches%
                _ => null
            };

            if (type == null)
            {
                result = null;
                return false;
            }

            result = Id.Create(type, identifier);
            return true;
        }

        public static TId Parse<TId>(string? id) where TId : Id
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!IdParser.TryParse(id, out var parsedId))
            {
                throw new InvalidOperationException("Invalid id.");
            }

            if (parsedId is not TId result)
            {
                throw new InvalidOperationException($"Id is not of type {typeof(TId).Name}.");
            }

            return result;
        }
    }
}