using System;
using System.IO;
using System.Diagnostics;

namespace Doggo.Serialiser
{
    public class NopBinaryReader : BinaryReader
    {
        #region Fields
        private readonly Stream stream;
        public Stopwatch byteSw = new Stopwatch();
        public Stopwatch intSw = new Stopwatch();
        public Stopwatch floatSw = new Stopwatch();
        public Stopwatch stringSw = new Stopwatch();
        public Stopwatch boolSw = new Stopwatch();

        #endregion

        #region Constructor
        public NopBinaryReader(Stream stream) : base(stream)
        {
            this.stream = stream;
        }
        #endregion

        #region Methods
        public override bool ReadBoolean()
        {
            boolSw.Start();
            var result = base.ReadBoolean();
            boolSw.Stop();
            return result;
        }
        public override byte ReadByte()
        {
            byteSw.Start();
            var result = base.ReadByte();
            byteSw.Stop();
            return result;
        }
        public override int ReadInt32()
        {
            intSw.Start();
            var result = base.ReadInt32();
            intSw.Stop();
            return result;
        }
        public override float ReadSingle()
        {
            floatSw.Start();
            var result = base.ReadSingle();
            floatSw.Stop();
            return result;
        }
        public override string ReadString()
        {
            stringSw.Start();
            var result = base.ReadString();
            stringSw.Stop();
            return result;
        }
        // public string ReadString()
        // {
        //     return "";
        // }

        #endregion
    }
}