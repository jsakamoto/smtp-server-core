using System;
using System.Text;

namespace SmtpServerCore.Test.Internal
{
    internal class BodyNamePatchedEncoding : Encoding
    {
        private Encoding InternalEncoding { get; }

        private string _BodyName;

        public BodyNamePatchedEncoding(Encoding encoding, string bodyName)
            : base(encoding.CodePage, encoding.EncoderFallback, encoding.DecoderFallback)
        {
            this.InternalEncoding = encoding;
            this._BodyName = bodyName;
        }

        public override string BodyName => this._BodyName ?? this.InternalEncoding.BodyName;

        public override string HeaderName => this._BodyName ?? this.InternalEncoding.HeaderName;

        public override int CodePage => this.InternalEncoding.CodePage;

        public override string EncodingName => this.InternalEncoding.EncodingName;

        public override bool IsBrowserSave => this.InternalEncoding.IsBrowserSave;

        public override bool IsBrowserDisplay => this.InternalEncoding.IsBrowserDisplay;

        public override bool IsMailNewsDisplay => this.InternalEncoding.IsMailNewsDisplay;

        public override bool IsMailNewsSave => this.InternalEncoding.IsMailNewsSave;

        public override bool IsSingleByte => this.InternalEncoding.IsSingleByte;

        public override string WebName => this.InternalEncoding.WebName;

        public override int WindowsCodePage => this.InternalEncoding.WindowsCodePage;

        public override int GetByteCount(char[] chars, int index, int count) => this.InternalEncoding.GetByteCount(chars, index, count);

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) => this.InternalEncoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);

        public override int GetCharCount(byte[] bytes, int index, int count) => this.InternalEncoding.GetCharCount(bytes, index, count);

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) => this.InternalEncoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);

        public override int GetMaxByteCount(int charCount) => this.InternalEncoding.GetMaxByteCount(charCount);

        public override int GetMaxCharCount(int byteCount) => this.InternalEncoding.GetMaxCharCount(byteCount);

        public override object Clone() => new BodyNamePatchedEncoding(this.InternalEncoding.Clone() as Encoding, this.BodyName);

        public override int GetByteCount(char[] chars) => this.InternalEncoding.GetByteCount(chars);

        public override int GetByteCount(string s) => this.InternalEncoding.GetByteCount(s);

        public override unsafe int GetByteCount(char* chars, int count) => this.InternalEncoding.GetByteCount(chars, count);

        public override byte[] GetBytes(char[] chars) => this.InternalEncoding.GetBytes(chars);

        public override byte[] GetBytes(char[] chars, int index, int count) => this.InternalEncoding.GetBytes(chars, index, count);

        public override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount) => this.InternalEncoding.GetBytes(chars, charCount, bytes, byteCount);

        public override byte[] GetBytes(string s) => this.InternalEncoding.GetBytes(s);

        public override int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex) => this.InternalEncoding.GetBytes(s, charIndex, charCount, bytes, byteIndex);

        public override int GetCharCount(byte[] bytes) => this.InternalEncoding.GetCharCount(bytes);

        public override unsafe int GetCharCount(byte* bytes, int count) => this.InternalEncoding.GetCharCount(bytes, count);

        public override char[] GetChars(byte[] bytes) => this.InternalEncoding.GetChars(bytes);

        public override char[] GetChars(byte[] bytes, int index, int count) => this.InternalEncoding.GetChars(bytes, index, count);

        public override unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount) => this.InternalEncoding.GetChars(bytes, byteCount, chars, charCount);

        public override string GetString(byte[] bytes) => this.InternalEncoding.GetString(bytes);

        public override string GetString(byte[] bytes, int index, int count) => this.InternalEncoding.GetString(bytes, index, count);

        public override Decoder GetDecoder() => this.InternalEncoding.GetDecoder();

        public override Encoder GetEncoder() => this.InternalEncoding.GetEncoder();

        public override int GetHashCode() => this.InternalEncoding.GetHashCode();

        public override byte[] GetPreamble() => this.InternalEncoding.GetPreamble();

        public override bool IsAlwaysNormalized(NormalizationForm form) => this.InternalEncoding.IsAlwaysNormalized(form);

        public override string ToString() => this.InternalEncoding.ToString();
    }
}
