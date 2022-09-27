using MessagePack;

namespace EditFangs
{
    [MessagePackObject]
    public class FangData
    {
        [Key(0)]
        public float scaleL { get; set; } = 0.1f;
        [Key(1)]
        public float scaleR { get; set; } = 0.1f;
        [Key(2)]
        public float spacingL { get; set; } = 1f;
        [Key(3)]
        public float spacingR { get; set; } = 1f;

        public bool IsDefault()
        {
            return scaleL == 0.1f && scaleR == 0.1f && spacingL == 1f && spacingR == 1f;
        }

        public void Reset()
        {
            scaleL = 0.1f;
            scaleR = 0.1f;
            spacingL = 1f;
            spacingR = 1f;
        }
    }
}
