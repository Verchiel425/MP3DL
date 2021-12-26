using System.Collections.Generic;
using System.Drawing;

namespace MP3DL.Media
{
    public interface IMediaCollection<IMedia>
    {
        string Title { get; }
        string Author { get; }
        Image? Art { get; }
        string ID { get; }
        uint MediaCount { get; }
        List<IMedia> Medias { get; }
    }
}
