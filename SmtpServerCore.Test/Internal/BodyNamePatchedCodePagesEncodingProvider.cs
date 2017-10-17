using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace SmtpServerCore.Test.Internal
{
    internal class BodyNamePatchedCodePagesEncodingProvider : EncodingProvider
    {
        private EncodingProvider InternalEncodingProvider { get; }

        private IReadOnlyDictionary<int, string> BodyNamePatches { get; }

        private Dictionary<int, Encoding> PatchedEncodings { get; } = new Dictionary<int, Encoding>();

        public BodyNamePatchedCodePagesEncodingProvider(EncodingProvider encodingProvider, IEnumerable<EncodingBodyNamePatch> bodyNamePatchList)
        {
            this.InternalEncodingProvider = encodingProvider;
            this.BodyNamePatches = bodyNamePatchList.ToDictionary(path => path.CodePage, patch => patch.BodyName);
        }

        public override Encoding GetEncoding(int codepage)
        {
            var encoding = this.InternalEncodingProvider.GetEncoding(codepage);
            return GetPatchedVersion(encoding);
        }

        public override Encoding GetEncoding(string name)
        {
            var encoding = this.InternalEncodingProvider.GetEncoding(name);
            return GetPatchedVersion(encoding);
        }

        private Encoding GetPatchedVersion(Encoding encoding)
        {
            if (encoding == null) return null;
            lock (PatchedEncodings)
            {
                if (PatchedEncodings.TryGetValue(encoding.CodePage, out var patchedEncoding))
                    return patchedEncoding;
                if (this.BodyNamePatches.TryGetValue(encoding.CodePage, out var bodyName))
                {
                    patchedEncoding = new BodyNamePatchedEncoding(encoding, bodyName);
                    this.PatchedEncodings.Add(patchedEncoding.CodePage, patchedEncoding);
                    return patchedEncoding;
                }
            }
            return encoding;
        }
    }
}
