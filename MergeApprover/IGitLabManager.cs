namespace MergeApprover
{
    public interface IGitLabManager
    {
        /// <summary>
        /// Goes through any open merge request, checks to seee if the source and destination branches are
        /// allowable for auto-approval, and then approves those branches
        /// </summary>
        void ProcessMergeRequests();
    }
}
