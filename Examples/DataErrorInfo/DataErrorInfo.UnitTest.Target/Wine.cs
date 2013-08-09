using System.ComponentModel.DataAnnotations;

namespace DataErrorInfo.UnitTest.Target
{
    [DataErrorInfo]
    public class Wine
    {
        public const string ShouldHaveACru = @"A respectable wine should have a cru.";

        [Required(ErrorMessage = ShouldHaveACru)]
        public string Cru { get; set; }

        public int Millesime { get; set; }
    }
}
