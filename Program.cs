//Written for games in the Heat Engine.
//AirMech https://store.steampowered.com/app/206500
//AirMech Wastelands https://store.steampowered.com/app/595770
//AirMech Command https://store.steampowered.com/app/364630
using System.IO;
using System.IO.Compression;

namespace Heat_Extractor
{
    class Program
    {
        public static BinaryReader br;
        static MemoryStream nameTable = new();
        static void Main(string[] args)
        {
            br = new(File.OpenRead(args[0]));

            if (new string(br.ReadChars(4)) != "HPK\0")
                throw new System.Exception("This is not a Heat Engine HPK file.");

            br.ReadInt32();//1
            int fileCount = br.ReadInt32();
            int filesStart = br.ReadInt32();
            br.ReadInt32();//1
            br.ReadInt32();
            int nameSize = br.ReadInt32();

            br.ReadInt16();
            using (var ds = new DeflateStream(new MemoryStream(br.ReadBytes(nameSize - 2)), CompressionMode.Decompress))
                ds.CopyTo(nameTable);

            System.Collections.Generic.List<Subfile> subfiles = new();
            for (int i = 0; i < fileCount; i++)
                subfiles.Add(new());

            br.BaseStream.Position = filesStart;
            string path = Path.GetDirectoryName(args[0]) + "//" + Path.GetFileNameWithoutExtension(args[0]);
            foreach (Subfile file in subfiles)
            {
                Directory.CreateDirectory(path + "//" + Path.GetDirectoryName(file.name));
                FileStream fs = File.Create(path + "//" + file.name);
                if (file.sizeCompressed == file.sizeUncompressed)
                {
                    BinaryWriter bw = new(fs);
                    bw.Write(br.ReadBytes(file.sizeUncompressed));
                }
                else
                {
                    br.ReadInt16();
                    using (var ds = new DeflateStream(new MemoryStream(br.ReadBytes(file.sizeCompressed - 2)), CompressionMode.Decompress))
                        ds.CopyTo(fs);
                }

                fs.Close();
            }
        }

        class Subfile
        {
            public int sizeCompressed = br.ReadInt32();
            public int start = br.ReadInt32();
            public int sizeUncompressed = br.ReadInt32();
            float unknown = br.ReadSingle();
            public string name = name(br.ReadInt32());
        }

        static string name(int start)
        {
            nameTable.Position = start;
            string name = "";
            byte x = (byte)nameTable.ReadByte();
            while (x != 0)
            {
                name += (char)x;
                x = (byte)nameTable.ReadByte();
            }
            return name;
        }
    }
}
