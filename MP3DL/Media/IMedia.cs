using System;
using System.Drawing;

namespace MP3DL.Media
{
    public interface IMedia : IEquatable<IMedia>
    {
        string Name { get; }
        string Title { get; }
        string[] Authors { get; }
        string PrintedAuthors { get; }
        string FirstAuthor { get; }
        Image? Art { get; }
        uint Number { get; }
        string Year { get; }
        double Duration { get; }
        string ID { get; }
        bool IsVideo { get; }
        void SetTags(string Filename);
        Image GetArt();
    }
}
