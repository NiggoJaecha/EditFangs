using MessagePack;

namespace EditFangs
{
    [MessagePackObject]
    public class FangData
    {
        [Key(0)]
        public float scaleL { get; set; }
        [Key(1)]
        public float scaleR { get; set; }
        [Key(2)]
        public float spacingL { get; set; }
        [Key(3)]
        public float spacingR { get; set; }
    }
}
