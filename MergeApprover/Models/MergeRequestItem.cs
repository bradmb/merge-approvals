namespace MergeApprover.Models
{
    public class MergeRequestItem
    {
        public int Id { get; set; }

        public int IId { get; set; }

        public int Project_Id { get; set; }

        public string Target_Branch { get; set; }

        public string Source_Branch { get; set; }
    }
}
