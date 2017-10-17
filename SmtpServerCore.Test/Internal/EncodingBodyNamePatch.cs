using System;

namespace SmtpServerCore.Test.Internal
{
    internal class EncodingBodyNamePatch
    {
        public int CodePage { get; }

        public string BodyName { get; }

        public EncodingBodyNamePatch(int codePage, string bodyName)
        {
            this.CodePage = codePage;
            this.BodyName = bodyName;
        }
    }

}
